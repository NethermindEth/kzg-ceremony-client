using System.Runtime.Intrinsics.X86;

namespace Nethermind.KZGCeremony;

public class Participant
{
    private readonly ICoordinator _coordinator;

    public Participant(ICoordinator coordinator)
    {
        _coordinator = coordinator;
    }

    public async Task<ContributionReceipt?> Contribute(int pollingInterval, string sessionToken, byte[] extRandomness)
    {
        // Wait our turn to contribute to the ceremony
        Console.WriteLine("Waiting for our turn to contribute...");
        BatchContributionJson? contributionBatchJson;
        while (true)
        {
            contributionBatchJson = await _coordinator.TryContribute(sessionToken);
            if (contributionBatchJson is not null)
                break;

            Console.WriteLine($"Still not our turn to contribute. Retrying in {pollingInterval} milliseconds...");
            await Task.Delay(pollingInterval);
        }

        var contributionBatch = new BatchContribution(contributionBatchJson!);
        contributionBatch.Contribute(extRandomness);

        return await _coordinator.Contribute(sessionToken, contributionBatch.ToBatchContributionJson());
    }
}
