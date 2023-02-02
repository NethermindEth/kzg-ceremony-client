namespace Nethermind.KZGCeremony;

public class Witness
{
    public string[]? runningProducts { get; set; }
    public string[]? potPubkeys { get; set; }
    public string[]? blsSignatures { get; set; }
}

public class PowersOfTau
{
    public string[]? G1Powers { get; set; }
    public string[]? G2Powers { get; set; }
}

public class SubTranscript
{
    public int numG1Powers { get; set; }
    public int numG2Powers { get; set; }
    public PowersOfTau? pot { get; set; }

    public Witness? witness { get; set; }

}

public class CeremonyState
{
    public SubTranscript[]? transcripts { get; set; }
    public string[]? participantIds { get; set; }
    public string[]? participantEcdsaSignatures { get; set; }
}
