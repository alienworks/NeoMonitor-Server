using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NeoMonitor.Data;
using NeoMonitor.Data.Models;
using NeoState.Common;

namespace NodeMonitor.Infrastructure
{
    public class NodeSynchronizer
    {
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IServiceScopeFactory _scopeFactory;
        //private readonly ILogger<NodeSynchronizer> _logger;

        private readonly LocationCaller _locationCaller;
        private readonly RPCNodeCaller _rPCNodeCaller;

        private readonly ConcurrentDictionary<int, Action<Node>> _nodeActionDict = new ConcurrentDictionary<int, Action<Node>>();
        private readonly ConcurrentDictionary<int, Action<NodeException>> _nodeExceptionActionDict = new ConcurrentDictionary<int, Action<NodeException>>();
        private readonly ConcurrentBag<Action<NeoMonitorContext>> _contextActions = new ConcurrentBag<Action<NeoMonitorContext>>();

        public NodeSynchronizer(IMapper mapper,
        IConfiguration configuration,
        IServiceScopeFactory scopeFactory,
        //ILogger<NodeSynchronizer> logger,
        LocationCaller locationCaller,
        RPCNodeCaller rPCNodeCaller
        //IOptions<NetSettings> netsettings,
        )
        {
            _mapper = mapper;
            _configuration = configuration;
            _scopeFactory = scopeFactory;
            //_logger = logger;

            _locationCaller = locationCaller;
            _rPCNodeCaller = rPCNodeCaller;

            ExceptionFilter = _configuration.GetValue<int>("ExceptionFilter");

            UpdateDbCache();
        }

        public int ExceptionFilter { get; }

        public int ParallelDegree { get; } = 50;

        public List<Node> CachedDbNodes { get; private set; }

        public List<NodeException> CachedDbNodeExceptions { get; private set; }

        public List<T> GetCachedNodesAs<T>()
        {
            var result = _mapper.Map<List<Node>, List<T>>(CachedDbNodes);
            return result;
        }

        public List<T> GetCachedNodeExceptionsAs<T>()
        {
            var result = _mapper.Map<List<NodeException>, List<T>>(CachedDbNodeExceptions);
            return result;
        }

        /// <summary>
        /// Update node info
        /// </summary>
        /// <returns></returns>
        /// <remarks>To save memory and be GC-friendly, it use the local collections for Action<T> cache. So don't use it in multi-thread environment, though it's thread-safe.</remarks>
        public async Task UpdateNodesInformationAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var dbCtx = scope.ServiceProvider.GetRequiredService<NeoMonitorContext>();
            var dbNodes = dbCtx.Nodes.AsNoTracking().Where(n => n.Type == NodeAddressType.RPC).ToList();
            if (dbNodes.Count < 1)
            {
                return;
            }
            using var semaphore = new SemaphoreSlim(ParallelDegree, ParallelDegree);
            var tasks = dbNodes.Select(n => CreateActions_UpdateDbNodeAsync(n, semaphore)).ToArray();
            await Task.WhenAll(tasks);

            ExecuteActions(dbCtx);
            UpdateDbCache();
        }

        private void ExecuteActions(NeoMonitorContext ctx)
        {
            foreach (var item in _nodeActionDict)
            {
                var node = ctx.Nodes.FirstOrDefault(p => p.Id == item.Key);
                if (node != null)
                {
                    item.Value.Invoke(node);
                }
            }
            foreach (var item in _nodeExceptionActionDict)
            {
                var node = ctx.NodeExceptionList.FirstOrDefault(p => p.Id == item.Key);
                if (node != null)
                {
                    item.Value.Invoke(node);
                }
            }
            foreach (var item in _contextActions)
            {
                item.Invoke(ctx);
            }
            ctx.SaveChanges();
            ClearActionsCache();
        }

        private async Task CreateActions_UpdateDbNodeAsync(Node dbNode, SemaphoreSlim semaphore)
        {
            semaphore.Wait();
            using var scope = _scopeFactory.CreateScope();
            var scopedCtx = scope.ServiceProvider.GetRequiredService<NeoMonitorContext>();

            Task heightTask = GetNodeHeightAsync(scopedCtx, dbNode);
            Task versionTask = GetNodeVersionAsync(dbNode);
            Task peersTask = GetNodePeersAsync(dbNode);
            Task mempoolTask = GetNodeMemPoolAsync(dbNode);
            await Task.WhenAll(heightTask, versionTask, peersTask, mempoolTask);

            //await GetNodeHeightAsync(scopedCtx, dbNode);
            //await GetNodeVersionAsync(dbNode);
            //await GetNodePeersAsync(dbNode);
            //await GetNodeMemPoolAsync(dbNode);

            if (!dbNode.Latitude.HasValue || !dbNode.Longitude.HasValue)
            {
                await GetNodeLocationAsync(dbNode);
            }

            semaphore.Release();
        }

