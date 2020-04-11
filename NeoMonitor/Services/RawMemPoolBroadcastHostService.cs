using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NeoMonitor.Abstractions.Caches;
using NeoMonitor.Abstractions.Clients.SignalR;
using NeoMonitor.Abstractions.ViewModels;
using NeoMonitor.Hubs;

namespace NeoMonitor.Services
{
    public sealed class RawMemPoolBroadcastHostService : BackgroundService
    {
        private readonly ILogger<RawMemPoolBroadcastHostService> _logger;

        private readonly IRawMemPoolDataCache _dataCache;
        private readonly IHubContext<NodeHub> _nodeHubContext;

        public RawMemPoolBroadcastHostService(
            ILogger<RawMemPoolBroadcastHostService> logger,
            IRawMemPoolDataCache rawMemPoolDataCache,
            IHubContext<NodeHub> nodeHubContext
            )
        {
            _logger = logger;
            _dataCache = rawMemPoolDataCache;
            _nodeHubContext = nodeHubContext;
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
                await Task.Delay(10 * 1000);
                var datas = await _dataCache.GetArrayAsync();
                var sizeInfo = datas.Select(p => new RawMemPoolSizeModel() { Id = p.NodeId, MemoryPool = p.Items.Count }).ToArray();
                var tasks = new List<Task>(datas.Length + 1);
                var sizeInfoTask = _nodeHubContext.Clients.Group(NodeHub.RawMemPoolSizeInfo_GroupName).SendAsync(nameof(INodeHubClient.UpdateRawMemPoolSizeInfo), sizeInfo);
                tasks.Add(sizeInfoTask);
                foreach (var data in datas)
                {
                    var temp = _nodeHubContext.Clients.Group(NodeHub.RawMemPoolItemsInfo_GroupNamePrefix + data.NodeId.ToString()).SendAsync(nameof(INodeHubClient.UpdateRawMemPoolItems), data.Items);
                    tasks.Add(temp);
                }
                await Task.WhenAll(tasks);
            }
        }
    }
}