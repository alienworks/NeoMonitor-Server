using System.Text.Json.Serialization;

namespace NeoState.Common.RPC
{
    public class RPCPeer
    {
        [JsonPropertyName("address")]
        public string Address { get; set; }

        [JsonPropertyName("port")]
        public uint Port { get; set; }
    }
}