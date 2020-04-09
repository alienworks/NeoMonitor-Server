using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using NeoMonitor.App.Abstractions.Clients;
using NeoMonitor.App.Abstractions.Services.Data;
using NeoMonitor.App.Abstractions.ViewModels;

namespace NeoMonitor.Hubs
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

            _datas = Array.Empty<RawMemPoolSizeModel>();
        }

        private volatile RawMemPoolSizeModel[] _datas;

        public IReadOnlyList<RawMemPoolSizeModel> Datas => _datas;

        public TaskStatus TaskStatus => _runningTask.Status;

        public void Cancel() => _runningTaskTokenSource.Cancel();

        private async Task UpdateDatasAsync()
        {
            while (true)
            {
                if (_runningTaskTokenSource.IsCancellationRequested)
                {
                    break;
                }
                await Task.Delay(10 * 1000);
                var latestDatas = await _dataLoader.LoadAsync().ConfigureAwait(false);
                string json = JsonSerializer.Serialize(latestDatas);
                _datas = latestDatas;
                await _hubContext.Clients.Group(nameof(NodeHub)).SendAsync(nameof(INodeClient.UpdateRawMemPoolInfos), json);
            }
        }
    }
}