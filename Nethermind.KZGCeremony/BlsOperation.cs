using Nethermind.Core.Specs;
using Nethermind.Evm.Precompiles;
using Nethermind.Evm.Precompiles.Bls.Shamatar;
using Nethermind.Specs.Forks;
using System.Numerics;
using Nethermind.Core.Extensions;

namespace Nethermind.KZGCeremony
{
    public class BlsPairingInput
    {
        public G1ElementAffine G1Encoding;
        public G2ElementAffine G2Encoding;

        public BlsPairingInput(G1ElementAffine g1Encoding, G2ElementAffine g2Encoding)
        {
            G1Encoding = g1Encoding;
            G2Encoding = g2Encoding;
        }
    }

    public class BlsOperation
    {

        private readonly IPrecompile _g1MulPrecompile;
        private readonly IPrecompile _g2MulPrecompile;
        private readonly IPrecompile _blsPairingPrecompile;
        private readonly IReleaseSpec _spec;
        public static BigInteger Modulus;

        public BlsOperation()
        {
            _g1MulPrecompile = G1MulPrecompile.Instance;
            _g2MulPrecompile = G2MulPrecompile.Instance;
            _blsPairingPrecompile = PairingPrecompile.Instance;
            _spec = MuirGlacier.Instance;


            var modBytes = Bytes.FromHexString("0x73eda753299d7d483339d80809a1d80553bda402fffe5bfeffffffff00000001");
            Bytes.ChangeEndianness8(modBytes);
            var blsMod = new BigInteger(modBytes);
            Modulus = blsMod;
        }

        public G1ElementAffine ScalarG1Mul(G1ElementAffine g1Point, BigInteger scale)
        {
            var scaleByte = ConvertBigIntToByte(scale, 32);

            return ScalarG1Mul(g1Point.ToBytes(), scaleByte);
        }


        public G1ElementAffine ScalarG1Mul(byte[] g1Point, byte[] scale)
        {
            var concatInput = g1Point.Concat(scale).ToArray();
            return ScalarG1Mul(concatInput);
        }

        public G1ElementAffine ScalarG1Mul(byte[] input)
        {
            (ReadOnlyMemory<byte> outputAdd, bool successAdd) = this._g1MulPrecompile.Run(input, this._spec);
            if (!successAdd)
            {
                throw new Exception("not successful");
            }

            return new G1ElementAffine(outputAdd.ToArray());
        }


        public G2ElementAffine ScalarG2Mul(G2ElementAffine g2Point, BigInteger scale)
        {
            var scaleByte = ConvertBigIntToByte(scale, 32);

            return ScalarG2Mul(g2Point.ToBytes(), scaleByte);
        }

        public G2ElementAffine ScalarG2Mul(byte[] g2Point, byte[] scale)
        {
            var concatInput = g2Point.Concat(scale).ToArray();
            return ScalarG2Mul(concatInput);
        }

        public G2ElementAffine ScalarG2Mul(byte[] input)
        {
            (ReadOnlyMemory<byte> outputAdd, bool successAdd) = this._g2MulPrecompile.Run(input, this._spec);
            if (!successAdd)
            {
                throw new Exception("not successful");
            }

            return new G2ElementAffine(outputAdd.ToArray());
        }

        public bool BlsPairings(BlsPairingInput[] pairings)
        {
            IEnumerable<byte> input = Enumerable.Empty<byte>(); ;

            foreach (var pairing in pairings)
            {
                input = pairing.G1Encoding.ToBytes().Concat(pairing.G2Encoding.ToBytes());
            }

            return BlsPairings(input.ToArray());
        }

        public bool BlsPairings(byte[] input)
        {
            (ReadOnlyMemory<byte> output, bool success) = this._blsPairingPrecompile.Run(input, this._spec);
            if (!success)
            {
                throw new Exception("not successful");
            }

            return new BigInteger(output.ToArray()).Equals(new BigInteger(1));
        }

        private byte[] ConvertBigIntToByte(BigInteger input, int toPad)
        {
            var outputByte = input.ToByteArray().PadRight(toPad);
            Bytes.ChangeEndianness8(outputByte);
            return outputByte;
        }
    }
}

