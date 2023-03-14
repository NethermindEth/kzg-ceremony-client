using Newtonsoft.Json;
using static Nethermind.Blst.BlsLib;

namespace Nethermind.KZGCeremony;

public class CeremonyTranscript
{
    [JsonProperty("participantIds")]
    public List<string> ParticipantIDs { get; set; }
    [JsonProperty("participantEcdsaSignatures")]
    public List<string> ParticipantECDSASignatures { get; set; }
    [JsonProperty("transcripts")]
    public List<Transcript> Transcripts { get; set; }
}

public class Transcript
{
    [JsonProperty("numG1Powers")]
    public int NumG1Powers { get; set; }
    [JsonProperty("numG2Powers")]
    public int NumG2Powers { get; set; }
    [JsonProperty("powersOfTau")]
    public PowersOfTauJson PowersOfTau { get; set; }
    [JsonProperty("witness")]
    public Witness Witness { get; set; }
}

public class Witness
{
    [JsonProperty("runningProducts")]
    public List<string> RunningProducts { get; set; }
    [JsonProperty("potPubkeys")]
    public List<string> PotPubKeys { get; set; }
    [JsonProperty("blsSignatures")]
    public List<string> BlsSignatures { get; set; }
}