using System.Text.Json.Serialization;

namespace NeoMonitor.Common.IP.Models
{
    public sealed class LanguageModel
    {
        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("native")]
        public string Native { get; set; }
    }
}