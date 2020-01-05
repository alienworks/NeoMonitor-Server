using System.Text.Json.Serialization;

namespace NeoState.Common.Location
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