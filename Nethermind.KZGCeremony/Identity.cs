using System;
using System.Net.Http;
using System.Net.Http.Json;

namespace Nethermind.KZGCeremony
{
    public class GitHubIdentityResponse
    {
        public int Id { get; set; }
    }

    public class Identity
    {
        private readonly HttpClient _httpClient;

        public Identity(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public static string FromEthAddress(string ethAddress)
        {
            if (!ethAddress.StartsWith("0x"))
            {
                throw new Exception("Must start from 0x");
            }
            return "eth|" + ethAddress.ToLower();
        }

        public async Task<string> FromGitHubId(string githubHandle)
        {
            if (githubHandle.StartsWith("@"))
            {
                githubHandle = githubHandle.Substring(1);
            }

            var githubId = await GetGitHubId(githubHandle);

            return String.Format("git|{0}|{1}", githubId, "@" + githubHandle.ToLower());
        }

        // githubHandle without @
        private async Task<int> GetGitHubId(string githubHandle)
        {
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "http://developer.github.com/v3/#user-agent-required");
            var response = await _httpClient.GetAsync("https://api.github.com/users/" + githubHandle);
            var content = response.EnsureSuccessStatusCode().Content;
            var githubResponse = await content.ReadFromJsonAsync<GitHubIdentityResponse>();

            return githubResponse != null ? githubResponse.Id : throw new Exception("GetGitHubId Json response fail");
        }
    }
}

