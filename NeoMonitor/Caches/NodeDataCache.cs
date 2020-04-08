using System;
using System.Collections.Generic;
using System.Threading;
using NeoMonitor.Basics.Models;

namespace NeoMonitor.Caches
{
    public sealed class NodeDataCache
    {
        private Node[] _nodes = Array.Empty<Node>();
        private NodeException[] _nodeExceptions = Array.Empty<NodeException>();

        public Node[] Nodes => _nodes;

        public NodeException[] NodeExceptions => _nodeExceptions;

        public List<Node> CopyNodes() => new List<Node>(_nodes);

        public List<NodeException> CopyNodeExceptions() => new List<NodeException>(_nodeExceptions);

        public Node[] UpdateNodes(Node[] value)
        {
            if (value is null || value.Length < 1)
            {
                return Array.Empty<Node>();
            }
            return Interlocked.Exchange(ref _nodes, value);
        }

        public NodeException[] UpdateNodeExceptions(NodeException[] value)
        {
            if (value is null || value.Length < 1)
            {
                return Array.Empty<NodeException>();
            }
            return Interlocked.Exchange(ref _nodeExceptions, value);
        }
    }
}