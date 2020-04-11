using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NeoMonitor.App.Abstractions.Caches;
using NeoMonitor.App.Abstractions.Models;

namespace NeoMonitor.Caches
{
    internal sealed class NodeDataMemoryCache : INodeDataCache
    {
        private Node[] _nodes = Array.Empty<Node>();
        private NodeException[] _nodeExceptions = Array.Empty<NodeException>();

        public Task<Node[]> GetNodesAsync(Func<Node, bool> filter = null)
        {
            var nodes = _nodes;
            var result = filter is null
                ? nodes
                : (nodes.Length > 0
                ? Array.FindAll(nodes, n => filter(n))
                : nodes);
            return Task.FromResult(result);
        }

        public Task<List<Node>> GetNodesCopyAsync(Func<Node, bool> filter = null)
        {
            var result = filter is null
                ? new List<Node>(_nodes)
                : _nodes.Where(filter).ToList();
            return Task.FromResult(result);
        }

        public Task<Node[]> UpdateNodesAsync(Node[] value)
        {
            var result = value is null
                ? Array.Empty<Node>()
                : Interlocked.Exchange(ref _nodes, value);
            return Task.FromResult(result);
        }

        public Task<NodeException[]> GetNodeExceptionsAsync(Func<NodeException, bool> filter = null)
        {
            var nodeExps = _nodeExceptions;
            var result = filter is null
                ? nodeExps
                : (nodeExps.Length > 0
                ? Array.FindAll(nodeExps, n => filter(n))
                : nodeExps);
            return Task.FromResult(result);
        }

        public Task<List<NodeException>> GetNodeExceptionsCopyAsync(Func<NodeException, bool> filter = null)
        {
            var result = filter is null
             ? new List<NodeException>(_nodeExceptions)
             : _nodeExceptions.Where(filter).ToList();
            return Task.FromResult(result);
        }

        public Task<NodeException[]> UpdateNodeExceptionsAsync(NodeException[] value)
        {
            var result = value is null
                ? Array.Empty<NodeException>()
                : Interlocked.Exchange(ref _nodeExceptions, value);
            return Task.FromResult(result);
        }
    }
}