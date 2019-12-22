using System;
using System.Collections.Generic;
using System.Linq;
using NeoMonitor.Data;
using NeoMonitor.Data.Models;
using NodeMonitor.ViewModels;

namespace NodeMonitor.Cache
{
	public class NodeCache
	{
		private readonly NeoMonitorContext _ctx;

		public HashSet<NodeViewModel> NodeList { get; }

		public int ExceptionCount => ExceptionList.Count;

		public List<NodeException> ExceptionList { get; }

		public NodeCache(NeoMonitorContext ctx)
		{
			_ctx = ctx;
			NodeList = new HashSet<NodeViewModel>();
			ExceptionList = new List<NodeException>();
		}

		public List<NodeViewModel> GetRpcEnabledNodeList()
		{
			return NodeList.Where(n => "RPC".Equals(n.Type, StringComparison.OrdinalIgnoreCase)).ToList();
		}

		public void AddNodes(IEnumerable<NodeViewModel> nodeViewModels)
		{
			foreach (var nodeItem in nodeViewModels)
			{
				NodeList.Add(nodeItem);
			}
		}

		public void AddNodeExceptions(IEnumerable<NodeException> nodeException)
		{
			ExceptionList.AddRange(nodeException);
		}

		public void UpdateNodeExceptions(IEnumerable<NodeException> nodeException)
		{
			ExceptionList.Clear();
			ExceptionList.AddRange(nodeException);
		}
	}
}