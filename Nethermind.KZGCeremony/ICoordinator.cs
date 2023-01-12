namespace Nethermind.KZGCeremony;

public interface ICoordinator
{
    Task<CeremonyStatus> GetStatus();
    Task<CeremonyTranscript> GetTranscript();
    Task<IContributionBatch?> TryContribute(string sessionToken);
    Task<ContributionReceipt> Contribute(string sessionToken, IContributionBatch contributionBatch);
    Task Abort(string sessionToken);
}
