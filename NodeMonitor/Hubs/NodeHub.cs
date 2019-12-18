using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

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