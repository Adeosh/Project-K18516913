using System.Text.Json.Serialization;

namespace Krepim.Mobile.Http
{
    public class ValidationError
    {
        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
    }
}
