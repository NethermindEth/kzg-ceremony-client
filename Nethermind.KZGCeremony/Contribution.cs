using System.Numerics;

namespace Nethermind.KZGCeremony
{
    public class PowersOfTau
    {
        public G1ElementAffine[] G1;
        public G2ElementAffine[] G2;

        public PowersOfTau(G1ElementAffine[] g1, G2ElementAffine[] g2)
        {
            G1 = g1;
            G2 = g2;
        }
    }

    public class Contribution
    {
        public int NumG1Powers;
        public int NumG2Powers;
        public PowersOfTau _PowersOfTau;
        public G2ElementAffine PotPubKey;
        private readonly BlsOperation _blsOperation;
        public readonly G2ElementAffine G2Generator;

        public Contribution(PowersOfTau _powersOfTau, G2ElementAffine _potPubKey)
        {
            _blsOperation = new BlsOperation();
            _PowersOfTau = _powersOfTau;
            PotPubKey = _potPubKey;

            var X_c0 = "0x024aa2b2f08f0a91260805272dc51051c6e47ad4fa403b02b4510b647ae3d1770bac0326a805bbefd48056c8c121bdb8";
            var X_c1 = "0x13e02b6052719f607dacd3a088274f65596bd0d09920b61ab5da61bbdc7f5049334cf11213945d57e5ac7d055d042b7e";
            var Y_c0 = "0x0ce5d527727d6e118cc9cdc6da2e351aadfd9baa8cbdd3a76d429a695160d12c923ac9cc3baca289e193548608b82801";
            var Y_c1 = "0x0606c4a02ea734cc32acd2b02bc28b99cb3e287e85a763af267492ab572e99ab3f370d275cec1da1aaa9075ff05f79be";
            G2Generator = new G2ElementAffine(new E2(X_c0, X_c1), new E2(Y_c0, Y_c1));
        }

        public void UpdatePowersOfTau(BigInteger x)
        {
            var xi = new BigInteger(1);
            for (var i = 0; i < NumG1Powers; i++)
            {
                var newG1 = _blsOperation.ScalarG1Mul(_PowersOfTau.G1[i], xi);

                _PowersOfTau.G1[i] = newG1;

                if (i < NumG2Powers)
                {
                    var newG2 = _blsOperation.ScalarG2Mul(_PowersOfTau.G2[i], xi);
                    _PowersOfTau.G2[i] = newG2;
                }

                var xiNew = BigInteger.Multiply(xi, x) % BlsOperation.Modulus;
            }
        }

        public bool Verify(Contribution previousContribution)
        {
            var prevG1Power = previousContribution._PowersOfTau.G1[1];

            var blsInputsLeft = new BlsPairingInput[] { new BlsPairingInput(prevG1Power, PotPubKey) };
            var left = _blsOperation.BlsPairings(blsInputsLeft);

            var postG1Power = _PowersOfTau.G1[1];
            var blsInputsRight = new BlsPairingInput[] { new BlsPairingInput(postG1Power, G2Generator) };
            var right = _blsOperation.BlsPairings(blsInputsRight);

            if (!left.Equals(right))
            {
                return false;
            }

            return true;

        }

        public void UpdateWithness(BigInteger x)
        {
            PotPubKey = _blsOperation.ScalarG2Mul(G2Generator, x);
        }
    }
}