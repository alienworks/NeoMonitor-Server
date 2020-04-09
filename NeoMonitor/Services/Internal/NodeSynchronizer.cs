using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NeoMonitor.Basics;
using NeoMonitor.Basics.Models;
using NeoMonitor.Caches;
using NeoMonitor.Common.IP;
using NeoMonitor.Configs;
using NeoMonitor.DbContexts;
using NeoMonitor.Rpc.APIs;
using NeoMonitor.Shared.EntityFrameworkCore;

namespace NeoMonitor.Services.Internal
{
    public sealed class NodeSynchronizer
    {
        private readonly ILocateIpService _locateIpService;
        private readonly INeoJsonRpcService _rpcService;
        private readonly NodeSyncSettings _nodeSyncSettings;
        private readonly ScopedDbContextFactory _dbContextFactory;
        private readonly NodeDataCache _nodeDataCache;

        private readonly ConcurrentDictionary<int, Action<Node>> _nodeActionDict = new ConcurrentDictionary<int, Action<Node>>();
        private readonly ConcurrentDictionary<int, Action<NodeException>> _nodeExceptionActionDict = new ConcurrentDictionary<int, Action<NodeException>>();
        private readonly ConcurrentBag<Action<NeoMonitorContext>> _contextActions = new ConcurrentBag<Action<NeoMonitorContext>>();

        public NodeSynchronizer(
            IOptions<NodeSyncSettings> settingsOptions,
            ILocateIpService locationCaller,
            INeoJsonRpcService rPCNodeCaller,
            ScopedDbContextFactory scopeFactory,
            NodeDataCache nodeDataCache
        )
        {
            _locateIpService = locationCaller;
            _rpcService = rPCNodeCaller;
            _nodeSyncSettings = settingsOptions.Value;
            _dbContextFactory = scopeFactory;
            _nodeDataCache = nodeDataCache;
        }

        public Task StartAsync()
        {
            return SyncDbCacheAsync();
        }

        public async Task UpdateNodesInformationAsync()
        {
            // To save memory and be GC-friendly, it use the local collections for Action<T> cache.
            // So don't use it in multi-thread environment, even though it's thread-safe.
            using var dbCtxWrapper = _dbContextFactory.CreateDbContextScopedWrapper<NeoMonitorContext>();
            var dbCtx = dbCtxWrapper.Context;
            var dbNodes = await dbCtx.Nodes.AsNoTracking().Where(n => n.Type == NodeAddressType.RPC).ToListAsync();
            if (dbNodes.Count < 1)
            {
                return;
            }
            int parallelDegree = _nodeSyncSettings.ParallelDegree;
            using var semaphore = new SemaphoreSlim(parallelDegree, parallelDegree);
            var tasks = dbNodes.Select(n => CreateActions_UpdateDbNodeAsync(n, semaphore)).ToArray();
            await Task.WhenAll(tasks);

            await ExecuteActionsAsync(dbCtx);
            await SyncDbCacheAsync();
        }

        private async Task ExecuteActionsAsync(NeoMonitorContext ctx)
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
            await ctx.SaveChangesAsync();
            ClearActionsCache();
        }

        private async Task CreateActions_UpdateDbNodeAsync(Node dbNode, SemaphoreSlim semaphore)
        {
            semaphore.Wait();

            using var dbCtxWrapper = _dbContextFactory.CreateDbContextScopedWrapper<NeoMonitorContext>();
            var scopedCtx = dbCtxWrapper.Context;

            Task heightTask = GetNodeHeightAsync(scopedCtx, dbNode);
            Task versionTask = GetNodeVersionAsync(dbNode);
            Task peersTask = GetNodePeersAsync(dbNode);
            Task mempoolTask = GetNodeMemPoolAsync(dbNode);
            await Task.WhenAll(heightTask, versionTask, peersTask, mempoolTask);

            if (!dbNode.Latitude.HasValue || !dbNode.Longitude.HasValue)
            {
                await GetNodeLocationAsync(dbNode);
            }

            semaphore.Release();
        }

        private async Task GetNodeHeightAsync(NeoMonitorContext scopedCtx, Node dbNode)
        {
            var sw = Stopwatch.StartNew();
            int? height = await _rpcService.GetBlockCountAsync(dbNode.Url);
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
            var ipCheckModel = await _locateIpService.GetLocationAsync(dbNode.IP);
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
            var v = await _rpcService.GetVersionAsync(dbNode.Url);
            if (!string.IsNullOrEmpty(v?.UserAgent))
            {
                int nodeId = dbNode.Id;
                AddOrUpdateAction(_nodeActionDict, nodeId, n => { n.Version = v.UserAgent; });
            }
        }

        private async Task GetNodePeersAsync(Node dbNode)
        {
            var peersRsp = await _rpcService.GetPeersAsync(dbNode.Url);
            if (peersRsp?.Connected != null)
            {
                int nodeId = dbNode.Id;
                int connectedCount = peersRsp.Connected.Count;
                AddOrUpdateAction(_nodeActionDict, nodeId, n => { n.Peers = connectedCount; });
            }
        }

        private async Task GetNodeMemPoolAsync(Node dbNode)
        {
            var newMempool = await _rpcService.GetRawMemPoolAsync(dbNode.Url);
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

        private void ClearActionsCache()
        {
            _nodeActionDict.Clear();
            _nodeExceptionActionDict.Clear();
            _contextActions.Clear();
        }

        private async Task SyncDbCacheAsync()
        {
            using var dbCtxWrapper = _dbContextFactory.CreateDbContextScopedWrapper<NeoMonitorContext>();
            var dbCtx = dbCtxWrapper.Context;
            var nodes = await dbCtx.Nodes.AsNoTracking().Where(n => n.Type == NodeAddressType.RPC).ToArrayAsync();
            _nodeDataCache.UpdateNodes(nodes);
            DateTime end = DateTime.Now;
            DateTime start = end.AddMonths(-3);
            var nodeExps = await dbCtx.NodeExceptionList.AsNoTracking().Where(ex => ex.GenTime > start && ex.GenTime < end && ex.Intervals > _nodeSyncSettings.ExceptionFilter).ToArrayAsync();
            _nodeDataCache.UpdateNodeExceptions(nodeExps);
        }

        private static void AddOrUpdateAction<T>(ConcurrentDictionary<int, Action<T>> dict, int id, Action<T> act) where T : class
        {
            dict.AddOrUpdate(id, act, (k, v) => n => { v.Invoke(n); act.Invoke(n); });
        }
    }
}