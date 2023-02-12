namespace Nethermind.KZGCeremony;

public class Participant
{
    private readonly ICoordinator _coordinator;
    private readonly IContributionSource _contributionSource;

    public Participant(ICoordinator coordinator, IContributionSource contributionSource)
    {
        _coordinator = coordinator;
        _contributionSource = contributionSource;
    }

    public async Task<ContributionReceipt?> Contribute(int pollingInterval)
    {
        // Wait our turn to contribute to the ceremony
        Console.WriteLine("Waiting for our turn to contribute...");
        IContributionBatch? contributionBatch;
        while (true)
        {
            contributionBatch = await _coordinator.TryContribute("a");
            if (contributionBatch is not null)
                break;

            Console.WriteLine($"Still not our turn to contribute. Retrying in {pollingInterval} milliseconds...");
            await Task.Delay(pollingInterval);
        }

        // Verifying the contribution batch
        if (!contributionBatch.Verify())
        {
            Console.WriteLine("Batch verification failed. Exiting...");
            return null; // TODO Maybe it is better to throw here
        }

        // Contributing to the ceremony
        Console.WriteLine("It is our turn to contribute. Contributing...");
        IContribution contribution = _contributionSource.Next();
        contributionBatch.Update(contribution);

        // Send updated batch to coordinator
        Console.WriteLine("Sending contribution to coordinator...");
        return await _coordinator.Contribute("a", contributionBatch);
    }
}
