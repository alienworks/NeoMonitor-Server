using System.Collections.Generic;
using System.Threading.Tasks;
using NeoMonitor.Data.Models;
using NeoMonitor.Infrastructure.RPC;
using NeoState.Common.RPC;

namespace NodeMonitor.Infrastructure
{
    public class RPCNodeCaller
    {
        public async Task<int?> GetNodeHeightAsync(Node node)
        {
            var rpcRsp = await MakeRPCCallAsync<RPCResponseBody<int>>(node);
            if (rpcRsp is null || rpcRsp.Result <= 0)
            {
                return null;
            }
            return rpcRsp.Result;
        }

        public async Task<string> GetNodeVersionAsync(Node node)
        {
            if (string.IsNullOrEmpty(node.Version))
            {
                var rpcRsp = await MakeRPCCallAsync<RPCResponseBody<RPCResultGetVersion>>(node, "getversion");
                if (rpcRsp?.Result != null)
                {
                    return rpcRsp.Result.Useragent;
                }
            }
            return node.Version;
        }

        public async Task<RPCPeersResponse> GetNodePeersAsync(Node node)
        {
            var result = await MakeRPCCallAsync<RPCResponseBody<RPCPeersResponse>>(node, "getpeers");
            return result?.Result;
        }

        public async Task<List<string>> GetNodeMemPoolAsync(Node node)
        {
            var result = await MakeRPCCallAsync<RPCResponseBody<List<string>>>(node, "getrawmempool");
            return result?.Result;
        }

        private Task<T> MakeRPCCallAsync<T>(Node node, string method = "getblockcount") where T : RPCBaseBody => RpcCaller.MakeRPCCallAsync<T>(node.Url, method);
    }
}