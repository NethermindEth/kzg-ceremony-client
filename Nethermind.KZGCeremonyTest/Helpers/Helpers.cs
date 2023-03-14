using System;
using System.Numerics;
using Nethermind.Core.Extensions;

namespace Nethermind.KZGCeremonyTest.Helpers
{
    public class Helpers
    {
        public Helpers()
        {
        }


        public static string GetTestContributionJson()
        {
            var json = "";
            using (StreamReader r = new StreamReader("Helpers/testContribution.json"))
            {
                json = r.ReadToEnd();
            }

            return json;
        }

        public static BigInteger GetX()
        {
            var xBytes = Bytes.FromHexString("73eda753299d7d483339d80809a1d80553bda402fffe5bfeffffffff00000000");
            return new BigInteger(xBytes, true, true);
        }
    }
}

