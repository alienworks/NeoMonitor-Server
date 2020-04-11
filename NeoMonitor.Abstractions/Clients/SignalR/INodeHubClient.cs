using System.Collections.Generic;
using System.Threading.Tasks;
using NeoMonitor.Abstractions.ViewModels;

namespace NeoMonitor.Abstractions.Clients.SignalR
{
    public interface INodeHubClient : INeoMonitorHubClient
    {
        Task UpdateNodes(IList<NodeViewModel> nodes);

        Task UpdateRawMemPoolSizeInfo(string json);

        Task UpdateRawMemPoolItems(IList<string> items);
    }
}