using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NeoState.Common.RPC
{
    public class RPCPeersResponse
    {
        [JsonPropertyName("unconnected")]
        public List<RPCPeer> Unconnected { get; set; }

        [JsonPropertyName("bad")]
        public List<RPCPeer> Bad { get; set; }

        [JsonPropertyName("connected")]
        public List<RPCPeer> Connected { get; set; }
    }
}