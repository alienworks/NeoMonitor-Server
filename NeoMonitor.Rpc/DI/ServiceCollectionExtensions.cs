using System;
using System.Text.Json;
using NeoMonitor.Rpc.DependencyInjection;
using NeoMonitor.Rpc.Http;
using NeoMonitor.Shared.Text.Json;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static INeoRpcServiceBuilder AddNeoRpcHttpClient(this IServiceCollection services, Action<RpcHttpClientConfig> configure = null)
        {
            var config = new RpcHttpClientConfig()
            {
                ApiVersion = new Version(2, 0),
                JsonSerializerOptions = new JsonSerializerOptions()
                {
                    AllowTrailingCommas = true,
                    PropertyNamingPolicy = new LowCaseJsonNamingPolicy()
                }
            };
            configure?.Invoke(config);
            services
                .AddSingleton(config)
                .AddHttpClient<RpcHttpClient>()
                .ConfigureHttpClient(p =>
                {
                    if (config.DefaultTimeout.HasValue)
                    {
                        p.Timeout = config.DefaultTimeout.Value;
                    }
                });
            return new NeoRpcServiceBuilder(services);
        }
    }
}