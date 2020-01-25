using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using NodeMonitor.Web.Abstraction.Hubs;

namespace NodeMonitor.Hubs
{
    public class NodeHub : Hub<INodeClient>
    {
        private readonly NodeTicker _ticker;

        public NodeHub(NodeTicker ticker)
        {
            _ticker = ticker;
        }

        [HubMethodName("RequestTest")]
        public Task RequestTestAsync(string msg) => Clients.All.ShowServerMsg(nameof(RequestTestAsync) + "--> " + msg);

        [HubMethodName("GetRawMemPoolInfosByIds")]
        public async Task GetRawMemPoolInfosByIdsAsync(IEnumerable<int> nodeIds)
        {
            if (nodeIds is null || !nodeIds.Any())
            {
                await Clients.All.ShowServerMsg("NodeIds cannot be empty.");
                return;
            }
            var ids = nodeIds.ToHashSet();
            var datas = Array.FindAll(_ticker.Datas, d => ids.Contains(d.Id));
            string json = JsonSerializer.Serialize(datas);
            await Clients.All.ReceiveRawMemPoolInfosByIds(json);
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