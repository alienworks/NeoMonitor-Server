using Newtonsoft.Json;

namespace NeoState.Common.RPC
{
    public class RPCRequestBody : RPCBaseBody
    {
        [JsonProperty(PropertyName = "method")]
        public string Method { get; set; } = "getblockcount";
        [JsonProperty(PropertyName = "params")]
        public string[] Params { get; set; } = new string[] { };

        public RPCRequestBody() { }
        public RPCRequestBody(string method)
        {
            this.Method = method;
        }
    }
}
