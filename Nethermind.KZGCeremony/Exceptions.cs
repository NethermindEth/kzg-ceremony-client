using System.Runtime.Serialization;

namespace Nethermind.KZGCeremony;

public class ContributionException : System.Exception { }

public class ContributionAbortException : System.Exception { }

public class RateLimitedException : System.Exception { }

public class UnauthorizedException : System.Exception { }
