using System;
using Nethermind.KZGCeremony;

namespace Nethermind.KZGCeremonyTest
{
    public class IdentityTest
    {

        [Fact]
        public void TestEthAddressIdentity()
        {
            var ethAddress = "0xd8dA6BF26964aF9D7eEd9e03E53415D37aA96045";
            var ethIdentity = Identity.FromEthAddress(ethAddress);

            Assert.Equal("eth|0xd8da6bf26964af9d7eed9e03e53415d37aa96045", ethIdentity);
        }

        [Fact]
        public async Task TestGithubIdentity()
        {
            var httpClient = new HttpClient();
            var identity = new Identity(httpClient);
            var githubHandle = "@nethermindeth";
            var githubIdentity = await identity.FromGitHubId(githubHandle);

            Assert.Equal(String.Format("git|{0}|{1}", 43478154, githubHandle), githubIdentity);
        }
    }
}

