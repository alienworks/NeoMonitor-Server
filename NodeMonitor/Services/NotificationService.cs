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

		public NotificationService(NodeSynchronizer nodeSynchronizer,
			IHubContext<NodeHub> nodeHub)
		{
			_nodeSynchronizer = nodeSynchronizer;
			_nodeHub = nodeHub;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while (!stoppingToken.IsCancellationRequested)
			{
				Console.WriteLine("Syncing... ...");
				try
				{
					await UpdateBlockCount();
				}
				catch (Exception e)
				{
					throw e;
				}

				await Task.Delay(5000, stoppingToken);
			}
		}

		private async Task UpdateBlockCount()
		{
			await _nodeSynchronizer.UpdateNodesInformation();
			var nodes = _nodeSynchronizer.GetCachedNodesAs<NodeViewModel>().ToList();
			var exceptions = _nodeSynchronizer.GetCachedNodeExceptionsAs<NodeException>().ToList();
			nodes.ForEach(node =>
			{
				node.ExceptionCount = exceptions.Where(ex => ex.Url == node.Url).ToList().Count();
			});
			await _nodeHub.Clients.All.SendAsync("Receive", nodes);
		}
	}
}