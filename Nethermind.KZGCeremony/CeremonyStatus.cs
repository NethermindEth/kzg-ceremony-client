using Newtonsoft.Json;

namespace Nethermind.KZGCeremony;

public class CeremonyStatus
{
    public int LobbySize { get; set; }
    public int NumContributions { get; set; }
    [JsonProperty("sequencer_address")]
    public string SequencerAddress { get; set; }
}
