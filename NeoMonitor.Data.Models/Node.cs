using NeoState.Common;
using System;
using System.ComponentModel.DataAnnotations;

namespace NeoMonitor.Data.Models
{
    public class Node
    {
        [Key]
        [Required]
        public int Id { get; set; }
        public string Url { get; set; }
        public string IP { get; set; }
        public NodeAddressType Type { get; set; }
        public string Version { get; set; }
        public int? Height { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public long latency { get; set; }
        public int? Peers { get; set; }
        public int? MemoryPool { get; set; }
        public int ExceptionCount { get; set; }

        public string Locale { get; set; }
        public string Location { get; set; }
        public double? Longitude { get; set; }
        public double? Latitude { get; set; }
        public string FlagUrl { get; set; }

        public string Net { get; set; }
    }
}
