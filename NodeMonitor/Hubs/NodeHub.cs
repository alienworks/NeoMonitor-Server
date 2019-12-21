using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace NodeMonitor.Hubs
{
	public interface INodeHub
	{
		Task SendAsync();
	}

	public class NodeHub : Hub<INodeHub>
	{
		public Task SendAsync() => Clients.All.SendAsync();
	}
}