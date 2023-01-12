namespace Nethermind.KZGCeremony;

public interface ICoordinator
{
    Task<IContributionBatch?> TryContribute();
    Task<ContributionReceipt> Contribute(IContributionBatch contributionBatch);
}
