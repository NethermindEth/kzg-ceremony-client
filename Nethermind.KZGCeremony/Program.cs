
using Nethermind.KZGCeremony;
using System.Net;

using static Blst.blst;

namespace KZGClient
{

    static class Progam
    {
        public static void Main()
        {

            Verifier v = new Verifier();
            // v.Verify().Wait();

            P1_Affine p = P1_Affine.generator();
            Console.WriteLine(p);

            Console.WriteLine("Hello World!!");
        }
    }

}