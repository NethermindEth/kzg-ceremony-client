using System.Numerics;
using Nethermind.Core.Extensions;

namespace Nethermind.KZGCeremony
{
    public class BigIntToBytesConverter
    {
        protected byte[] ConvertBigIntToByte(BigInteger input, int toPad)
        {
            var outputByte = input.ToByteArray().PadRight(toPad);
            Bytes.ChangeEndianness8(outputByte);
            return outputByte;
        }


        // concat X and Y to bytes which is compatible to shamatar lib's input
        protected virtual byte[] ToBytes(BigInteger x, BigInteger y)
        {
            var xBytes = ConvertBigIntToByte(x, 64);
            var yBytes = ConvertBigIntToByte(y, 64);
            var res = xBytes.Concat(yBytes);

            return res.ToArray();
        }

        protected virtual (BigInteger, BigInteger) FromBytes(byte[] inputBytes)
        {
            if (inputBytes.Length != 128)
            {
                new Exception("Incorrect bytes length. Not 128");
            }
            var xBytes = inputBytes.Slice(0, 64);
            var yBytes = inputBytes.Slice(64);

            Bytes.ChangeEndianness8(xBytes);
            Bytes.ChangeEndianness8(yBytes);


            var X = new BigInteger(xBytes);
            var Y = new BigInteger(yBytes);

            return (X, Y);
        }
    }

    public class G1ElementAffine : BigIntToBytesConverter
    {
        public BigInteger X;
        public BigInteger Y;
        public G1ElementAffine(byte[] inputBytes)
        {
            (X, Y) = FromBytes(inputBytes);
        }

        public byte[] ToBytes()
        {
            return base.ToBytes(X, Y);
        }
    }

    // field extension
    public class E2 : BigIntToBytesConverter
    {
        public BigInteger A0;
        public BigInteger A1;

        public E2(string a0Hex, string a1Hex)
        {
            var a0Bytes = Bytes.FromHexString(a0Hex);
            var a1Bytes = Bytes.FromHexString(a1Hex);
            Bytes.ChangeEndianness8(a0Bytes);
            Bytes.ChangeEndianness8(a1Bytes);

            A0 = new BigInteger(a0Bytes);
            A1 = new BigInteger(a1Bytes);
        }

        public E2(BigInteger a0, BigInteger a1)
        {
            A0 = a0;
            A1 = a1;
        }

        public E2(byte[] input)
        {
            (A0, A1) = FromBytes(input);
        }

        public byte[] ToBytes()
        {
            return base.ToBytes(A0, A1);
        }
    }

    public class G2ElementAffine
    {
        public E2 X;
        public E2 Y;

        public G2ElementAffine(E2 x, E2 y)
        {
            X = x;
            Y = y;
        }

        public G2ElementAffine(byte[] input)
        {
            var xByte = input.Slice(0, 128);
            var yByte = input.Slice(128);

            X = new E2(xByte);
            Y = new E2(yByte);
        }

        public byte[] ToBytes()
        {
            var xByte = X.ToBytes();
            var yBytes = Y.ToBytes();

            return xByte.Concat(yBytes).ToArray();
        }
    }
}