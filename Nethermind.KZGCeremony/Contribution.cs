using System;
using System.Numerics;
using System.Reflection.Emit;
using System.Runtime.Intrinsics.X86;
using Nethermind.Blst;
using Nethermind.Core.Extensions;
using Nethermind.KZGCeremony;
using static Nethermind.Blst.BlsLib;

namespace Nethermind.KZGCeremony
{
    public class PowersOfTau
    {
        public List<BlsLib.P1_Affine> G1Affines;
        public List<BlsLib.P2_Affine> G2Affines;

        public PowersOfTau(List<BlsLib.P1_Affine> g1Affines, List<BlsLib.P2_Affine> g2Affines)
        {
            G1Affines = g1Affines;
            G2Affines = g2Affines;
        }
    }

    public class Contribution
    {
        public int NumG1Powers;
        public int NumG2Powers;
        public PowersOfTau _PowersOfTau;
        public BlsLib.P2_Affine PotPubKey;


        public Contribution(int numG1Powers, int numG2Powers, PowersOfTau powersOfTau, BlsLib.P2_Affine potPubKey)
        {
            NumG1Powers = numG1Powers;
            NumG2Powers = numG2Powers;
            _PowersOfTau = powersOfTau;
            PotPubKey = potPubKey;
        }

        public Contribution(ContributionJson contributionJson)
        {
            NumG1Powers = contributionJson.NumG1Powers;
            NumG2Powers = contributionJson.NumG2Powers;

            var g1Affines = new List<BlsLib.P1_Affine>();
            foreach (var g1PowerStr in contributionJson.PowersOfTau.G1Powers)
            {
                var g1Power = Bytes.FromHexString(g1PowerStr.Substring(2));
                var g1Affine = new BlsLib.P1_Affine(g1Power);

                g1Affines.Add(g1Affine);
            }

            var g2Affines = new List<BlsLib.P2_Affine>();
            foreach (var g2PowerStr in contributionJson.PowersOfTau.G2Powers)
            {
                var g2Power = Bytes.FromHexString(g2PowerStr.Substring(2));
                var g2Affine = new BlsLib.P2_Affine(g2Power);
                g2Affines.Add(g2Affine);
            }

            var potPubKey = Bytes.FromHexString(contributionJson.PotPubkey.Substring(2));
            PotPubKey = new BlsLib.P2_Affine(potPubKey);

            var pot = new PowersOfTau(g1Affines, g2Affines);
            _PowersOfTau = pot;
        }

        public ContributionJson ToContributionJson()
        {
            var contributionJson = new ContributionJson();

            contributionJson.NumG1Powers = this.NumG1Powers;
            contributionJson.NumG2Powers = this.NumG2Powers;

            var powersOfTauJson = new PowersOfTauJson();

            var g1AffinesHexes = new List<string>();
            foreach(var g1Affine in _PowersOfTau.G1Affines)
            {
                var g1AffineBytes = g1Affine.compress();
                var g1AffineHex = Bytes.ToHexString(g1AffineBytes);
                g1AffinesHexes.Add("0x" + g1AffineHex);
            }

            var g2AffinesHexes = new List<string>();
            foreach (var g2Affine in _PowersOfTau.G2Affines)
            {
                var g2AffineBytes = g2Affine.compress();
                var g2AffineHex = Bytes.ToHexString(g2AffineBytes);
                g2AffinesHexes.Add("0x" + g2AffineHex);
            }

            powersOfTauJson.G1Powers = g1AffinesHexes;
            powersOfTauJson.G2Powers = g2AffinesHexes;
            contributionJson.PowersOfTau = powersOfTauJson;

            var potPubKeyBytes = PotPubKey.compress();
            var potPubKeyHex = Bytes.ToHexString(potPubKeyBytes);
            contributionJson.PotPubkey = "0x" + potPubKeyHex;

            return contributionJson;
        }

        public bool Verify(Contribution previousContribution)
        {
            var prevG1Power = previousContribution._PowersOfTau.G1Affines[1];

            var potPubBytes = PotPubKey.compress();
            var potPubHex = Bytes.ToHexString(potPubBytes);
            var left = new BlsLib.PT(prevG1Power, this.PotPubKey);

            var postG1Power = this._PowersOfTau.G1Affines[1];
            var right = new BlsLib.PT(postG1Power, BlsLib.P2_Affine.generator());

            return BlsLib.PT.finalverify(left, right);
        }

        public void UpdatePowersOfTau(BigInteger x)
        {
            var xi = BigInteger.One;
            for (var i = 0; i < this.NumG1Powers; i++)
            {
                var p1 = new BlsLib.P1(this._PowersOfTau.G1Affines[i]);
                var newP1 = p1.mult(xi);

                _PowersOfTau.G1Affines[i] = new BlsLib.P1_Affine(newP1);

                if (i < this.NumG2Powers)
                {
                    var p2 = new BlsLib.P2(this._PowersOfTau.G2Affines[i]);
                    var newP2 = p2.mult(xi);
                    this._PowersOfTau.G2Affines[i] = new BlsLib.P2_Affine(newP2);
                }

                xi = BigInteger.Multiply(xi, x) % BlsLib.FrModulus();
            }
        }

        public void UpdateWitness(BigInteger x)
        {
            var p2Generator = BlsLib.P2.generator();
            var newPotPubKey = p2Generator.mult(x);
            this.PotPubKey = new BlsLib.P2_Affine(newPotPubKey);
        }
    }
}