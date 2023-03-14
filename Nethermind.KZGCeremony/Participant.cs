using System.Runtime.Intrinsics.X86;

namespace Nethermind.KZGCeremony;

public class Participant
{
    private readonly ICoordinator _coordinator;

    public Participant(ICoordinator coordinator)
    {
        _coordinator = coordinator;
    }

    public async Task<ContributionResult?> Contribute(int pollingInterval, string sessionToken, byte[] extRandomness)
    {
        // Wait our turn to contribute to the ceremony
        Console.WriteLine("Waiting for our turn to contribute...");
        BatchContributionJson? contributionBatchJson;
        while (true)
        {
            contributionBatchJson = await _coordinator.TryContribute(sessionToken);

            if (contributionBatchJson is not null)
            {
                Console.WriteLine("Yes! Our time to contribute!");
                break;
            }

            Console.WriteLine($"Still not our turn to contribute. Retrying in {pollingInterval} milliseconds...");
            await Task.Delay(pollingInterval);
        }

        var contributionBatch = new BatchContribution(contributionBatchJson!);

        Console.WriteLine("Updating Powers of Tau");
        contributionBatch.Contribute(extRandomness);
        Console.WriteLine("Sending contribution...");

        var receipt = await _coordinator.Contribute(sessionToken, contributionBatch.ToBatchContributionJson());
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Contribution completed 🎉");
        Console.ResetColor();

        var result = new ContributionResult
        {
            ContributionReceipt = receipt,
            BatchContribution = contributionBatch.ToBatchContributionJson()
        };

        return result;
    }
}
