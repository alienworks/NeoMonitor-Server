using System;

namespace NeoMonitor.Abstractions.ViewModels
{
    public sealed class NeoMatrixItemViewModel
    {
        public string Url { get; set; }

        public string Net { get; set; }

        public string Method { get; set; }

        public byte Available { get; set; }

        public DateTime CreateTime { get; set; }
    }
}