using System.Text.Json.Serialization;

namespace NeoState.Common.RPC
{
    public class RPCResultGetVersion
    {
        [JsonPropertyName("port")]
        public uint Port { get; set; }

        [JsonPropertyName("nonce")]
        public uint Nonce { get; set; }

        [JsonPropertyName("useragent")]
        public string Useragent { get; set; }
    }
}