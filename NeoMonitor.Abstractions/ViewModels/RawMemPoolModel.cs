using System.Collections.Generic;

namespace NeoMonitor.Abstractions.ViewModels
{
    public sealed class RawMemPoolModel
    {
        public int NodeId { get; set; }

        public List<string> Items { get; set; }
    }
}