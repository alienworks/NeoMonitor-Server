using System.Text.Json.Serialization;

namespace NeoState.Common.RPC
{
    public class RPCResponseBody<T> : RPCBaseBody
    {
        [JsonPropertyName("result")]
        public T Result { get; set; }
    }
}