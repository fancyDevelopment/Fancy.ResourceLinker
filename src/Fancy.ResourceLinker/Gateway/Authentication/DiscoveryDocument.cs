using System.Text.Json.Serialization;

namespace Fancy.ResourceLinker.Gateway.Authentication
{
    public class DiscoveryDocument
    {
        [JsonPropertyName("token_endpoint")]
        public string TokenEndpoint { get; set; } = "";
    }
}
