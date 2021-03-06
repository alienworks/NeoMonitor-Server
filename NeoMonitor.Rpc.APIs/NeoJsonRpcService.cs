﻿using System.Collections.Generic;
using System.Threading.Tasks;
using NeoMonitor.Rpc.APIs.Models;
using NeoMonitor.Rpc.Http;

namespace NeoMonitor.Rpc.APIs
{
    internal sealed class NeoJsonRpcService : INeoJsonRpcService
    {
        private readonly RpcHttpClient _rpcHttpClient;

        public NeoJsonRpcService(RpcHttpClient rpcHttpClient)
        {
            _rpcHttpClient = rpcHttpClient;
        }

        public async Task<int?> GetBlockCountAsync(string url, long id = 1)
        {
            var rsp = await _rpcHttpClient.PostAsync<int?>(url, new RpcRequestBody("getblockcount") { Id = id });
            return rsp.Success ? rsp.Body.Result : default;
        }

        public async Task<List<string>> GetRawMemPoolAsync(string url, long id = 1)
        {
            var rsp = await _rpcHttpClient.PostAsync<List<string>>(url, new RpcRequestBody("getrawmempool") { Id = id });
            return rsp.Success ? rsp.Body.Result : default;
        }

        public async Task<PeerModel> GetPeersAsync(string url, long id = 1)
        {
            var rsp = await _rpcHttpClient.PostAsync<PeerModel>(url, new RpcRequestBody("getpeers") { Id = id });
            return rsp.Success ? rsp.Body.Result : default;
        }

        public async Task<VersionModel> GetVersionAsync(string url, long id = 1)
        {
            var rsp = await _rpcHttpClient.PostAsync<VersionModel>(url, new RpcRequestBody("getversion") { Id = id });
            return rsp.Success ? rsp.Body.Result : default;
        }
    }
}