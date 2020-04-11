namespace NeoMonitor.Abstractions.Services
{
    public interface INodeSeedsLoaderFactory
    {
        INodeSeedsLoader Build();
    }
}