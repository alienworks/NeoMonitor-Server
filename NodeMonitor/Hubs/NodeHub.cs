using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace NodeMonitor.Hubs
{
    public interface INodeHub
    {
        Task Send();
    }

    public class NodeHub : Hub<INodeHub>
    {
        public async Task Send() => await Clients.All.Send();
    }
}