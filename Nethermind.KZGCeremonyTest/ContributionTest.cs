using Nethermind.KZGCeremony;
using Newtonsoft.Json;

namespace Nethermind.KZGCeremonyTest;

public class ContributionTest
{
    [Fact]
    public void TestJsonEncodeAndDecode()
    {
        var json = "";
        using (StreamReader r = new StreamReader("testContribution.json"))
        {
            json = r.ReadToEnd();
        }

        var batchContributionJson = BatchContributionJson.Decode(json);
        var batchContribution = new BatchContribution(batchContributionJson);

        var reBatchContributionJson = batchContribution.ToBatchContributionJson();
        var reJson = BatchContributionJson.Encode(reBatchContributionJson);

        Assert.Equal(json, reJson);
    }
}

