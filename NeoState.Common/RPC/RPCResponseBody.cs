using Newtonsoft.Json;

namespace NeoState.Common.RPC
{
    public class RPCResponseBody<T> : RPCBaseBody
    {
        [JsonProperty(PropertyName = "result")]
        public T Result { get; set; }
    }
}
