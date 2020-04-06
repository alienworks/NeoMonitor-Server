using NeoMonitor.RpcAPIs;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static INeoRpcServiceBuilder AddNeoJsonRpcAPIs(this INeoRpcServiceBuilder builder)
        {
            builder.Services.AddTransient<INeoJsonRpcService, NeoJsonRpcService>();
            return builder;
        }
    }
}