# KZG Ceremony Client
[EIP4844](https://www.eip4844.com/) aka proto-danksharding, utilizes KZG proofs to ensure blob availablility. KZG proofs relies on a trusted setup to generate a Structured Reference String (SRS) that is required during the proof verification phase. The goal is to have as many people contribute their 'randomness' to the trusted setup and as long as one participant has done the setup corrrectly, ie. discard their randonmess (toxic waste), the entire trusted setup is deemed to be safe.

## Motivation
Most existing KZG client implementations are written in rust or golang. We wrote a KZG client in C# in hope of providing more language diversity to the existing clients.

## Our Approach
We utlize C bindings for [blst](https://github.com/supranational/blst) library to perform our group operations

## How to use
### Installing and executing
Download the zip file according to your OS and architecutre from the [release](https://github.com/NethermindEth/kzg-ceremony-client/releases) page. 

Unzip your file and in your terminal, test the cli by running: 
```
./kzg --help
```

You should be seeing something like this:
```
Description:
  Contribute to Ethereum's KZG Ceremony

Usage:
  kzg [command] [options]

Options:
  --file <file>          File containing randomness
  --url <url>            Url of KZG ceremony sequencer [default: 
                         https://seq.ceremony.ethereum.org]
  --interval <interval>  Polling time interval in ms [default: 30000]
  --version              Show version information
  -?, -h, --help         Show help and usage information

Commands:
  ethereum  Contribute using Ethereum address
  github    Contribute using Github handle
```

*For mac users, you may be prompted a message saying 'Apple cannot check it for malicious software.'. Refer to [this](https://support.apple.com/en-my/guide/mac-help/mchleab3a043/mac) by Apple to resolve the issue*

### Passing in sequencer url
By default. the cli points to `https://seq.ceremony.ethereum.org` . To change that, you will be able to pass the sequencer url through a flag.

eg.
```
./kzg github --url https://kzg-ceremony-sequencer-dev.fly.dev
```

### Passing in randomness
By default, the randomness is generated in the cli. Randomness is generated using the RNG in [System.Security.Cryptography](https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.randomnumbergenerator?view=net-7.0)

To pass in custom randomness, you may use the following command:

``` shell
./kzg github --file my_randomness.txt
```

### Contributing via Ethereum address or Github
The cli expects an `option` or either `ethereum` or `github`.

To contribute using an Ethereum address:
``` shell
./kzg ethereum
```

To contribute using a Github account:
``` shell
./kzg github
```

### Obtaining sessionId
After selecting your contibution method, you will be prompted a url as such:

```
Visit this link and enter your session Id below.
https://github.com/login/oauth/authorize?response_type=code&client_id=xxxxxxx&redirect_uri=https%3A%2F%2Fkzg-ceremony-sequencer-dev.fly.dev%2Fauth%2Fcallback%2Fgithub
```

Copy and paste the link to your browser and authorized the kzg ceremony sequencer to connect to your account. 
It will then return our a json containing the session Id.

``` json
{"id_token":{"exp":18446744073709551615,"nickname":"xxxx","provider":"Github","sub":"git|yyyy|xxxx"},"session_id":"my_session_id"}
```

Copy and paste the `session_id` in the cli running the kzg client

```
Enter Session id: 

my_session_id
```

### Wait, contribute & Success 🎉
After which, the cli will query the sequencer to wait for our turn to contribute. The polling interval is 30seconds.

If successful, you will see something like:
```
Waiting for our turn to contribute...
Yes! Our time to contribute!
Updating Powers of Tau
Sending contribution...
Contribution completed 
Contribution written to /Users/contribution.json
ContributionReceipt written to /Users/contributionReceipt.json

```

You may then check your contribution and contribution receipt in the file path mentioned above.

Congratulation!!

## Build from this repo
Ensure you have downloaded the [.Net 7.0 Sdk](https://dotnet.microsoft.com/en-us/download/dotnet/7.0). 
Git clone this repo and at the root directory, run:

```sh
dotnet publish Nethermind.KZGCli/Nethermind.KZGCli.csproj -c Release -o .
# or just
make
```

An executable file named `kzg` will appear.

You will then invoke command as such: 

```sh
./kzg github
```