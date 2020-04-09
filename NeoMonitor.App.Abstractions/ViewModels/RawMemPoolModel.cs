using System.Collections.Generic;

namespace NeoMonitor.App.Abstractions.ViewModels
{
    public sealed class RawMemPoolModel
    {
        public long NodeId { get; set; }

        public List<string> Items { get; set; }
    }
}