using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using NeoMonitor.Abstractions.Clients;

namespace NeoMonitor.Hubs
{
    public class NodeHub : Hub<INodeClient>
    {
        private readonly NodeTicker _ticker;

        public NodeHub(NodeTicker ticker)
        {
            _ticker = ticker;
        }

        public override async Task OnConnectedAsync()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, nameof(NodeHub));
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, nameof(NodeHub));
            await base.OnDisconnectedAsync(exception);
        }
    }
}