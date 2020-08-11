using System;

namespace NeoMonitor.Abstractions.Models
{
    public sealed class NeoMatrixItemEntity
    {
        public long Id { get; set; }

        public string Url { get; set; }

        public string Net { get; set; }

        public string Method { get; set; }

        public byte Available { get; set; }

        public string Error { get; set; }

        public long GroupId { get; set; }

        public DateTime CreateTime { get; set; }
    }
}