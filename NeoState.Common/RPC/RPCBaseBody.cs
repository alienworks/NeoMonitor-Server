using System.Text.Json.Serialization;

namespace NeoState.Common.RPC
{
    public class RPCBaseBody
    {
        [JsonPropertyName("jsonrpc")]
        public string Jsonrpc { get; set; } = "2.0";

        [JsonPropertyName("id")]
        public int Id { get; set; } = 1;
    }
}