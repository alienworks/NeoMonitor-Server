using System.Threading.Tasks;

namespace NeoMonitor.Abstractions.Clients.SignalR
{
    public interface INeoMonitorHubClient
    {
        Task ShowServerMsg(string msg);
    }
}