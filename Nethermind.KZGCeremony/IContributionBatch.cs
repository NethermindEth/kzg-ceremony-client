namespace Nethermind.KZGCeremony;

public interface IContributionBatch
{
    bool Verify();
    void Update(IContribution contribution);
}
