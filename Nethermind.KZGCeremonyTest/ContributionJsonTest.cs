using System;
using Nethermind.KZGCeremony;
using Nethermind.KZGCeremonyTest.Helpers;

namespace Nethermind.KZGCeremonyTest
{
    public class ContributionJsonTest
    {
        [Fact]
        public void TestJsonEncodeAndDecode()
        {
            var json = Helpers.Helpers.GetTestContributionJson();

            var batchContributionJson = BatchContributionJson.Decode(json);
            var batchContribution = new BatchContribution(batchContributionJson);

            var reBatchContributionJson = batchContribution.ToBatchContributionJson();
            var reJson = BatchContributionJson.Encode(reBatchContributionJson);

            Assert.Equal(json, reJson);
        }
    }
}

