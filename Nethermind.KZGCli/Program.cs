using System.CommandLine;
using Nethermind.KZGCeremony;
using Newtonsoft.Json;
using static System.Net.Mime.MediaTypeNames;

var rootCommand = new RootCommand("Contribute to Ethereum's KZG Ceremony");
var entropyFileOption = new Option<FileInfo?>(
            name: "--file",
            description: "File containing randomness");
var sequencerUrlOption = new Option<string>(
            name: "--url",
            description: "Url of KZG ceremony sequencer",
            getDefaultValue: () => "https://kzg-ceremony-sequencer-dev.fly.dev");
var pollingTimeOption = new Option<int>(
            name: "--interval",
            description: "Polling time interval in ms",
            getDefaultValue: () => 10000);


rootCommand.AddGlobalOption(entropyFileOption);
rootCommand.AddGlobalOption(sequencerUrlOption);
rootCommand.AddGlobalOption(pollingTimeOption);


var etheruemCommand = new Command("ethereum", "Contribute using Ethereum address");
var githubCommand = new Command("github", "Contribute using Github handle");

rootCommand.AddCommand(etheruemCommand);
rootCommand.AddCommand(githubCommand);

etheruemCommand.SetHandler(async (entropyFile, sequencerUrl, pollingInterval) =>
{

    var sequencerHttpClient = new HttpClient();
    sequencerHttpClient.BaseAddress = new Uri(sequencerUrl);

    var coordinator = new Coordinator(sequencerHttpClient);
    var participant = new Participant(coordinator);

    var requestLink = await coordinator.GetAuthRequestLink();

    Console.WriteLine("Visit this link and enter your session Id below.");
    Console.WriteLine(requestLink.EthAuthUrl);
    Console.WriteLine("\n Enter Session id: \n");
    var sessionToken = Console.ReadLine();
    if (string.IsNullOrEmpty(sessionToken))
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Error: Session Id not found");
        Console.ResetColor();
        Console.ReadKey();
        return;
    }

    // TODO: callback to get ethaddress

    byte[] extRandomness;
    if (entropyFile == null)
    {

        var rnd = new Random();
        var randomBytes = new byte[48];
        rnd.NextBytes(randomBytes);
        extRandomness = randomBytes;
    }
    else
    {
        extRandomness = await File.ReadAllBytesAsync(entropyFile.FullName);
    }

    await CliContributeAsync(extRandomness, participant, sessionToken, pollingInterval);
},
    entropyFileOption, sequencerUrlOption, pollingTimeOption);

githubCommand.SetHandler(async (entropyFile, sequencerUrl, pollingInterval) =>
{

    var sequencerHttpClient = new HttpClient();
    sequencerHttpClient.BaseAddress = new Uri(sequencerUrl);

    var coordinator = new Coordinator(sequencerHttpClient);
    var participant = new Participant(coordinator);

    var requestLink = await coordinator.GetAuthRequestLink();

    Console.WriteLine("Visit this link and enter your session Id below.");
    Console.WriteLine(requestLink.GithubAuthUrl);

    Console.WriteLine("\n Enter Session id: \n");
    var sessionToken = Console.ReadLine();
    if (string.IsNullOrEmpty(sessionToken))
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Error: Session Id not found");
        Console.ResetColor();
        Console.ReadKey();
        return;
    }

    // TODO: callback to get github handle

    byte[] extRandomness;
    if (entropyFile == null)
    {

        var rnd = new Random();
        var randomBytes = new byte[48];
        rnd.NextBytes(randomBytes);
        extRandomness = randomBytes;
    }
    else
    {
        extRandomness = await File.ReadAllBytesAsync(entropyFile.FullName);
    }

    await CliContributeAsync(extRandomness, participant, sessionToken, pollingInterval);
},
    entropyFileOption, sequencerUrlOption, pollingTimeOption);

return await rootCommand.InvokeAsync(args);

async Task CliContributeAsync(byte[] extRandomness, Participant participant, string sessionToken, int pollingInterval)
{
    try
    {
        var contributionReceipt = await participant.Contribute(pollingInterval, sessionToken, extRandomness);

        if (contributionReceipt == null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Error: contributionReceipt not found");
            Console.ResetColor();
            Console.ReadKey();
            return;
        }

        var json = JsonConvert.SerializeObject(contributionReceipt);
        string destPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "contributionReceipt.json");
        System.IO.File.WriteAllText(destPath, json);

    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(ex.ToString());
        Console.ResetColor();
        Console.ReadKey();
        return;
    }

    return;
}