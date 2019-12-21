using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
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

		public NotificationService(NodeSynchronizer nodeSynchronizer, IHubContext<NodeHub> nodeHub)
		{
			_nodeSynchronizer = nodeSynchronizer;
			_nodeHub = nodeHub;
		}

		protected override async Task ExecuteAsync(CancellationToken cancelToken)
		{
			while (!cancelToken.IsCancellationRequested)
			{
				Console.WriteLine("Syncing... ...");
				try
				{
					await UpdateBlockCountAsync();
				}
				catch (Exception)
				{
					throw;
				}
				await Task.Delay(5000, cancelToken);
			}
		}

		private async Task UpdateBlockCountAsync()
		{
			await _nodeSynchronizer.UpdateNodesInformationAsync();
			var nodes = _nodeSynchronizer.GetCachedNodesAs<NodeViewModel>();
			var exceptions = _nodeSynchronizer.GetCachedNodeExceptionsAs<NodeException>();
			nodes.ForEach(node =>
			{
				node.ExceptionCount = exceptions.Count(ex => ex.Url == node.Url);
			});
			await _nodeHub.Clients.All.SendAsync("Receive", nodes);
		}
	}
}