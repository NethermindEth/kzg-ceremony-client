using System;
using Nethermind.Core.Extensions;
using Nethermind.KZGCeremony;

namespace Nethermind.KZGCeremonyTest
{
    public class BatchContributionTest
    {

        [Fact]
        public void TestWithInitialFile()
        {
            var xBytes = Bytes.FromHexString("73eda753299d7d483339d80809a1d80553bda402fffe5bfeffffffff00000000");

            var json = Helpers.Helpers.GetTestContributionJson();
            var batchContributionJson = BatchContributionJson.Decode(json);
            var batchContribution = new BatchContribution(batchContributionJson);

            batchContribution.Contribute(xBytes);

            var oriBatchContributionJson = BatchContributionJson.Decode(json);
            var oriBatchContribution = new BatchContribution(oriBatchContributionJson);
            var isVerify = batchContribution.Verify(oriBatchContribution);
            Assert.True(isVerify);
        }
    }
}