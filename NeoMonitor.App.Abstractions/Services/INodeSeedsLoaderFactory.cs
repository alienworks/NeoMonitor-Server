namespace NeoMonitor.App.Abstractions.Services
{
    public interface INodeSeedsLoaderFactory
    {
        INodeSeedsLoader Build();
    }
}