namespace Microsoft.Extensions.DependencyInjection
{
    public interface INeoMonitorModuleBuilder
    {
        IServiceCollection Services { get; }
    }
}