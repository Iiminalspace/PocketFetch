using System.Text.RegularExpressions;

namespace PocketFetch.Requests;

public static partial class CardRequestParser
{
    // [[name]]  [[!name]]  [[?name]]  [[#name]]  [[name|set]]  [[name|set|number]]
    [GeneratedRegex(@"\[\[(?<prefix>[!?#]?)(?<name>[^\[\]|]+)(\|(?<set>[^\[\]|]*))?(\|(?<number>[^\[\]|]*))?\]\]")]
    private static partial Regex CardRequestPattern();

    public static IReadOnlyList<CardRequest> Parse(string message) =>
        CardRequestPattern()
            .Matches(message)
            .Select(ToCardRequest)
            .Where(request => request.Name.Length > 0)
            .Distinct()
            .ToList();

    private static CardRequest ToCardRequest(Match match)
    {
        var setCode = match.Groups["set"].Value.Trim();
        var hasNumber = int.TryParse(match.Groups["number"].Value, out var number);

        return new CardRequest(
            Kind: ToKind(match.Groups["prefix"].Value),
            Name: match.Groups["name"].Value.Trim(),
            SetCode: setCode.Length > 0 ? setCode : null,
            Number: hasNumber ? number : null);
    }

    private static CardRequestKind ToKind(string prefix) => prefix switch
    {
        "!" => CardRequestKind.Image,
        "?" => CardRequestKind.HowToGet,
        "#" => CardRequestKind.Prints,
        _ => CardRequestKind.Text,
    };
}
