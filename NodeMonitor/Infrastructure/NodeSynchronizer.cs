using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
		private readonly IConfiguration _configuration;
		private readonly IServiceScopeFactory _scopeFactory;
		//private readonly ILogger<NodeSynchronizer> _logger;

		//private readonly LocationCaller _locationCaller;
		private readonly RPCNodeCaller _rPCNodeCaller;

		public NodeSynchronizer(IConfiguration configuration,
		IServiceScopeFactory scopeFactory,
		//IOptions<NetSettings> netsettings,
		//ILogger<NodeSynchronizer> logger,
		//NeoMonitorContext ctx,
		RPCNodeCaller rPCNodeCaller
		//LocationCaller locationCaller,
		)
		{
			_configuration = configuration;
			_scopeFactory = scopeFactory;
			//_logger = logger;
			_rPCNodeCaller = rPCNodeCaller;
			//_locationCaller = locationCaller;
			ExceptionFilter = _configuration.GetValue<int>("ExceptionFilter");

			UpdateDbCache();
		}

		public int ExceptionFilter { get; }

		public List<Node> CachedDbNodes { get; private set; }

		public List<NodeException> CachedDbNodeExceptions { get; private set; }

		public List<T> GetCachedNodesAs<T>()
		{
			var result = Mapper.Map<List<Node>, List<T>>(CachedDbNodes);
			return result;
		}

		public List<T> GetCachedNodeExceptionsAs<T>()
		{
			var result = Mapper.Map<List<NodeException>, List<T>>(CachedDbNodeExceptions);
			return result;
		}

		public async Task UpdateNodesInformationAsync()
		{
			using var scope = _scopeFactory.CreateScope();
			var dbCtx = scope.ServiceProvider.GetRequiredService<NeoMonitorContext>();
			var dbNodes = dbCtx.Nodes.Where(n => n.Type == NodeAddressType.RPC).ToList();
			if (dbNodes.Count < 1)
			{
				return;
			}
			foreach (var dbNode in dbNodes)
			{
				await UpdateDbNodeAsync(dbCtx, dbNode);
			}

			UpdateDbCache();
		}

		private async Task UpdateDbNodeAsync(NeoMonitorContext scopedCtx, Node dbNode)
		{
			await UpdateNodeHeightAsync(scopedCtx, dbNode);

			var newVersion = await _rPCNodeCaller.GetNodeVersionAsync(dbNode);
			if (!string.IsNullOrEmpty(newVersion))
			{
				dbNode.Version = newVersion;
			}

			var peersRsp = await _rPCNodeCaller.GetNodePeersAsync(dbNode);
			if (peersRsp?.Connected != null)
			{
				dbNode.Peers = peersRsp.Connected.Count;
			}

			var newMempool = await _rPCNodeCaller.GetNodeMemPoolAsync(dbNode);
			if (newMempool != null)
			{
				dbNode.MemoryPool = newMempool.Count;
			}

			var locationCaller = new LocationCaller(scopedCtx);
			await locationCaller.UpdateNodeAsync(dbNode.Id);

			scopedCtx.Nodes.Update(dbNode);
			scopedCtx.SaveChanges();
		}

		private async Task UpdateNodeHeightAsync(NeoMonitorContext scopedCtx, Node dbNode)
		{
			var sw = Stopwatch.StartNew();
			int? height = await _rPCNodeCaller.GetNodeHeightAsync(dbNode);
			sw.Stop();
			long latency = sw.ElapsedMilliseconds;
			if (height.HasValue)
			{
				dbNode.Latency = latency;
				if (!dbNode.Height.HasValue || height > dbNode.Height)
				{
					dbNode.Height = height;
				}
				else if (height == dbNode.Height)
				{
					AddOrUpdateNodeException(scopedCtx, dbNode);
				}
			}
			else
			{
				dbNode.Latency = -1;
			}
			dbNode.LastUpdateTime = DateTime.Now;
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

		private static void AddOrUpdateNodeException(NeoMonitorContext scopedCtx, Node dbNode)
		{
			var nodeEx = scopedCtx.NodeExceptionList.FirstOrDefault(e => e.Url == dbNode.Url && e.ExceptionHeight == dbNode.Height);
			if (nodeEx is null)
			{
				int interval = (int)Math.Round((DateTime.Now - dbNode.LastUpdateTime).TotalSeconds, 0);
				scopedCtx.NodeExceptionList.Add(new NodeException
				{
					Url = dbNode.Url,
					ExceptionHeight = dbNode.Height.Value,
					GenTime = DateTime.Now,
					Intervals = interval
				});
				dbNode.ExceptionCount++;
			}
			else
			{
				nodeEx.Intervals = (int)Math.Round((DateTime.Now - nodeEx.GenTime).TotalSeconds, 0);
			}
		}
	}
}