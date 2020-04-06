using System.Collections.Generic;
using NeoMonitor.Basics.Models;

namespace NeoMonitor.App.Abstractions.Services
{
    public interface INodeSeedsLoader
    {
        List<Node> Load();
    }
}