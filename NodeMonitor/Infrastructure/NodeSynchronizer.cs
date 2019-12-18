using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper.QueryableExtensions;
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
		private NeoMonitorContext _ctx;
		private RPCNodeCaller _rPCNodeCaller;
		private LocationCaller _locationCaller;
		private int exceptionFilter { get; set; }

		public List<Node> CachedDbNodes;
		public List<NodeException> CachedDbNodeExceptions;

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
			exceptionFilter = int.Parse(_configuration.GetSection("ExceptionFilter").Value);

			UpdateDbCache();
		}

		public IEnumerable<T> GetCachedNodesAs<T>()
		{
			return CachedDbNodes.AsQueryable().ProjectTo<T>();
		}

		public IEnumerable<T> GetCachedNodeExceptionsAs<T>()
		{
			return CachedDbNodeExceptions.AsQueryable().ProjectTo<T>();
		}

		public async Task UpdateNodesInformation()
		{
			var dbNodes = _ctx.Nodes.ToList();
			foreach (var dbNode in dbNodes)
			{
				if (dbNode.Type == NodeAddressType.RPC)
				{
					var stopwatch = Stopwatch.StartNew();
					var height = await _rPCNodeCaller.GetNodeHeight(dbNode);
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

					var newVersion = await _rPCNodeCaller.GetNodeVersion(dbNode.Url);
					if (!string.IsNullOrEmpty(newVersion))
					{
						dbNode.Version = newVersion;
					}

					var newPeers = await _rPCNodeCaller.GetNodePeers(dbNode);
					if (newPeers != null)
					{
						dbNode.Peers = newPeers.Connected.Count();
					}

					var newMempool = await _rPCNodeCaller.GetNodeMemPool(dbNode);
					if (newMempool != null)
					{
						dbNode.MemoryPool = newMempool.Count;
					}

					await _locationCaller.UpdateNodeLocation(dbNode.Id);

					_ctx.Nodes.Update(dbNode);
					_ctx.SaveChanges();
				}
			}

			UpdateDbCache();
		}

		private void UpdateDbCache()
		{
			CachedDbNodes = _ctx.Nodes.Where(x => x.Type == NodeAddressType.RPC).ToList();

			var end = DateTime.Now;
			var start = end.AddMonths(-3);
			CachedDbNodeExceptions = _ctx.NodeExceptionList.Where(ex => ex.GenTime > start && ex.GenTime < end && ex.Intervals > exceptionFilter).ToList();
		}
	}
}