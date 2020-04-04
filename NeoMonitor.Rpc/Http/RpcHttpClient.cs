using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NeoMonitor.Rpc.Http
{
    public sealed class RpcHttpClient
    {
        private readonly HttpClient _httpClient;

        public RpcHttpClient(HttpClient httpClient, RpcHttpClientConfig config)
        {
            _httpClient = httpClient;
            Config = config;
        }

        public RpcHttpClientConfig Config { get; }

        public async Task<RpcResponse<T>> PostAsync<T>(string url, RpcRequestBody body)
        {
            FixRequestBody(body);
            string bodyStr = JsonSerializer.Serialize(body, Config.JsonSerializerOptions);
            var httpContent = new StringContent(bodyStr, Encoding.UTF8, "application/json");
            HttpResponseMessage response = null;
            try
            {
                response = await _httpClient.PostAsync(url, httpContent);
            }
            catch (HttpRequestException rex)
            {
                response?.Dispose();
                return new RpcResponse<T>(false) { ErrorMsg = (rex.InnerException ?? rex).Message };
            }
            catch (Exception ex)
            {
                response?.Dispose();
                return new RpcResponse<T>(false) { ErrorMsg = ex.Message };
            }
            if (!response.IsSuccessStatusCode)
            {
                return new RpcResponse<T>(false) { ErrorMsg = response.StatusCode.ToString() };
            }
            using var rspStream = await response.Content.ReadAsStreamAsync();
            RpcResponseBody<T> rspBody = null;
            try
            {
                rspBody = await JsonSerializer.DeserializeAsync<RpcResponseBody<T>>(rspStream, Config.JsonSerializerOptions);
            }
            catch (JsonException jex)
            {
#if DEBUG
                string rspBodyText = await response.Content.ReadAsStringAsync();
                Debug.WriteLine("JsonParseError: at[{0}]->{1}", nameof(RpcHttpClient), rspBodyText);
#endif
                return new RpcResponse<T>(false) { ErrorMsg = jex.Message };
            }
            return new RpcResponse<T>(true) { Body = rspBody };
        }

        private void FixRequestBody(RpcRequestBody body)
        {
            if (string.IsNullOrEmpty(body.JsonRpc))
            {
                body.JsonRpc = Config.ApiVersion.ToString(2);
            }
            if (body.Params is null)
            {
                body.Params = Array.Empty<object>();
            }
        }
    }
}