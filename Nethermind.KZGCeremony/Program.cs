using Nethermind.Blst;
using Nethermind.Core.Extensions;
using Nethermind.KZGCeremony;

Console.WriteLine("Hello, World!");



var httpClient = new HttpClient();
var identity = new Identity(httpClient);
var githubHandle = "@chee-chyuan";
var githubIdentity = await identity.FromGitHubId(githubHandle);
Console.WriteLine("githubIdentity: " + githubIdentity);