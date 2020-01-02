using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using NeoState.Common.RPC;
using NeoState.Common.Tools;
using Newtonsoft.Json;

namespace NeoMonitor.Infrastructure.RPC
{
    public class RpcCaller
    {
        private static readonly HttpClient _httpClient = new HttpClient(new SocketsHttpHandler());

        private static readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings()
        {
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore
        };

        public static async Task<T> MakeRPCCallAsync<T>(string url, string method = "getblockcount") where T : RPCBaseBody
        {
            var rpcRequest = new RPCRequestBody(method);
            string rpcJson = JsonConvert.SerializeObject(rpcRequest, _jsonSerializerSettings);
            HttpResponseMessage response = null;
            try
            {
                response = await _httpClient.PostAsync(url, new StringContent(rpcJson, Encoding.UTF8, "application/json"));
            }
            catch
            {
                response?.Dispose();
                return default;
            }
            if (!response.IsSuccessStatusCode)
            {
                return default;
            }
            string rspText = await response.Content.ReadAsStringAsync();
            var result = JsonTool.DeserializeObject<T>(rspText);
            return result;
        }
    }
}