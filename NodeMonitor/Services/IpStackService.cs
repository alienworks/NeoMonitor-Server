using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using NeoState.Common.Location;

namespace NodeMonitor.Services
{
	public sealed class IpStackService : ILocateIpService
	{
		public HttpClient Client { get; }

		/// <summary>
		/// TODO: should add into config?
		/// </summary>
		public string AccessKey { get; } = "86e45b940f615f26bba14dde0a002bc3";

		public IpStackService(HttpClient client)
		{
			client.BaseAddress = new Uri("http://api.ipstack.com/");
			client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
			client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");
			client.DefaultRequestHeaders.Add("Accept-Language", "zh-CN,zh;q=0.9");
			client.DefaultRequestHeaders.Add("DNT", "1");
			client.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
			client.DefaultRequestHeaders.Host = "api.ipstack.com";
			client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/79.0.3945.88 Safari/537.36");

			Client = client;
		}

		public async Task<IpCheckModel> GetLocationAsync(string ip)
		{
			StringBuilder sb = new StringBuilder(ip.Length + AccessKey.Length + 13);
			sb.Append('/');
			sb.Append(ip);
			sb.Append("?access_key=");
			sb.Append(AccessKey);
			string relativeUrl = sb.ToString();
			HttpResponseMessage response = null;
			try
			{
				response = await Client.GetAsync(relativeUrl);
			}
			catch
			{
				response?.Dispose();
				return null;
			}
			if (!response.IsSuccessStatusCode)
			{
				return null;
			}
			var result = await response.Content.ReadAsAsync<IpCheckModel>();
			return result;
		}
	}
}