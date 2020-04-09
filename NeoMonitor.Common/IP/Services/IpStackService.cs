using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using NeoMonitor.Common.IP.Configs;
using NeoMonitor.Common.IP.Models;

namespace NeoMonitor.Common.IP.Services
{
    internal sealed class IpStackService : ILocateIpService
    {
        private readonly HttpClient _httpClient;

        private readonly IpStackSettings _settings;

        public IpStackService(HttpClient client, IOptions<IpStackSettings> settings)
        {
            client.BaseAddress = new Uri("http://api.ipstack.com/");
            client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
            client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");
            client.DefaultRequestHeaders.Add("Accept-Language", "zh-CN,zh;q=0.9");
            client.DefaultRequestHeaders.Add("DNT", "1");
            client.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
            client.DefaultRequestHeaders.Host = "api.ipstack.com";
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/81.0.4044.92 Safari/537.36");
            _httpClient = client;
            _settings = settings.Value;
        }

        public async Task<IpCheckModel> GetLocationAsync(string ip)
        {
            string accessKey = _settings.AccessKey;
            StringBuilder sb = new StringBuilder(ip.Length + accessKey + 13);
            sb.Append('/');
            sb.Append(ip);
            sb.Append("?access_key=");
            sb.Append(accessKey);
            string relativeUrl = sb.ToString();
            HttpResponseMessage response = null;
            try
            {
                response = await _httpClient.GetAsync(relativeUrl);
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
            byte[] bytes = await response.Content.ReadAsByteArrayAsync();
            var result = JsonSerializer.Deserialize<IpCheckModel>(bytes);
            return result;
        }
    }
}