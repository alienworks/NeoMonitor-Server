using System;
using System.Collections.Generic;
using System.Linq;
using NeoMonitor.App.ViewModels;
using NeoMonitor.Basics.Models;

namespace NeoMonitor.Cache
{
    public class NodeCache
    {
        public HashSet<NodeViewModel> NodeList { get; }

        public int ExceptionCount => ExceptionList.Count;

        public List<NodeException> ExceptionList { get; }

        public NodeCache()
        {
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