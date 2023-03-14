using Nethermind.Blst;
using Nethermind.Core.Extensions;
using Nethermind.KZGCeremony;

var sequencerHttpClient = new HttpClient();
sequencerHttpClient.BaseAddress = new Uri("http://127.0.0.1:3000/");

var coordinator = new Coordinator(sequencerHttpClient);
var ceremonyStatus = await coordinator.GetStatus();
var ceremonyTranscript = await coordinator.GetTranscript();
var sessionId = "test2";

var contributionBatchJson = await coordinator.TryContribute(sessionId);

var contributionBatch = new BatchContribution(contributionBatchJson!);
Console.WriteLine(contributionBatch);

var rnd = new Random();
var bytes = new Byte[16];
rnd.NextBytes(bytes);

contributionBatch.Contribute(bytes);
var contributionReceipt = await coordinator.Contribute(sessionId, contributionBatch.ToBatchContributionJson());
Console.WriteLine(contributionReceipt);

//var contributionBatch = contributionBatchJson.


//var httpClient = new HttpClient();
//var identity = new Identity(httpClient);
//var githubHandle = "@chee-chyuan";
//var githubIdentity = await identity.FromGitHubId(githubHandle);
//Console.WriteLine("githubIdentity: " + githubIdentity);