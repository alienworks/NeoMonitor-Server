using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NeoMonitor.Data;
using NodeMonitor.Web.Abstraction.Hubs;

namespace NodeMonitor.Hubs
{
    public class NodeHub : Hub<INodeCaller>
    {
        private readonly NeoMonitorContext _dbContext;

        public NodeHub(NeoMonitorContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task RequestTestAsync(string msg) => Clients.All.ShowMsgAsync(nameof(RequestTestAsync) + ':' + msg);

        public async Task GetRawMemPoolInfosByIdsAsync(IEnumerable<int> nodeIds)
        {
            if (nodeIds is null || !nodeIds.Any())
            {
                await Clients.All.ShowMsgAsync("NodeIds cannot be empty.");
                return;
            }
            var ids = nodeIds.ToHashSet();
            var nodes = _dbContext.Nodes.AsNoTracking().Where(n => ids.Contains(n.Id)).Select(n => new { n.Id, n.MemoryPool }).ToArray();
            string json = JsonSerializer.Serialize(nodes);
            await Clients.All.SendRawMemPoolInfosByIdsAsync(json);
        }
    }
}