using System.CommandLine;
using System.Security.Cryptography;
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
            getDefaultValue: () => "https://seq.ceremony.ethereum.org");
var pollingTimeOption = new Option<int>(
            name: "--interval",
            description: "Polling time interval in ms",
            getDefaultValue: () => 30000);
var outputFilePathOption = new Option<string>(
            name: "--output",
            description: "Directory of output contribution receipt file",
            getDefaultValue: () => AppDomain.CurrentDomain.BaseDirectory);

rootCommand.AddGlobalOption(entropyFileOption);
rootCommand.AddGlobalOption(sequencerUrlOption);
rootCommand.AddGlobalOption(pollingTimeOption);


var etheruemCommand = new Command("ethereum", "Contribute using Ethereum address");
var githubCommand = new Command("github", "Contribute using Github handle");

rootCommand.AddCommand(etheruemCommand);
rootCommand.AddCommand(githubCommand);

etheruemCommand.SetHandler(async (entropyFile, sequencerUrl, pollingInterval, outputFilePath) =>
{

    var sequencerHttpClient = new HttpClient();
    sequencerHttpClient.BaseAddress = new Uri(sequencerUrl);

    var coordinator = new Coordinator(sequencerHttpClient);
    var participant = new Participant(coordinator);

    var requestLink = await coordinator.GetAuthRequestLink();

    Console.WriteLine("Visit this link and enter your session Id below.");
    Console.WriteLine(requestLink.EthAuthUrl);
    Console.WriteLine("\n Enter Session id: \n");
    var sessionToken = Console.ReadLine().Trim(' ', '\t', '\r', '\n');
    if (string.IsNullOrEmpty(sessionToken))
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Error: Session Id not found");
        Console.ResetColor();
        Console.ReadKey();
        return;
    }

    byte[] extRandomness;
    if (entropyFile == null)
    {

        var rnd = RandomNumberGenerator.Create();
        var randomBytes = new byte[48];
        rnd.GetBytes(randomBytes);
        extRandomness = randomBytes;
    }
    else
    {
        extRandomness = await File.ReadAllBytesAsync(entropyFile.FullName);
    }

    await CliContributeAsync(extRandomness, participant, sessionToken, pollingInterval, outputFilePath);
},
    entropyFileOption, sequencerUrlOption, pollingTimeOption, outputFilePathOption);

githubCommand.SetHandler(async (entropyFile, sequencerUrl, pollingInterval, outputFilePath) =>
{
    var sequencerHttpClient = new HttpClient();
    sequencerHttpClient.BaseAddress = new Uri(sequencerUrl);

    var coordinator = new Coordinator(sequencerHttpClient);
    var participant = new Participant(coordinator);

    var requestLink = await coordinator.GetAuthRequestLink();

    Console.WriteLine("Visit this link and enter your session Id below.");
    Console.WriteLine(requestLink.GithubAuthUrl);

    Console.WriteLine("\n Enter Session id: \n");
    var sessionToken = Console.ReadLine().Trim(' ', '\t', '\r', '\n');
    if (string.IsNullOrEmpty(sessionToken))
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Error: Session Id not found");
        Console.ResetColor();
        Console.ReadKey();
        return;
    }

    byte[] extRandomness;
    if (entropyFile == null)
    {

        var rnd = RandomNumberGenerator.Create();
        var randomBytes = new byte[48];
        rnd.GetBytes(randomBytes);
        extRandomness = randomBytes;
    }
    else
    {
        var fileBytes = await File.ReadAllBytesAsync(entropyFile.FullName);
        var byteLength = fileBytes.Length;
        var mult16 = byteLength / 16;
        var supposedLength = (mult16 + 1) * 16;
        var newArray = new byte[supposedLength];

        var startAt = newArray.Length - byteLength;
        Array.Copy(fileBytes, 0, newArray, startAt, byteLength);
        extRandomness = newArray;
    }

    await CliContributeAsync(extRandomness, participant, sessionToken, pollingInterval, outputFilePath);
},
    entropyFileOption, sequencerUrlOption, pollingTimeOption, outputFilePathOption);

return await rootCommand.InvokeAsync(args);

async Task CliContributeAsync(byte[] extRandomness, Participant participant, string sessionToken,
                              int pollingInterval, string outputFilePath)
{
    if (string.IsNullOrEmpty(outputFilePath))
    {
        outputFilePath = AppDomain.CurrentDomain.BaseDirectory;
    }

    try
    {
        var contributionResult = await participant.Contribute(pollingInterval, sessionToken, extRandomness);

        if (contributionResult == null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Error: contributionReceipt not found");
            Console.ResetColor();
            Console.ReadKey();
            return;
        }

        var receiptJson = JsonConvert.SerializeObject(contributionResult.ContributionReceipt);
        string receiptPath = Path.Combine(outputFilePath, "contributionReceipt.json");
        System.IO.File.WriteAllText(receiptPath, receiptJson);

        var contributionJson = JsonConvert.SerializeObject(contributionResult.BatchContribution);
        string contributionPath = Path.Combine(outputFilePath, "contribution.json");
        System.IO.File.WriteAllText(contributionPath, contributionJson);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(String.Format("Contribution written to {0}", contributionPath));
        Console.WriteLine(String.Format("ContributionReceipt written to {0}", receiptPath));
        Console.ResetColor();
    }
    catch (RateLimitedException)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("You have been rate limited. Try a longer polling interval");
        Console.ResetColor();
        Console.ReadKey();
        return;
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