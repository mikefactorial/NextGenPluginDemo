using System.Text.Json.Serialization;

namespace NextGenDemo.Shared.Models
{
    public class DeviceInfo
    {
        [JsonPropertyName("ip")]
        public string Ip { get; set; } = string.Empty;

        [JsonPropertyName("alias")]
        public string Alias { get; set; } = string.Empty;

        [JsonPropertyName("on")]
        public bool On { get; set; }

        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;

        [JsonPropertyName("sw_ver")]
        public string SoftwareVersion { get; set; } = string.Empty;

        [JsonPropertyName("mac")]
        public string MacAddress { get; set; }
    }

    public class UpdateAliasRequest
    {
        [JsonPropertyName("alias")]
        public string Alias { get; set; } = string.Empty;
    }
}