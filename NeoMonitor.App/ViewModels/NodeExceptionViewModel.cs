using System;
using System.Text.Json.Serialization;

namespace NeoMonitor.App.ViewModels
{
    public sealed class NodeExceptionViewModel
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("nodeName")]
        public string Url { get; set; }

        [JsonPropertyName("exceptionHeight")]
        public int ExceptionHeight { get; set; }

        [JsonPropertyName("exceptionTime")]
        public DateTime GenTime { get; set; }

        [JsonPropertyName("intervals")]
        public int Intervals { get; set; }
    }
}