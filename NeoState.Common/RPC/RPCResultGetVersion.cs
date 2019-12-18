using Newtonsoft.Json;

namespace NeoState.Common.RPC
{
    public class RPCResultGetVersion
    {
        [JsonProperty(PropertyName = "port")]
        public uint Port { get; set; }
        [JsonProperty(PropertyName = "nonce")]
        public uint Nonce { get; set; }
        [JsonProperty(PropertyName = "useragent")]
        public string Useragent { get; set; }
    }
}
