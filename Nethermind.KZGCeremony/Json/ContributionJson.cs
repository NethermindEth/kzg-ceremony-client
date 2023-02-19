using Newtonsoft.Json;

namespace Nethermind.KZGCeremony.Json
{
    public class ContributionDto
    {
        public int numG1Powers { get; set; }
        public int numG2Powers { get; set; }
        public PowersOfTauDto powersOfTau { get; set; }
        public string potPubkey { get; set; }
    }

    public class PowersOfTauDto
    {
        public List<string> G1Powers { get; set; }
        public List<string> G2Powers { get; set; }
    }

    public static class ContributionJsonTool
    {
        // public static List<Contribution> ContributionFromJson(string contributionJson)
        // {
        //     var contributionJsons = JsonConvert.DeserializeObject<List<ContributionDto>>(contributionJson);
        //     var contributions =
        //          new List<Contribution>();

        //     if (contributionJsons == null)
        //     {
        //         return contributions;
        //     }

        //     foreach (var json in contributionJsons)
        //     {
        //         var g1s = new List<G1ElementAffine>();
        //         foreach(var g1Powers in json.powersOfTau.G1Powers) {
        //             var g1 = new G1ElementAffine(g1Powers);
        //             g1s.Add(g1);
        //         }

        //         var g2s = new List<G2ElementAffine>();
        //         foreach(var g2Powers in json.powersOfTau.G2Powers) {
        //             var e2X = new E2()
        //         }
        //     }
        // }

        public static string ContributionToJson(List<ContributionDto> contributions)
        {
            return JsonConvert.SerializeObject(contributions);
        }
    }
}