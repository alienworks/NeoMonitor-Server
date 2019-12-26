﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
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

		private readonly LocationCaller _locationCaller;
		private readonly RPCNodeCaller _rPCNodeCaller;

		private readonly ConcurrentDictionary<int, Action<Node>> _nodeActionDict = new ConcurrentDictionary<int, Action<Node>>();
		private readonly ConcurrentDictionary<int, Action<NodeException>> _nodeExceptionActionDict = new ConcurrentDictionary<int, Action<NodeException>>();
		private readonly ConcurrentBag<Action<NeoMonitorContext>> _contextActions = new ConcurrentBag<Action<NeoMonitorContext>>();

		public NodeSynchronizer(IConfiguration configuration,
		IServiceScopeFactory scopeFactory,
		//ILogger<NodeSynchronizer> logger,
		LocationCaller locationCaller,
		RPCNodeCaller rPCNodeCaller
		//IOptions<NetSettings> netsettings,
		)
		{
			_configuration = configuration;
			_scopeFactory = scopeFactory;
			//_logger = logger;

			_locationCaller = locationCaller;
			_rPCNodeCaller = rPCNodeCaller;

			ExceptionFilter = _configuration.GetValue<int>("ExceptionFilter");

			UpdateDbCache();
		}

		public int ExceptionFilter { get; }

		public int ParallelDegree { get; } = 100;

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
			var dbNodes = dbCtx.Nodes.AsNoTracking().Where(n => n.Type == NodeAddressType.RPC).ToList();
			if (dbNodes.Count < 1)
			{
				return;
			}
			using var semaphore = new SemaphoreSlim(ParallelDegree, ParallelDegree);
			var tasks = dbNodes.Select(n => CreateActions_UpdateDbNodeAsync(n, semaphore)).ToArray();
			//foreach (var dbNode in dbNodes)
			//{
			//	await UpdateDbNodeAsync(dbCtx, dbNode);
			//}
			await Task.WhenAll(tasks);

			ExecuteActions(dbCtx);
			UpdateDbCache();
		}

		private void ExecuteActions(NeoMonitorContext ctx)
		{
			foreach (var item in _nodeActionDict)
			{
				var node = ctx.Nodes.FirstOrDefault(p => p.Id == item.Key);
				if (node != null)
				{
					item.Value.Invoke(node);
				}
			}
			foreach (var item in _nodeExceptionActionDict)
			{
				var node = ctx.NodeExceptionList.FirstOrDefault(p => p.Id == item.Key);
				if (node != null)
				{
					item.Value.Invoke(node);
				}
			}
			foreach (var item in _contextActions)
			{
				item.Invoke(ctx);
			}
			ctx.SaveChanges();
			ClearActionsCache();
		}

		private async Task CreateActions_UpdateDbNodeAsync(Node dbNode, SemaphoreSlim semaphore)
		{
			semaphore.Wait();
			using var scope = _scopeFactory.CreateScope();
			var scopedCtx = scope.ServiceProvider.GetRequiredService<NeoMonitorContext>();
			await CreateActions_UpdateNodeHeightAsync(scopedCtx, dbNode);

			int nodeId = dbNode.Id;
			var newVersion = await _rPCNodeCaller.GetNodeVersionAsync(dbNode);
			var peersRsp = await _rPCNodeCaller.GetNodePeersAsync(dbNode);
			var newMempool = await _rPCNodeCaller.GetNodeMemPoolAsync(dbNode);
			if (!dbNode.Latitude.HasValue || !dbNode.Longitude.HasValue)
			{
				var locModel = await _locationCaller.CheckIpCallAsync(dbNode.IP);
				semaphore.Release();
				if (locModel != null)
				{
					string flag = locModel.Flag;
					string countryName = locModel.CountryName;
					double lat = locModel.Latitude, lng = locModel.Longitude;
					AddOrUpdateAction(_nodeActionDict, nodeId, n =>
					{
						n.FlagUrl = flag;
						n.Location = countryName;
						n.Latitude = lat;
						n.Longitude = lng;
					});
				}
			}
			else
			{
				semaphore.Release();
			}

			if (!string.IsNullOrEmpty(newVersion))
			{
				AddOrUpdateAction(_nodeActionDict, nodeId, n => { n.Version = newVersion; });
			}
			if (peersRsp?.Connected != null)
			{
				int connectedCount = peersRsp.Connected.Count;
				AddOrUpdateAction(_nodeActionDict, nodeId, n => { n.Peers = connectedCount; });
			}
			if (newMempool != null)
			{
				int memPoolSize = newMempool.Count;
				AddOrUpdateAction(_nodeActionDict, nodeId, n => { n.MemoryPool = memPoolSize; });
			}
		}

		private async Task CreateActions_UpdateNodeHeightAsync(NeoMonitorContext scopedCtx, Node dbNode)
		{
			var sw = Stopwatch.StartNew();
			int? height = await _rPCNodeCaller.GetNodeHeightAsync(dbNode);
			sw.Stop();
			long latency = sw.ElapsedMilliseconds;
			int nodeId = dbNode.Id;
			if (height.HasValue)
			{
				if (!dbNode.Height.HasValue || height > dbNode.Height)
				{
					AddOrUpdateAction(_nodeActionDict, nodeId, n => { n.Latency = latency; n.Height = height; });
				}
				else if (height == dbNode.Height)
				{
					CreateActions_AddOrUpdateNodeException(scopedCtx, dbNode, latency);
				}
			}
			else
			{
				AddOrUpdateAction(_nodeActionDict, nodeId, n => { n.Latency = -1; n.LastUpdateTime = DateTime.Now; });
			}
		}

		private void CreateActions_AddOrUpdateNodeException(NeoMonitorContext scopedCtx, Node dbNode, long latency)
		{
			int nodeId = dbNode.Id;
			var nodeEx = scopedCtx.NodeExceptionList.AsNoTracking().FirstOrDefault(e => e.Url == dbNode.Url && e.ExceptionHeight == dbNode.Height);
			if (nodeEx is null)
			{
				int interval = (int)Math.Round((DateTime.Now - dbNode.LastUpdateTime).TotalSeconds, 0);
				string url = dbNode.Url;
				int height = dbNode.Height.Value;
				_contextActions.Add((ctx) =>
				{
					ctx.NodeExceptionList.Add(new NodeException
					{
						Url = url,
						ExceptionHeight = height,
						GenTime = DateTime.Now,
						Intervals = interval
					});
				});

				AddOrUpdateAction(_nodeActionDict, nodeId, n => { n.Latency = latency; n.ExceptionCount++; });
			}
			else
			{
				AddOrUpdateAction(_nodeActionDict, nodeId, n => { n.Latency = latency; });
				int nodeExId = nodeEx.Id;
				int intervals = (int)Math.Round((DateTime.Now - nodeEx.GenTime).TotalSeconds, 0);
				AddOrUpdateAction(_nodeExceptionActionDict, nodeExId, n => n.Intervals = intervals);
			}
		}

		private static void AddOrUpdateAction<T>(ConcurrentDictionary<int, Action<T>> dict, int id, Action<T> act) where T : class
		{
			dict.AddOrUpdate(id, act, (k, v) => n => { v.Invoke(n); act.Invoke(n); });
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

		private void ClearActionsCache()
		{
			_nodeActionDict.Clear();
			_nodeExceptionActionDict.Clear();
			_contextActions.Clear();
		}

		#region Backup

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

			if (!dbNode.Latitude.HasValue || !dbNode.Longitude.HasValue)
			{
				var locModel = await _locationCaller.CheckIpCallAsync(dbNode.IP);
				if (locModel != null)
				{
					dbNode.FlagUrl = locModel.Flag;
					dbNode.Location = locModel.CountryName;
					dbNode.Latitude = locModel.Latitude;
					dbNode.Longitude = locModel.Longitude;
				}
			}

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

		#endregion Backup
	}
}