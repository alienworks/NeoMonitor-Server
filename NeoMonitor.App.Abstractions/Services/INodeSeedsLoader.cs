using System.Collections.Generic;
using NeoMonitor.App.Abstractions.Models;

namespace NeoMonitor.App.Abstractions.Services
{
    public interface INodeSeedsLoader
    {
        List<Node> Load();
    }
}