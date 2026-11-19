using System.Text.RegularExpressions;

namespace PocketFetch.Requests;

public static partial class CardRequestParser
{
    // {{name}}  {{!name}}  {{?name}}  {{#name}}  {{name|set}}  {{name|set|number}}  {{sets}}
    [GeneratedRegex(@"\{\{(?<prefix>[!?#]?)(?<name>[^{}|]+)(\|(?<set>[^{}|]*))?(\|(?<number>[^{}|]*))?\}\}")]
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
        var name = match.Groups["name"].Value.Trim();
        var set = match.Groups["set"].Value.Trim();
        var hasNumber = int.TryParse(match.Groups["number"].Value, out var number);

        var isSetList = match.Groups["prefix"].Value.Length == 0
            && !match.Groups["set"].Success
            && name.Equals("sets", StringComparison.OrdinalIgnoreCase);
        if (isSetList) return new CardRequest(CardRequestKind.SetList, name);

        return new CardRequest(
            Kind: ToKind(match.Groups["prefix"].Value),
            Name: name,
            Set: set.Length > 0 ? set : null,
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
