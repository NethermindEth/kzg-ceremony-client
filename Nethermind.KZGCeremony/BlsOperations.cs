using Nethermind.Crypto.Bls;
using Nethermind.KZGCeremony;

public static class BlsParams
{
    public const int LenFr = 32;
    public const int LenFp = 64;
}

public static class BlsOperation
{
    public static (ReadOnlyMemory<byte>, bool) AddG1(in ReadOnlyMemory<byte> inputData)
    {
        const int expectedInputLength = 4 * BlsParams.LenFp;
        if (inputData.Length != expectedInputLength)
        {
            return (Array.Empty<byte>(), false);
        }

        // Span<byte> inputDataSpan = stackalloc byte[expectedInputLength];
        // inputData.PrepareEthInput(inputDataSpan);

        (byte[], bool) result;

        Span<byte> output = stackalloc byte[2 * BlsParams.LenFp];
        bool success = ShamatarLib.BlsG1Add(inputData.Span, output);
        if (success)
        {
            result = (output.ToArray(), true);
        }
        else
        {
            result = (Array.Empty<byte>(), false);
        }

        return result;
    }

    public static (ReadOnlyMemory<byte>, bool) AddG2(in ReadOnlyMemory<byte> inputData)
    {
        const int expectedInputLength = 4 * 2 * BlsParams.LenFp;
        if (inputData.Length != expectedInputLength)
        {
            return (Array.Empty<byte>(), false);
        }

        // Span<byte> inputDataSpan = stackalloc byte[expectedInputLength];
        // inputData.PrepareEthInput(inputDataSpan);

        (byte[], bool) result;

        Span<byte> output = stackalloc byte[4 * BlsParams.LenFp];
        bool success = ShamatarLib.BlsG2Add(inputData.Span, output);
        if (success)
        {
            result = (output.ToArray(), true);
        }
        else
        {
            result = (Array.Empty<byte>(), false);
        }

        return result;
    }

    public static (ReadOnlyMemory<byte>, bool) G1Mul(in ReadOnlyMemory<byte> inputData)
    {
        const int expectedInputLength = 2 * BlsParams.LenFp + BlsParams.LenFr;
        if (inputData.Length != expectedInputLength)
        {
            return (Array.Empty<byte>(), false);
        }

        // Span<byte> inputDataSpan = stackalloc byte[expectedInputLength];
        // inputData.PrepareEthInput(inputDataSpan);

        (byte[], bool) result;

        Span<byte> output = stackalloc byte[2 * BlsParams.LenFp];
        bool success = ShamatarLib.BlsG1Mul(inputData.Span, output);
        if (success)
        {
            result = (output.ToArray(), true);
        }
        else
        {
            result = (Array.Empty<byte>(), false);
        }

        return result;
    }

    public static (ReadOnlyMemory<byte>, bool) G2Mul(in ReadOnlyMemory<byte> inputData)
    {
        const int expectedInputLength = 4 * BlsParams.LenFp + BlsParams.LenFr;
        if (inputData.Length != expectedInputLength)
        {
            return (Array.Empty<byte>(), false);
        }

        // Span<byte> inputDataSpan = stackalloc byte[4 * BlsParams.LenFp + BlsParams.LenFr];
        // inputData.PrepareEthInput(inputDataSpan);

        (byte[], bool) result;

        Span<byte> output = stackalloc byte[4 * BlsParams.LenFp];
        bool success = ShamatarLib.BlsG2Mul(inputData.Span, output);
        if (success)
        {
            result = (output.ToArray(), true);
        }
        else
        {
            result = (Array.Empty<byte>(), false);
        }

        return result;
    }

    private const int ItemSizeG2MulExp = 288;

    public static (ReadOnlyMemory<byte>, bool) G2MulExp(in ReadOnlyMemory<byte> inputData)
    {
        if (inputData.Length % ItemSizeG2MulExp > 0 || inputData.Length == 0)
        {
            return (Array.Empty<byte>(), false);
        }

        (byte[], bool) result;

        Span<byte> output = stackalloc byte[4 * BlsParams.LenFp];
        bool success = ShamatarLib.BlsG2MultiExp(inputData.Span, output);
        if (success)
        {
            result = (output.ToArray(), true);
        }
        else
        {
            result = (Array.Empty<byte>(), false);
        }

        return result;
    }

    // Below is size for G1MultiExp
    private const int ItemSizeG1MulExp = 160;
    public static (ReadOnlyMemory<byte>, bool) G1MultiExp(in ReadOnlyMemory<byte> inputData)
    {
        if (inputData.Length % ItemSizeG1MulExp > 0 || inputData.Length == 0)
        {
            return (Array.Empty<byte>(), false);
        }

        (byte[], bool) result;

        Span<byte> output = stackalloc byte[2 * BlsParams.LenFp];
        bool success = ShamatarLib.BlsG1MultiExp(inputData.Span, output);
        if (success)
        {
            result = (output.ToArray(), true);
        }
        else
        {
            result = (Array.Empty<byte>(), false);
        }

        return result;
    }


    public static (ReadOnlyMemory<byte>, bool) MapToG1(in ReadOnlyMemory<byte> inputData)
    {
        const int expectedInputLength = 64;
        if (inputData.Length != expectedInputLength)
        {
            return (Array.Empty<byte>(), false);
        }

        // Span<byte> inputDataSpan = stackalloc byte[expectedInputLength];
        // inputData.PrepareEthInput(inputDataSpan);

        (byte[], bool) result;

        Span<byte> output = stackalloc byte[128];
        bool success = ShamatarLib.BlsMapToG1(inputData.Span, output);
        if (success)
        {
            result = (output.ToArray(), true);
        }
        else
        {
            result = (Array.Empty<byte>(), false);
        }

        return result;
    }

    public static (ReadOnlyMemory<byte>, bool) MapToG2(in ReadOnlyMemory<byte> inputData)
    {
        const int expectedInputLength = 2 * BlsParams.LenFp;
        if (inputData.Length != expectedInputLength)
        {
            return (Array.Empty<byte>(), false);
        }

        // Span<byte> inputDataSpan = stackalloc byte[2 * BlsParams.LenFp];
        // inputData.PrepareEthInput(inputDataSpan);

        (byte[], bool) result;

        Span<byte> output = stackalloc byte[4 * BlsParams.LenFp];
        bool success = ShamatarLib.BlsMapToG2(inputData.Span, output);
        if (success)
        {
            result = (output.ToArray(), true);
        }
        else
        {
            result = (Array.Empty<byte>(), false);
        }

        return result;
    }

    private const int PairSize = 384;
    public static (ReadOnlyMemory<byte>, bool) BlsPair(in ReadOnlyMemory<byte> inputData)
    {
        if (inputData.Length % PairSize > 0 || inputData.Length == 0)
        {
            return (Array.Empty<byte>(), false);
        }

        (byte[], bool) result;

        Span<byte> output = stackalloc byte[32];
        bool success = ShamatarLib.BlsPairing(inputData.Span, output);
        if (success)
        {
            result = (output.ToArray(), true);
        }
        else
        {
            result = (Array.Empty<byte>(), false);
        }

        return result;
    }

}