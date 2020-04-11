using NeoMonitor.Abstractions.Services;

namespace NeoMonitor.Services.Seeds
{
    public sealed class NodeSeedsLoaderFactory : INodeSeedsLoaderFactory
    {
        public INodeSeedsLoader Build()
        {
            return new NodeSeedsLoader();
        }
    }
}