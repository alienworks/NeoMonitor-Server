using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NeoMonitor.Data.Models;
using NodeMonitor.Hubs;
using NodeMonitor.Infrastructure;
using NodeMonitor.ViewModels;

namespace NodeMonitor.Services
{
    public class NotificationService : BackgroundService
    {
        private readonly NodeSynchronizer _nodeSynchronizer;
        private readonly IHubContext<NodeHub> _nodeHub;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(NodeSynchronizer nodeSynchronizer, IHubContext<NodeHub> nodeHub, ILogger<NotificationService> logger)
        {
            _nodeSynchronizer = nodeSynchronizer;
            _nodeHub = nodeHub;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken cancelToken)
        {
            Stopwatch sw = new Stopwatch();
            while (!cancelToken.IsCancellationRequested)
            {
                _logger.LogDebug("[{0}] Syncing... ...", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                sw.Restart();
                await UpdateBlockCountAsync();
                sw.Stop();
                _logger.LogDebug("[{0}] UpdateBlockCountAsync: {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), sw.Elapsed.ToString());
                await Task.Delay(5000, cancelToken);
            }
        }

        private async Task UpdateBlockCountAsync()
        {
            await _nodeSynchronizer.UpdateNodesInformationAsync();
            var nodes = _nodeSynchronizer.GetCachedNodesAs<NodeViewModel>();
            var nodeExceptions = _nodeSynchronizer.GetCachedNodeExceptionsAs<NodeException>();
            if (nodeExceptions.Count > 0)
            {
                foreach (var node in nodes)
                {
                    node.ExceptionCount = nodeExceptions.Count(ex => ex.Url == node.Url);
                }
            }
            else
            {
                foreach (var node in nodes)
                {
                    node.ExceptionCount = 0;
                }
            }
            await _nodeHub.Clients.All.SendAsync("Receive", nodes);
        }
    }
}