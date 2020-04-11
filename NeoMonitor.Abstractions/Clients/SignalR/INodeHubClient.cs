using System.Collections.Generic;
using System.Threading.Tasks;

namespace NeoMonitor.Abstractions.Clients.SignalR
{
    public interface INodeHubClient : INeoMonitorHubClient
    {
        Task UpdateRawMemPoolSizeInfo(string json);

        Task UpdateRawMemPoolItems(IList<string> items);
    }
}