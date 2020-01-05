using System;
using System.Text.Json.Serialization;

namespace NeoState.Common.RPC
{
    public class RPCRequestBody : RPCBaseBody
    {
        [JsonPropertyName("method")]
        public string Method { get; set; }

        [JsonPropertyName("params")]
        public string[] Params { get; set; }

        public RPCRequestBody() : this("getblockcount")
        {
        }

        public RPCRequestBody(string method)
        {
            Method = method;
            Params = Array.Empty<string>();
        }
    }
}