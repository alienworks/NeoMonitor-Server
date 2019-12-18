using NeoMonitor.Data.Models;
using NeoMonitor.Infrastructure.RPC;
using Newtonsoft.Json;
using NeoState.Common.RPC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace NodeMonitor.Infrastructure
{
    public class RPCNodeCaller
    {

        public async Task<int?> GetNodeHeight(Node node)
        {
            int? result = null;
            var httpResult = await this.MakeRPCCall<RPCResponseBody<int>>(node);
            if (httpResult?.Result > 0)
            {
                result = httpResult.Result;
            }

            return result;
        }

        public async Task<string> GetNodeVersion(string endpoint)
        {
            var result = await RpcCaller.MakeRPCCall<RPCResponseBody<RPCResultGetVersion>>(endpoint, "getversion");
            return result == null ? string.Empty : result.Result.Useragent;
        }

        public async Task<string> GetNodeVersion(Node node)
        {
            if (string.IsNullOrEmpty(node.Version))
            {
                var result = await MakeRPCCall<RPCResponseBody<RPCResultGetVersion>>(node, "getversion");
                if (result?.Result != null)
                {
                    return result.Result.Useragent;
                }
            }

            return node.Version;
        }

        public async Task<RPCPeersResponse> GetNodePeers(Node node)
        {
            var result = await this.MakeRPCCall<RPCResponseBody<RPCPeersResponse>>(node, "getpeers");
            return result?.Result;
        }

        public async Task<List<string>> GetNodeMemPool(Node node)
        {
            var result = await this.MakeRPCCall<RPCResponseBody<List<string>>>(node, "getrawmempool");
            return result?.Result;
        }

        private async Task<T> MakeRPCCall<T>(Node node, string method = "getblockcount")
        {
            try
            {
                HttpResponseMessage response = null;
                var rpcRequest = new RPCRequestBody(method);
                response = await RpcCaller.SendRPCCall(HttpMethod.Post, $"{node.Url}", rpcRequest);
              
                if (response != null && response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    var serializedResult = JsonConvert.DeserializeObject<T>(result);
                    return serializedResult;
                }
            }
            catch (Exception e)
            {

            }

            return default(T);
        }
    }
}
