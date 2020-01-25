using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using NodeMonitor.Web.Abstraction.DataLoaders;
using NodeMonitor.Web.Abstraction.Hubs;
using NodeMonitor.Web.Abstraction.Models;

namespace NodeMonitor.Hubs
{
    public class NodeTicker
    {
        private readonly IRawMemPoolDataLoader _dataLoader;
        private readonly IHubContext<NodeHub> _hubContext;

        private readonly Task _runningTask;
        private readonly CancellationTokenSource _runningTaskTokenSource;

        public NodeTicker(IRawMemPoolDataLoader dataLoader, IHubContext<NodeHub> hubContext)
        {
            _dataLoader = dataLoader;
            _hubContext = hubContext;

            _runningTaskTokenSource = new CancellationTokenSource();
            _runningTask = Task.Factory.StartNew(UpdateDatasAsync, _runningTaskTokenSource.Token,
                TaskCreationOptions.LongRunning, TaskScheduler.Default);

            _datas = Array.Empty<RawMemPoolData>();
        }

        private volatile RawMemPoolData[] _datas;

        public TaskStatus TaskStatus => _runningTask.Status;

        public RawMemPoolData[] Datas => _datas;

        public void Cancel() => _runningTaskTokenSource.Cancel();

        private async Task UpdateDatasAsync()
        {
            while (true)
            {
                if (_runningTaskTokenSource.IsCancellationRequested)
                {
                    break;
                }
                await Task.Delay(5000);
                var latestDatas = await _dataLoader.LoadAsync().ConfigureAwait(false);
                string json = JsonSerializer.Serialize(latestDatas);
                _datas = latestDatas;
                await _hubContext.Clients.Group(nameof(NodeHub)).SendAsync(nameof(INodeClient.UpdateRawMemPoolInfos), json);
            }
        }
    }
}