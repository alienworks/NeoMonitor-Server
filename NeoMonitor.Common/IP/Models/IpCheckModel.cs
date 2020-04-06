using System.Text.Json.Serialization;

namespace NeoMonitor.Common.IP.Models
{
    public class IpCheckModel
    {
        [JsonPropertyName("ip")]
        public string Ip { get; set; }

        [JsonPropertyName("city")]
        public string City { get; set; }

        [JsonPropertyName("country_name")]
        public string CountryName { get; set; }

        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }

        [JsonPropertyName("location")]
        public LocationModel Location { get; set; }
    }
}