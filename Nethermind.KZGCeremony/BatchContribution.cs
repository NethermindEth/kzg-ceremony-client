using System;
using Nethermind.KZGCeremony;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Numerics;
using Nethermind.Core.Extensions;
using Nethermind.Blst;

namespace Nethermind.KZGCeremony
{
    public class BatchContribution
    {
        public readonly List<Contribution> Contributions;
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
            var frs = new List<BigInteger>();
            var rnd = new Random();
            Bytes.ChangeEndianness8(extRandomness);
            var randomExt = new BigInteger(extRandomness);

            for (var i = 0; i < Contributions.Count; i++)
            {
                // 381 bits to 48 bytes
                var randomBytes = new byte[48];
                rnd.NextBytes(randomBytes);

                var randomLocal = new BigInteger(randomBytes, true, true);
                var fr = BigInteger.Multiply(randomExt, randomLocal) % BlsLib.FrModulus();

                frs.Add(fr);
            }

            ContributeWithFrs(frs);
        }

        public void ContributeWithFrs(List<BigInteger> frs)
        {
            // TODO: use async
            for (var i = 0; i < Contributions.Count; i++)
            {
                Contributions[i].UpdatePowersOfTau(frs[i]);
                Contributions[i].UpdateWitness(frs[i]);
            }
        }

        public bool Verify(BatchContribution previousBatchContribution)
        {
            for (var i = 0; i < previousBatchContribution.Contributions.Count; i++)
            {
                var ok = Contributions[i].Verify(previousBatchContribution.Contributions[i]);
                if (!ok)
                {
                    return false;
                }
            }

            return true;
        }
    }
}