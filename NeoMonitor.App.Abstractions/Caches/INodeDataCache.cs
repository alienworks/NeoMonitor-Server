using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NeoMonitor.App.Abstractions.Models;

namespace NeoMonitor.App.Abstractions.Caches
{
    public interface INodeDataCache
    {
        Task<Node[]> GetNodesAsync(Func<Node, bool> filter = null);

        Task<List<Node>> GetNodesCopyAsync(Func<Node, bool> filter = null);

        Task<Node[]> UpdateNodesAsync(Node[] value);

        Task<NodeException[]> GetNodeExceptionsAsync(Func<NodeException, bool> filter = null);

        Task<List<NodeException>> GetNodeExceptionsCopyAsync(Func<NodeException, bool> filter = null);

        Task<NodeException[]> UpdateNodeExceptionsAsync(NodeException[] value);
    }
}