using System;
namespace Nethermind.KZGCeremony
{
    public class BatchContribution
    {
        List<Contribution> Contributions;
        public BatchContribution(BatchContributionJson batchContributionJson)
        {
            Contributions = new List<Contribution>();
            foreach (var contributionJson in batchContributionJson.Contributions)
            {
                Contributions.Add(new Contribution(contributionJson));
            }
        }

        public BatchContributionJson ToBatchContributionJson()
        {
            var batchContributionJson = new BatchContributionJson();
            batchContributionJson.Contributions = new List<ContributionJson>();

            foreach (var contribution in Contributions)
            {
                var contributionJson = contribution.ToContributionJson();
                batchContributionJson.Contributions.Add(contributionJson);
            }

            return batchContributionJson;
        }

        public void Contribute(byte[] extRandomness)
        {

        }
    }
}

