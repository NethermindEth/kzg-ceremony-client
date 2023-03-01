namespace Nethermind.KZGCeremony;

public interface ICoordinator
{
    Task<CeremonyStatus> GetStatus();
    Task<CeremonyTranscript> GetTranscript();
    Task<BatchContributionJson?> TryContribute(string sessionToken);
    Task<ContributionReceipt> Contribute(string sessionToken, BatchContributionJson batchContributionJson);
    Task Abort(string sessionToken);
}
