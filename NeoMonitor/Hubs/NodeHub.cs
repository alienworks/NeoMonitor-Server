using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using NeoMonitor.Abstractions.Caches;
using NeoMonitor.Abstractions.Clients.SignalR;

namespace NeoMonitor.Hubs
{
    public class NodeHub : Hub<INodeHubClient>
    {
        public const string NodesInfo_GroupName = "NodesInfo";
        public const string RawMemPoolSizeInfo_GroupName = "RawMemPoolSizeInfo";
        public const string RawMemPoolItemsInfo_GroupNamePrefix = "RawMemPoolItemsInfo_";

        private readonly IRawMemPoolDataCache _dataCache;

        public NodeHub(IRawMemPoolDataCache dataCache)
        {
            _dataCache = dataCache;
        }

        public async Task SubscribeNodesInfo()
        {
            string clientId = Context.ConnectionId;
            await Groups.AddToGroupAsync(clientId, NodesInfo_GroupName);
        }

        public async Task UnsubscribeNodesInfo()
        {
            string clientId = Context.ConnectionId;
            await Groups.RemoveFromGroupAsync(clientId, NodesInfo_GroupName);
        }

        public async Task SubscribeRawMemPoolSizeInfo()
        {
            string clientId = Context.ConnectionId;
            await Groups.AddToGroupAsync(clientId, RawMemPoolSizeInfo_GroupName);
        }

        public async Task UnsubscribeRawMemPoolSizeInfo()
        {
            string clientId = Context.ConnectionId;
            await Groups.RemoveFromGroupAsync(clientId, RawMemPoolSizeInfo_GroupName);
        }

        public async Task SubscribeRawMemPoolItemsInfo(string nodeIdStr)
        {
            var valid = await CheckNodeIdValidAsync(nodeIdStr);
            if (!valid)
            {
                Context.Abort();
            }
            string clientId = Context.ConnectionId;
            await Groups.AddToGroupAsync(clientId, RawMemPoolItemsInfo_GroupNamePrefix + nodeIdStr);
        }

        public async Task UnsubscribeRawMemPoolItemsInfo(string nodeIdStr)
        {
            var valid = await CheckNodeIdValidAsync(nodeIdStr);
            if (!valid)
            {
                return;
            }
            string clientId = Context.ConnectionId;
            await Groups.RemoveFromGroupAsync(clientId, RawMemPoolItemsInfo_GroupNamePrefix + nodeIdStr);
        }

        private async ValueTask<bool> CheckNodeIdValidAsync(string nodeIdStr)
        {
            if (string.IsNullOrEmpty(nodeIdStr))
            {
                return false;
            }
            if (!int.TryParse(nodeIdStr, out int nodeId) || nodeId < 1)
            {
                return false;
            }
            return await _dataCache.ContainsAsync(nodeId);
        }
    }
}