        private async Task GetNodeHeightAsync(NeoMonitorContext scopedCtx, Node dbNode)
        {
            var sw = Stopwatch.StartNew();
            int? height = await _rPCNodeCaller.GetNodeHeightAsync(dbNode);
            sw.Stop();
            long latency = sw.ElapsedMilliseconds;
            int nodeId = dbNode.Id;
            if (height.HasValue)
            {
                if (!dbNode.Height.HasValue || height > dbNode.Height)
                {
                    AddOrUpdateAction(_nodeActionDict, nodeId, n => { n.Latency = latency; n.Height = height; });
                }
                else if (height == dbNode.Height)
                {
                    AddOrUpdateNodeException(scopedCtx, dbNode, latency);
                }
            }
            else
            {
                AddOrUpdateAction(_nodeActionDict, nodeId, n => { n.Latency = -1; n.LastUpdateTime = DateTime.Now; });
            }
        }

        private async Task GetNodeLocationAsync(Node dbNode)
        {
            var ipCheckModel = await _locationCaller.CheckIpCallAsync(dbNode.IP);
            if (ipCheckModel != null)
            {
                int nodeId = dbNode.Id;
                double lat = ipCheckModel.Latitude, lng = ipCheckModel.Longitude;
                string flag = ipCheckModel.Location.Flag;
                string countryName = ipCheckModel.CountryName;
                string locale = ipCheckModel.Location.Languages.FirstOrDefault()?.Code;
                AddOrUpdateAction(_nodeActionDict, nodeId, n =>
                {
                    n.FlagUrl = flag;
                    n.Location = countryName;
                    n.Latitude = lat;
                    n.Longitude = lng;
                    n.Locale = locale;
                });
            }
        }

        private async Task GetNodeVersionAsync(Node dbNode)
        {
            string newVersion = await _rPCNodeCaller.GetNodeVersionAsync(dbNode);
            if (!string.IsNullOrEmpty(newVersion))
            {
                int nodeId = dbNode.Id;
                AddOrUpdateAction(_nodeActionDict, nodeId, n => { n.Version = newVersion; });
            }
        }

        private async Task GetNodePeersAsync(Node dbNode)
        {
            var peersRsp = await _rPCNodeCaller.GetNodePeersAsync(dbNode);
            if (peersRsp?.Connected != null)
            {
                int nodeId = dbNode.Id;
                int connectedCount = peersRsp.Connected.Count;
                AddOrUpdateAction(_nodeActionDict, nodeId, n => { n.Peers = connectedCount; });
            }
        }

        private async Task GetNodeMemPoolAsync(Node dbNode)
        {
            var newMempool = await _rPCNodeCaller.GetNodeMemPoolAsync(dbNode);
            if (newMempool != null)
            {
                int nodeId = dbNode.Id;
                int memPoolSize = newMempool.Count;
                AddOrUpdateAction(_nodeActionDict, nodeId, n => { n.MemoryPool = memPoolSize; });
            }
        }

        private void AddOrUpdateNodeException(NeoMonitorContext scopedCtx, Node dbNode, long latency)
        {
            int nodeId = dbNode.Id;
            var nodeEx = scopedCtx.NodeExceptionList.AsNoTracking().FirstOrDefault(e => e.Url == dbNode.Url && e.ExceptionHeight == dbNode.Height);
            if (nodeEx is null)
            {
                int interval = (int)Math.Round((DateTime.Now - dbNode.LastUpdateTime).TotalSeconds, 0);
                string url = dbNode.Url;
                int height = dbNode.Height.Value;
                _contextActions.Add((ctx) =>
                {
                    ctx.NodeExceptionList.Add(new NodeException
                    {
                        Url = url,
                        ExceptionHeight = height,
                        GenTime = DateTime.Now,
                        Intervals = interval
                    });
                });

                AddOrUpdateAction(_nodeActionDict, nodeId, n => { n.Latency = latency; n.ExceptionCount++; });
            }
            else
            {
                AddOrUpdateAction(_nodeActionDict, nodeId, n => { n.Latency = latency; });
                int nodeExId = nodeEx.Id;
                int intervals = (int)Math.Round((DateTime.Now - nodeEx.GenTime).TotalSeconds, 0);
                AddOrUpdateAction(_nodeExceptionActionDict, nodeExId, n => n.Intervals = intervals);
            }
        }

        private void UpdateDbCache()
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<NeoMonitorContext>();
            CachedDbNodes = context.Nodes.AsNoTracking().Where(x => x.Type == NodeAddressType.RPC).ToList();
            DateTime end = DateTime.Now;
            DateTime start = end.AddMonths(-3);
            CachedDbNodeExceptions = context.NodeExceptionList.AsNoTracking().Where(ex => ex.GenTime > start && ex.GenTime < end && ex.Intervals > ExceptionFilter).ToList();
        }

        private void ClearActionsCache()
        {
            _nodeActionDict.Clear();
            _nodeExceptionActionDict.Clear();
            _contextActions.Clear();
        }

        private static void AddOrUpdateAction<T>(ConcurrentDictionary<int, Action<T>> dict, int id, Action<T> act) where T : class
        {
            dict.AddOrUpdate(id, act, (k, v) => n => { v.Invoke(n); act.Invoke(n); });
        }
    }
}