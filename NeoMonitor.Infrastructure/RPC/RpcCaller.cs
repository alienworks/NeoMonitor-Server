using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using NeoState.Common.RPC;
using Newtonsoft.Json;

namespace NeoMonitor.Infrastructure.RPC
{
	public class RpcCaller
	{
		private static readonly HttpClient _httpClient = new HttpClient();

		private static readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings()
		{
			NullValueHandling = NullValueHandling.Ignore,
			DefaultValueHandling = DefaultValueHandling.Ignore
		};

		public static async Task<T> MakeRPCCallAsync<T>(string endpoint, string method = "getblockcount") where T : RPCBaseBody
		{
			var rpcRequest = new RPCRequestBody(method);
			HttpResponseMessage response;
			try
			{
				response = await SendRPCCallAsync(HttpMethod.Post, endpoint, rpcRequest);
			}
			catch
			{
				return default;
			}
			if (response is null || !response.IsSuccessStatusCode)
			{
				return default;
			}
			string result = await response.Content.ReadAsStringAsync();
			var serializedResult = JsonConvert.DeserializeObject<T>(result);
			return serializedResult;
		}

		public static async Task<HttpResponseMessage> SendRPCCallAsync(HttpMethod httpMethod, string endpoint, object rpcData)
		{
			HttpResponseMessage response;
			try
			{
				var req = new HttpRequestMessage(httpMethod, $"{endpoint}");
				var data = JsonConvert.SerializeObject(rpcData, _jsonSerializerSettings);
				req.Content = new StringContent(data, Encoding.Default, "application/json");
				response = await _httpClient.SendAsync(req);
			}
			catch (Exception)
			{
				response = new HttpResponseMessage(HttpStatusCode.BadRequest);
			}
			return response;
		}
	}
}