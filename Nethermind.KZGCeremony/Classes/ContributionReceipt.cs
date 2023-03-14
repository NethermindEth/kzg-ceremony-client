using Newtonsoft.Json;

namespace Nethermind.KZGCeremony;

public class ContributionReceipt
{
    [JsonProperty("receipt")]
    public string Receipt { get; set; }
    [JsonProperty("signature")]
    public string Signature { get; set; }
}