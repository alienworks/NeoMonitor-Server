﻿using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NeoState.Common.Location
{
    public class LocationModel
    {
        [JsonPropertyName("country_flag")]
        public string Flag { get; set; }

        [JsonPropertyName("capital")]
        public string Capital { get; set; }

        [JsonPropertyName("languages")]
        public List<LanguageModel> Languages { get; set; }
    }
}