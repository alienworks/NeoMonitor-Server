using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using NeoMonitor.Data;
using NeoMonitor.Data.Models;
using NeoState.Common;

namespace NodeMonitor.Infrastructure
{
	public class NodeSynchronizer
	{
		private readonly IConfiguration _configuration;
		private readonly NeoMonitorContext _ctx;
		private readonly RPCNodeCaller _rPCNodeCaller;
		private readonly LocationCaller _locationCaller;

		public int ExceptionFilter { get; }

		public List<Node> CachedDbNodes { get; private set; }

		public List<NodeException> CachedDbNodeExceptions { get; private set; }

		public NodeSynchronizer(IConfiguration configuration,
			NeoMonitorContext ctx,
			RPCNodeCaller rPCNodeCaller,
			LocationCaller locationCaller,
			IOptions<NetSettings> netsettings)
		{
			_configuration = configuration;
			_ctx = ctx;
			_rPCNodeCaller = rPCNodeCaller;
			_locationCaller = locationCaller;
			ExceptionFilter = _configuration.GetValue<int>("ExceptionFilter");

			UpdateDbCache();
		}

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
			var dbNodes = _ctx.Nodes.Where(n => n.Type == NodeAddressType.RPC).ToList();
			foreach (var dbNode in dbNodes)
			{
				var stopwatch = Stopwatch.StartNew();
				var height = await _rPCNodeCaller.GetNodeHeightAsync(dbNode);
				stopwatch.Stop();
				var latency = stopwatch.ElapsedMilliseconds;

				if (height.HasValue)
				{
					dbNode.latency = latency;
					if (!dbNode.Height.HasValue || height > dbNode.Height)
					{
						dbNode.Height = height;
					}
					else if (height == dbNode.Height)
					{
						var exception = _ctx.NodeExceptionList.SingleOrDefault(e => e.Url == dbNode.Url && e.ExceptionHeight == dbNode.Height);
						if (exception == null)
						{
							var interval = (int)Math.Round((DateTime.Now - dbNode.LastUpdateTime).TotalSeconds, 0);
							_ctx.NodeExceptionList.Add(new NodeException
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
							exception.Intervals = (int)Math.Round((DateTime.Now - exception.GenTime).TotalSeconds, 0);
						}
					}
				}
				else
				{
					dbNode.latency = -1;
				}
				dbNode.LastUpdateTime = DateTime.Now;

				var newVersion = await _rPCNodeCaller.GetNodeVersionAsync(dbNode);
				if (!string.IsNullOrEmpty(newVersion))
				{
					dbNode.Version = newVersion;
				}

				var newPeers = await _rPCNodeCaller.GetNodePeersAsync(dbNode);
				if (newPeers != null)
				{
					dbNode.Peers = newPeers.Connected.Count();
				}

				var newMempool = await _rPCNodeCaller.GetNodeMemPoolAsync(dbNode);
				if (newMempool != null)
				{
					dbNode.MemoryPool = newMempool.Count;
				}

				await _locationCaller.UpdateNodeAsync(dbNode.Id);

				_ctx.Nodes.Update(dbNode);
				_ctx.SaveChanges();
			}

			UpdateDbCache();
		}

		private void UpdateDbCache()
		{
			CachedDbNodes = _ctx.Nodes.Where(x => x.Type == NodeAddressType.RPC).ToList();

			DateTime end = DateTime.Now;
			DateTime start = end.AddMonths(-3);
			CachedDbNodeExceptions = _ctx.NodeExceptionList.Where(ex => ex.GenTime > start && ex.GenTime < end && ex.Intervals > ExceptionFilter).ToList();
		}
	}
}