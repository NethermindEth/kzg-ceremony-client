using System;
namespace Nethermind.KZGCeremony
{
    public class ContributionResult
    {
        public BatchContributionJson BatchContribution { get; set; }
        public ContributionReceipt ContributionReceipt { get; set; }
    }
}

