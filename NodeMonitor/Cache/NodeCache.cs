using NeoMonitor.Data;
using NeoMonitor.Data.Models;
using NodeMonitor.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace NodeMonitor.Cache
{
    public class NodeCache
    {
        private readonly NeoMonitorContext _ctx;

        public HashSet<NodeViewModel> NodeList { get; private set; }
        public int ExceptionCount => this.ExceptionList.Count;
        public List<NodeException> ExceptionList { get; set; }

        public IEnumerable<NodeViewModel> RpcEnabled => this.NodeList.Where(x => x.Type == "RPC").ToList();

        public NodeCache(NeoMonitorContext ctx)
        {
            _ctx = ctx;
            NodeList = new HashSet<NodeViewModel>();
            ExceptionList = new List<NodeException>();
        }

        public void UpdateNodes(IEnumerable<NodeViewModel> nodeViewModels)
        {
            foreach (var node in nodeViewModels)
            {
                NodeList.Add(node);
            }
        }

        public void AddExceptions(ICollection<NodeException> nodeException)
        {
            this.ExceptionList.AddRange(nodeException);
        }

        public void UpdateExceptions(ICollection<NodeException> nodeException)
        {
            this.ExceptionList.Clear();
            this.ExceptionList.AddRange(nodeException);
        }
    }
}
