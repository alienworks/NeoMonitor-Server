using Microsoft.Extensions.DependencyInjection;

namespace NeoMonitor.Rpc.DependencyInjection
{
    internal sealed class NeoRpcServiceBuilder : INeoRpcServiceBuilder
    {
        public NeoRpcServiceBuilder(IServiceCollection services)
        {
            Services = services;
        }

        public IServiceCollection Services { get; }
    }
}