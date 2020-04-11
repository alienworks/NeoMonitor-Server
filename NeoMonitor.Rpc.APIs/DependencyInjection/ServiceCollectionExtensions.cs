using NeoMonitor.Rpc.APIs;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static INeoRpcServiceBuilder AddNeoJsonRpcAPIs(this INeoRpcServiceBuilder builder)
        {
            builder.Services.AddSingleton<INeoJsonRpcService, NeoJsonRpcService>();
            return builder;
        }
    }
}