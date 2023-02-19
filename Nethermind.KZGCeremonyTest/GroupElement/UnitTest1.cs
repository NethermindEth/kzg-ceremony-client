using Nethermind.KZGCeremony;
namespace GroupElement;

[TestClass]
public class UnitTest1
{
    [TestMethod]
    public void TestG1AffineInitialize()
    {
        var g1Hex = "0x97f1d3a73197d7942695638c4fa9ac0fc3688c4f9774b905a14e3a3f171bac586c55e83ff97a1aeffb3af00adb22c6bb";
        var g1 = new G1ElementAffine(g1Hex);
    }
}