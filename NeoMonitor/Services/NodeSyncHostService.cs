using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NeoMonitor.Abstractions.Caches;
using NeoMonitor.Abstractions.Clients.SignalR;
using NeoMonitor.Abstractions.ViewModels;
using NeoMonitor.Configs;
using NeoMonitor.Hubs;
using NeoMonitor.Services.Data;

namespace NeoMonitor.Services
{
    public sealed class NodeSyncHostService : BackgroundService
    {
        private readonly ILogger<NodeSyncHostService> _logger;
        private readonly IHubContext<NodeHub> _nodeHub;

        private readonly IMapper _mapper;

        private readonly NodeSynchronizer _nodeSynchronizer;
        private readonly INodeDataCache _nodeDataCache;

        private readonly NodeSyncSettings _nodeSyncSettings;

        public NodeSyncHostService(
            ILogger<NodeSyncHostService> logger,
            IHubContext<NodeHub> nodeHub,
            IMapper mapper,
            NodeSynchronizer nodeSynchronizer,
            INodeDataCache nodeDataCache,
            IOptions<NodeSyncSettings> nodeSyncSettingsOption
            )
        {
            _logger = logger;
            _nodeHub = nodeHub;
            _mapper = mapper;
            _nodeSynchronizer = nodeSynchronizer;
            _nodeDataCache = nodeDataCache;
            _nodeSyncSettings = nodeSyncSettingsOption.Value;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await _nodeSynchronizer.StartAsync();
            await base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken cancelToken)
        {
            _logger.LogInformation("[Service]--> {0} Executing.", nameof(NodeSyncHostService));
            Stopwatch sw = new Stopwatch();
            while (!cancelToken.IsCancellationRequested)
            {
                _logger.LogInformation("[{0}] Syncing... ...", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                sw.Restart();
                await _nodeSynchronizer.UpdateNodesInformationAsync();
                sw.Stop();
                _logger.LogInformation("[{0}] UpdateNodesPeriod: {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), sw.Elapsed.ToString());
                await BroadcastToClientsAsync(cancelToken);
                if (_nodeSyncSettings.NodeInfoSyncIntervalMilliseconds > 0)
                {
                    await Task.Delay(_nodeSyncSettings.NodeInfoSyncIntervalMilliseconds, cancelToken);
                    //GC.Collect();
                }
            }
        }

        private async Task BroadcastToClientsAsync(CancellationToken cancelToken)
        {
            var nodes = await _nodeDataCache.GetNodesAsync();
            var nodeExps = await _nodeDataCache.GetNodeExceptionsAsync();
            var nodeViews = _mapper.Map<NodeViewModel[]>(nodes);
            foreach (var n in nodeViews)
            {
                n.ExceptionCount = nodeExps.Count(e => e.Url == n.Url);
            }
            await _nodeHub.Clients.Group(NodeHub.NodesInfo_GroupName).SendAsync(nameof(INodeHubClient.UpdateNodes), nodeViews, cancellationToken: cancelToken);
        }
    }
}