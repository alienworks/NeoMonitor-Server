using System.Collections.Generic;
using Newtonsoft.Json;

namespace NeoState.Common.Location
{
    public class LocationModel
    {
        [JsonProperty(PropertyName = "country_flag")]
        public string Flag { get; set; }

        [JsonProperty(PropertyName = "capital")]
        public string Capital { get; set; }

        [JsonProperty(PropertyName = "languages")]
        public List<LanguageModel> Languages { get; set; }
    }
}