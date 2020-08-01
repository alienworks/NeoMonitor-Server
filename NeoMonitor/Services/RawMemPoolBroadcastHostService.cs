using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NeoMonitor.Abstractions.Caches;
using NeoMonitor.Abstractions.Clients.SignalR;
using NeoMonitor.Abstractions.ViewModels;
using NeoMonitor.Configs;
using NeoMonitor.Hubs;

namespace NeoMonitor.Services
{
    public sealed class RawMemPoolBroadcastHostService : BackgroundService
    {
        private readonly ILogger<RawMemPoolBroadcastHostService> _logger;

        private readonly IRawMemPoolDataCache _dataCache;
        private readonly IHubContext<NodeHub> _nodeHubContext;

        private readonly NodeSyncSettings _nodeSyncSettings;

        public RawMemPoolBroadcastHostService(
            ILogger<RawMemPoolBroadcastHostService> logger,
            IRawMemPoolDataCache rawMemPoolDataCache,
            IHubContext<NodeHub> nodeHubContext,
            IOptions<NodeSyncSettings> nodeSyncSettingsOption
            )
        {
            _logger = logger;
            _dataCache = rawMemPoolDataCache;
            _nodeHubContext = nodeHubContext;

            _nodeSyncSettings = nodeSyncSettingsOption.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogDebug("[Service]--> {0} Executing.", nameof(RawMemPoolBroadcastHostService));
            while (true)
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                await Task.Delay(Math.Max(1, _nodeSyncSettings.RawMemPoolBroadcastIntervalSeconds) * 1000);
                var datas = await _dataCache.GetArrayAsync();
                var sizeInfo = datas.Select(p => new RawMemPoolSizeModel() { Id = p.NodeId, MemoryPool = p.Items.Count }).ToArray();
                var tasks = new List<Task>(datas.Length + 1);
                var sizeInfoTask = _nodeHubContext.Clients.Group(NodeHub.RawMemPoolSizeInfo_GroupName).SendAsync(nameof(INodeHubClient.UpdateRawMemPoolSizeInfo), sizeInfo, cancellationToken: stoppingToken);
                tasks.Add(sizeInfoTask);
                foreach (var data in datas)
                {
                    var temp = _nodeHubContext.Clients.Group(NodeHub.RawMemPoolItemsInfo_GroupNamePrefix + data.NodeId.ToString()).SendAsync(nameof(INodeHubClient.UpdateRawMemPoolItems), data.Items, cancellationToken: stoppingToken);
                    tasks.Add(temp);
                }
                await Task.WhenAll(tasks);
            }
        }
    }
}