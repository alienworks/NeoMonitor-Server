using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using NeoState.Common.RPC;

namespace NeoMonitor.Infrastructure.RPC
{
    public class RpcCaller
    {
        private static readonly HttpClient _httpClient = new HttpClient(new SocketsHttpHandler());

        private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions()
        {
            AllowTrailingCommas = true,
            IgnoreNullValues = true,
            PropertyNameCaseInsensitive = true
        };

        public static async Task<T> MakeRPCCallAsync<T>(string url, string method = "getblockcount") where T : RPCBaseBody
        {
            var rpcRequest = new RPCRequestBody(method);
            string rpcJson = JsonSerializer.Serialize(rpcRequest, _jsonSerializerOptions);
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
            byte[] rspBytes = await response.Content.ReadAsByteArrayAsync();
            if (rspBytes is null || rspBytes.Length < 1)
            {
                return default;
            }
            T result;
            try
            {
                result = JsonSerializer.Deserialize<T>(rspBytes, _jsonSerializerOptions);
            }
            catch
            {
#if DEBUG
                string rspText = Encoding.UTF8.GetString(rspBytes);
                Debug.WriteLine("JsonParseError: at[{0}]->{1}", nameof(RpcCaller), rspText);
#endif
                return default;
            }
            return result;
        }
    }
}