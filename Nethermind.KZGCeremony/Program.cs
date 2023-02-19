

// using System.Numerics;
// using System.Reflection.Emit;
// using System.Runtime.Intrinsics.X86;
// using Nethermind.Evm.Precompiles;
// using Nethermind.Evm.Precompiles.Bls.Shamatar;
// using Nethermind.Specs.Forks;
// using Nethermind.Core.Extensions;
// using Nethermind.Core.Specs;
// using Nethermind.KZGCeremony;
// // using Nethermind.KZGCeremony;

// var g1Add = G1AddPrecompile.Instance;
// var g1Mul = G1MulPrecompile.Instance;
// // { Bytes.FromHexString("0000000000000000000000000000000012196c5a43d69224d8713389285f26b98f86ee910ab3dd668e413738282003cc5b7357af9a7af54bb713d62255e80f560000000000000000000000000000000006ba8102bfbeea4416b710c73e8cce3032c31c6269c44906f8ac4f7874ce99fb17559992486528963884ce429a992feeb3c940fe79b6966489b527955de7599194a9ac69a6ff58b8d99e7b1084f0464e"), Bytes.FromHexString("000000000000000000000000000000000f1f230329be03ac700ba718bc43c8ee59a4b2d1e20c7de95b22df14e7867eae4658ed2f2dfed4f775d4dcedb4235cf00000000000000000000000000000000012924104fdb82fb074cfc868bdd22012694b5bae2c0141851a5d6a97d8bc6f22ecb2f6ddec18cba6483f2e73faa5b942") },


// var sampleInput = Bytes.FromHexString("0000000000000000000000000000000012196c5a43d69224d8713389285f26b98f86ee910ab3dd668e413738282003cc5b7357af9a7af54bb713d62255e80f560000000000000000000000000000000006ba8102bfbeea4416b710c73e8cce3032c31c6269c44906f8ac4f7874ce99fb17559992486528963884ce429a992feeb3c940fe79b6966489b527955de7599194a9ac69a6ff58b8d99e7b1084f0464e");

// // (ReadOnlyMemory<byte> outputAdd, bool successAdd) = g1Add.Run(sampleInput, MuirGlacier.Instance);
// (ReadOnlyMemory<byte> outputMul, bool successMul) = g1Mul.Run(sampleInput, MuirGlacier.Instance);

// // Bytes.ChangeEndianness8(sampleInput);

// // var xx = new BigInteger(sampleInput);



// // var test = "E8D4A51000";
// // var testBytes = Bytes.FromHexString(test);
// // var paddedTestBytes = Bytes.PadLeft(testBytes, 16);
// // Bytes.ChangeEndianness8(paddedTestBytes);
// // var yy = new BigInteger(paddedTestBytes);


// // Console.WriteLine("Hello, World!");

// // var g1MulTest = Bytes.FromHexString("0000000000000000000000000000000012196c5a43d69224d8713389285f26b98f86ee910ab3dd668e413738282003cc5b7357af9a7af54bb713d62255e80f560000000000000000000000000000000006ba8102bfbeea4416b710c73e8cce3032c31c6269c44906f8ac4f7874ce99fb17559992486528963884ce429a992feeb3c940fe79b6966489b527955de7599194a9ac69a6ff58b8d99e7b1084f0464e");
// // var scaleMul = Bytes.FromHexString("b3c940fe79b6966489b527955de7599194a9ac69a6ff58b8d99e7b1084f0464e");
// // Bytes.ChangeEndianness8(g1MulTest);
// // Bytes.ChangeEndianness8(scaleMul);
// // var g1MulBigInteger = new BigInteger(g1MulTest);
// // var scaleMulBigInteger = new BigInteger(scaleMul);

// var blsOperation = new BlsOperation();

// // input bytes
// var res2 = Bytes.FromHexString("0000000000000000000000000000000012196c5a43d69224d8713389285f26b98f86ee910ab3dd668e413738282003cc5b7357af9a7af54bb713d62255e80f560000000000000000000000000000000006ba8102bfbeea4416b710c73e8cce3032c31c6269c44906f8ac4f7874ce99fb17559992486528963884ce429a992feeb3c940fe79b6966489b527955de7599194a9ac69a6ff58b8d99e7b1084f0464e");
// var blsRes = blsOperation.ScalarG1Mul(res2);


// var inputG1 = Bytes.FromHexString("0000000000000000000000000000000012196c5a43d69224d8713389285f26b98f86ee910ab3dd668e413738282003cc5b7357af9a7af54bb713d62255e80f560000000000000000000000000000000006ba8102bfbeea4416b710c73e8cce3032c31c6269c44906f8ac4f7874ce99fb17559992486528963884ce429a992fee");
// var scaleBytes = Bytes.FromHexString("b3c940fe79b6966489b527955de7599194a9ac69a6ff58b8d99e7b1084f0464e");

// var blsRes2 = blsOperation.ScalarG1Mul(inputG1, scaleBytes);

// var inputG1_LE = inputG1;
// Bytes.ChangeEndianness8(inputG1_LE);
// var inputG1_LE_BigInt = new BigInteger(inputG1_LE);
// var convert_back_inputG1 = inputG1_LE_BigInt.ToByteArray();
// var convert_back_inputG1_padded = convert_back_inputG1.PadRight(128);
// Bytes.ChangeEndianness8(convert_back_inputG1_padded);

// var scale_LE = scaleBytes;
// Bytes.ChangeEndianness8(scale_LE);
// var scale_LE_BigInt = new BigInteger(scale_LE);
// var convert_back_scale = scale_LE_BigInt.ToByteArray();
// var convert_back_scale_padded = convert_back_scale.PadRight(32);
// Bytes.ChangeEndianness8(convert_back_scale_padded);


// System.Console.WriteLine(string.Format("convertInput: {0}", convert_back_inputG1_padded.Concat(convert_back_scale_padded).ToArray().ToHexString()));

// // var blsRes3 = blsOperation.ScalarG1Mul(inputG1_LE_BigInt, scale_LE_BigInt);

// var xxxx = 1;



// Group Element
using Nethermind.KZGCeremony;

var g1Hex = "0x97f1d3a73197d7942695638c4fa9ac0fc3688c4f9774b905a14e3a3f171bac586c55e83ff97a1aeffb3af00adb22c6bb";
var g1 = new G1ElementAffine(g1Hex);