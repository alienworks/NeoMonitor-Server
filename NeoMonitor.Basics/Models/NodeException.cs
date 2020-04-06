using System;
using System.ComponentModel.DataAnnotations;

namespace NeoMonitor.Basics.Models
{
    public class NodeException
    {
        [Key]
        [Required]
        public int Id { get; set; }

        public string Url { get; set; }

        public int ExceptionHeight { get; set; }
        public DateTime GenTime { get; set; }
        public int Intervals { get; set; }
    }
}