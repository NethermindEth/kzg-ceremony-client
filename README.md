# KZG Ceremony Client
[EIP4844](https://www.eip4844.com/) aka proto-danksharding, utilizes KZG proofs to ensure blob availablility. KZG proofs relies on a trusted setup to generate a Structured Reference String (SRS) that is required during the proof verification phase. The goal is to have as many people contribute their 'randomness' to the trusted setup and as long as one participant has done the setup corrrectly, ie. discard their randonmess (toxic waste), the entire trusted setup is deemed to be safe.

## Motivation
Most existing KZG client implementations are written in rust or golang. We wrote a KZG client in C# in hope of providing more language diversity to the existing clients.

## Our Approach
We utlize C bindings for [blst](https://github.com/supranational/blst) library to perform our group operations

## Prerequisites
- [.Net 7.0 Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)

## How to use

## Build from this repo
Ensure you have downloaded the [.Net 7.0 Sdk](https://dotnet.microsoft.com/en-us/download/dotnet/7.0). 
Git clone this repo and at the root directory, run:
```
dotnet build -c Release
```

The executable file is located at `Nethermind.KZGCli/bin/Release/net7.0/kzg`

You will then invoke command as such: 

```
./kzg github
```