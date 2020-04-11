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
            _httpClient = client;
            _settings = settings.Value;
        }

        public async Task<IpCheckModel> GetLocationAsync(string ip)
        {
            string accessKey = _settings.AccessKey;
            StringBuilder sb = new StringBuilder(ip.Length + accessKey.Length + 13);
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