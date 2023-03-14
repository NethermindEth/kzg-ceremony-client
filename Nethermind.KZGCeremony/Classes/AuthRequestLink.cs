using System;
using Newtonsoft.Json;

namespace Nethermind.KZGCeremony.Classes
{
    public class AuthRequestLink
    {
        [JsonProperty("eth_auth_url")]
        public string EthAuthUrl { get; set; }
        [JsonProperty("github_auth_url")]
        public string GithubAuthUrl { get; set; }
    }
}

