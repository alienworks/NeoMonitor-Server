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

		public List<NodeViewModel> NodeList { get; }

		public int ExceptionCount => ExceptionList.Count;

		public List<NodeException> ExceptionList { get; }

		public NodeCache(NeoMonitorContext ctx)
		{
			_ctx = ctx;
			NodeList = new List<NodeViewModel>();
			ExceptionList = new List<NodeException>();
		}

		public List<NodeViewModel> GetRpcEnabledNodeList()
		{
			return NodeList.FindAll(n => "RPC".Equals(n.Type, StringComparison.OrdinalIgnoreCase));
		}

		public void UpdateNodes(IEnumerable<NodeViewModel> nodeViewModels)
		{
			NodeList.AddRange(nodeViewModels.Distinct(NodeViewModelEqualityComparer.Instance));
		}

		public void AddExceptions(ICollection<NodeException> nodeException)
		{
			ExceptionList.AddRange(nodeException);
		}

		public void UpdateExceptions(ICollection<NodeException> nodeException)
		{
			ExceptionList.Clear();
			ExceptionList.AddRange(nodeException);
		}
	}
}