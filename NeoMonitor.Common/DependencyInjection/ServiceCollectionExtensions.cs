using System;
using System.Net;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using NeoMonitor.Common.IP;
using NeoMonitor.Common.IP.Configs;
using NeoMonitor.Common.IP.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddNeoCommonModule(this IServiceCollection services, IConfiguration config)
        {
            services
                .Configure<IpStackSettings>(config.GetSection(nameof(IpStackSettings)))
                .AddHttpClient<ILocateIpService, IpStackService>(client =>
                {
                    client.BaseAddress = new Uri("http://api.ipstack.com/");
                    client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
                    client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");
                    client.DefaultRequestHeaders.Add("Accept-Language", "zh-CN,zh;q=0.9");
                    client.DefaultRequestHeaders.Add("DNT", "1");
                    client.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
                    client.DefaultRequestHeaders.Host = "api.ipstack.com";
                    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/81.0.4044.92 Safari/537.36");
                })
                .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler() { AutomaticDecompression = DecompressionMethods.GZip });
            return services;
        }
    }
}