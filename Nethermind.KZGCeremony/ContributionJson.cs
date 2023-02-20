using System;
using Newtonsoft.Json;

namespace Nethermind.KZGCeremony
{
    public class BatchContributionJson
    {
        [JsonProperty("contributions")]
        public List<ContributionJson> Contributions;

        public static BatchContributionJson Decode(string jsonStr)
        {
            var batchContribution = JsonConvert.DeserializeObject<BatchContributionJson>(jsonStr);
            if (batchContribution == null)
            {
                throw new Exception("Cannot be null");
            }

            return batchContribution;
        }

        public static string Encode(BatchContributionJson batchContributionJson)
        {
            return JsonConvert.SerializeObject(batchContributionJson, Formatting.Indented);
        }
    }

    public class PowersOfTauJson
    {
        public List<string> G1Powers;
        public List<string> G2Powers;
    }

    public class ContributionJson
    {
        [JsonProperty("numG1Powers")]
        public int NumG1Powers;
        [JsonProperty("numG2Powers")]
        public int NumG2Powers;
        [JsonProperty("powersOfTau")]
        public PowersOfTauJson PowersOfTau;
        [JsonProperty("potPubkey")]
        public string PotPubkey;
    }
}

