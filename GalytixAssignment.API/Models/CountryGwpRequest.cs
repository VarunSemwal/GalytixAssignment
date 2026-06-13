using System.Text.Json.Serialization;

namespace GalytixAssignment.API.Models
{
    public sealed class CountryGwpRequest
    {
        public string Country { get; set; } = string.Empty;

        [JsonPropertyName("lob")]
        public IEnumerable<string> LineOfBusinesses { get; set; } = Enumerable.Empty<string>();
    }
}
