using System;
using System.Text.Json;

namespace NeoMonitor.Rpc.Http
{
    public sealed class RpcHttpClientConfig
    {
        public Version ApiVersion { get; set; }

        public JsonSerializerOptions JsonSerializerOptions { get; set; }
    }
}