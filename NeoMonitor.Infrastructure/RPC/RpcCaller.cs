using System;
using System.Net;
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
		private static readonly HttpClient _httpClient = new HttpClient();

		private static readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings()
		{
			NullValueHandling = NullValueHandling.Ignore,
			DefaultValueHandling = DefaultValueHandling.Ignore
		};

		public static async Task<T> MakeRPCCallAsync<T>(string endpoint, string method = "getblockcount") where T : RPCBaseBody
		{
			var rpcRequest = new RPCRequestBody(method);
			using var response = await SendRPCCallAsync(HttpMethod.Post, endpoint, rpcRequest);
			if (!response.IsSuccessStatusCode)
			{
				return default;
			}
			string rspText = await response.Content.ReadAsStringAsync();
			var result = JsonTool.DeserializeObject<T>(rspText);
			return result;
		}

		private static async Task<HttpResponseMessage> SendRPCCallAsync(HttpMethod httpMethod, string endpoint, object rpcData)
		{
			var data = JsonConvert.SerializeObject(rpcData, _jsonSerializerSettings);
			var req = new HttpRequestMessage(httpMethod, endpoint)
			{
				Content = new StringContent(data, Encoding.UTF8, "application/json")
			};
			HttpResponseMessage response = null;
			try
			{
				response = await _httpClient.SendAsync(req);
			}
			catch (Exception)
			{
				response?.Dispose();
				response = new HttpResponseMessage(HttpStatusCode.BadRequest);
			}
			return response;
		}
	}
}