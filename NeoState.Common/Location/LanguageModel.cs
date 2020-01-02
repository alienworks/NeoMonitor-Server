using Newtonsoft.Json;

namespace NeoState.Common.Location
{
    public sealed class LanguageModel
    {
        [JsonProperty(PropertyName = "code")]
        public string Code { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "native")]
        public string Native { get; set; }
    }
}