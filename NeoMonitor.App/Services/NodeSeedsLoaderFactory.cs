using NeoMonitor.App.Abstractions.Services;

namespace NeoMonitor.App.Services
{
    public sealed class NodeSeedsLoaderFactory : INodeSeedsLoaderFactory
    {
        public INodeSeedsLoader Build()
        {
            return new NodeSeedsLoader();
        }
    }
}