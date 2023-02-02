namespace Nethermind.KZGCeremony;

public class CeremonyStatus
{
    public int lobby_size { get; set; }
    public int num_contributions { get; set; }
    public string? sequencer_address { get; set; }

    public override string ToString()
    {
        return $"\nLobby Size: {lobby_size} \nNumber of Contributions: {num_contributions} \nSequencer ETH Address: {sequencer_address}";
    }
}
