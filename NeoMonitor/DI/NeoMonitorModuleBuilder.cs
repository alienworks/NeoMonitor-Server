namespace Microsoft.Extensions.DependencyInjection
{
    internal sealed class NeoMonitorModuleBuilder : INeoMonitorModuleBuilder
    {
        public NeoMonitorModuleBuilder(IServiceCollection services)
        {
            Services = services;
        }

        public IServiceCollection Services { get; }
    }
}