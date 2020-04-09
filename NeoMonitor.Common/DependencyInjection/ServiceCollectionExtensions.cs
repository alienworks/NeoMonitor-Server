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
                .AddHttpClient<ILocateIpService, IpStackService>();
            return services;
        }
    }
}