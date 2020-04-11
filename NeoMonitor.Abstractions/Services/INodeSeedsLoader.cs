using System.Collections.Generic;
using NeoMonitor.Abstractions.Models;

namespace NeoMonitor.Abstractions.Services
{
    public interface INodeSeedsLoader
    {
        List<Node> Load();
    }
}