namespace Microsoft.Extensions.DependencyInjection
{
    public interface INeoRpcServiceBuilder
    {
        public IServiceCollection Services { get; }
    }
}