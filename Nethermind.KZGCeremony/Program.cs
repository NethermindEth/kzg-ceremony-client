using Nethermind.Blst;
using Nethermind.Core.Extensions;

Console.WriteLine("Hello, World!");

var sampleP1Hex = "97f1d3a73197d7942695638c4fa9ac0fc3688c4f9774b905a14e3a3f171bac586c55e83ff97a1aeffb3af00adb22c6bb";
var sampleP1Bytes = Bytes.FromHexString(sampleP1Hex);
// var scaleBytes = Bytes.FromHexString("b3c940fe79b6966489b527955de7599194a9ac69a6ff58b8d99e7b1084f0464e");

//// something wrong here??
//var sampleP1Uncompressed = BlsLib.BlsP1Uncompress(sampleP1Bytes);
//Console.WriteLine(sampleP1Uncompressed);

//var _sig = new blst.P2_Affine(sig_for_wire);
var sampleP1Affine = new BlsLib.P1_Affine(sampleP1Bytes);
var sampleP1Compress = sampleP1Affine.compress();
Console.WriteLine(sampleP1Affine);

var sampleP2Hex = "93e02b6052719f607dacd3a088274f65596bd0d09920b61ab5da61bbdc7f5049334cf11213945d57e5ac7d055d042b7e024aa2b2f08f0a91260805272dc51051c6e47ad4fa403b02b4510b647ae3d1770bac0326a805bbefd48056c8c121bdb8";
var sampleP2Bytes = Bytes.FromHexString(sampleP2Hex);
var sampleP2Affine = new BlsLib.P2_Affine(sampleP2Bytes);
var sampleP2Compress = sampleP2Affine.compress();
Console.WriteLine(sampleP2Affine);