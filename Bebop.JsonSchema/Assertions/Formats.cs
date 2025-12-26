using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Xml;

namespace Bebop.JsonSchema.Assertions;

internal static partial class Formats
{
    private static readonly Dictionary<string, Func<string, bool>> _formatValidators = new()
    {
        ["email"] = MatchEmail,
        ["idn-email"] = MatchIdnEmail,
        ["ipv4"] = MatchIpv4,
        ["ipv6"] = MatchIpv6,
        ["uri"] = MatchUri,
        ["uuid"] = MatchUuid,
        ["date-time"] = MatchDateTime,
        ["date"] = MatchDate,
        ["time"] = MatchTime,
        ["duration"] = MatchDuration,
    };

    private static bool MatchUuid(string arg)
    {
        return Guid.TryParse(arg, out _);
    }

    private static bool MatchUri(string arg)
    {
        return Uri.TryCreate(arg, UriKind.Absolute, out _);
    }

    private static bool MatchIpv6(string arg)
    {
        return IPAddress.TryParse(arg, out var address) && address.AddressFamily == AddressFamily.InterNetworkV6;
    }

    private static bool MatchIpv4(string arg)
    {
        return IPAddress.TryParse(arg, out var address) && address.AddressFamily == AddressFamily.InterNetwork;
    }

    private static bool MatchEmail(string s)
    {
        return Patterns.Email().IsMatch(s);
    }

    private static bool MatchIdnEmail(string s)
    {
        // TODO

        return true;
    }

    private static bool MatchDateTime(string s)
    {
        return DateTimeOffset.TryParse(s, out _);
    }

    private static bool MatchDate(string s)
    {
        return DateOnly.TryParse(s, out _);
    }

    private static bool MatchTime(string s)
    {
        return TimeOnly.TryParse(s, out _);
    }

    private static bool MatchDuration(string s)
    {
        try 
        { 
            XmlConvert.ToTimeSpan(s); 
            return true; 
        }
        catch 
        {
            return false; 
        }
    }

    internal static Func<string, bool> Get(string v)
    {
        if (_formatValidators.TryGetValue(v, out var func))
        {
            return func;
        }

        return _ => true;
    }

    internal partial class Patterns
    {
        [GeneratedRegex(@"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Singleline)]
        public static partial Regex Email();


    }
}
