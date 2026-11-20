using System.Globalization;
using System.Text;

namespace PocketFetch.Cards;

public static class NameMatcher
{
    public static string? FindBestMatch(string typedName, IEnumerable<string> cardNames)
    {
        var typed = Simplify(typedName);
        if (typed.Length == 0) return null;

        var typedWithoutSpaces = WithoutSpaces(typed);
        var names = cardNames.Distinct().Select(name => new SimplifiedName(name, Simplify(name))).ToList();

        // Spaces are ignored when looking for the typed letters, so "porygonz" finds "Porygon-Z",
        // but kept when counting typos, so short names are not treated as near misses.
        var exact = names.FirstOrDefault(n => n.WithoutSpaces == typedWithoutSpaces);
        if (exact is not null) return exact.Original;

        // For partial matches, the shortest name is the one closest to what was typed.
        var startsWith = names.Where(n => n.WithoutSpaces.StartsWith(typedWithoutSpaces, StringComparison.Ordinal)).MinBy(n => n.Simplified.Length);
        if (startsWith is not null) return startsWith.Original;

        var contains = names.Where(n => n.WithoutSpaces.Contains(typedWithoutSpaces, StringComparison.Ordinal)).MinBy(n => n.Simplified.Length);
        if (contains is not null) return contains.Original;

        var closest = names.MinBy(n => CountTypos(typed, n.Simplified));
        var isCloseEnough = closest is not null && CountTypos(typed, closest.Simplified) <= AllowedTypos(typed);
        return isCloseEnough ? closest!.Original : null;
    }

    public static IReadOnlyList<string> FindClosest(string typedName, IEnumerable<string> cardNames, int count)
    {
        var typed = Simplify(typedName);
        var mostTyposWorthSuggesting = typed.Length / 2;

        return cardNames
            .Distinct()
            .Select(name => (Name: name, Typos: CountTypos(typed, Simplify(name))))
            .Where(candidate => candidate.Typos <= mostTyposWorthSuggesting)
            .OrderBy(candidate => candidate.Typos)
            .ThenBy(candidate => candidate.Name)
            .Take(count)
            .Select(candidate => candidate.Name)
            .ToList();
    }

    private static string Simplify(string name)
    {
        var simplified = new StringBuilder();

        foreach (var character in ReplaceGenderSymbolsWithLetters(name).Normalize(NormalizationForm.FormD))
        {
            var isAccentMark = CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark;
            if (isAccentMark) continue;

            if (char.IsLetterOrDigit(character))
                simplified.Append(char.ToLowerInvariant(character));
            else if (simplified.Length > 0 && simplified[^1] != ' ')
                simplified.Append(' ');
        }

        return simplified.ToString().Trim();
    }

    private static string WithoutSpaces(string simplifiedName) => simplifiedName.Replace(" ", "");

    private static string ReplaceGenderSymbolsWithLetters(string name) =>
        name.Replace("♀", " f").Replace("♂", " m");

    private static int AllowedTypos(string typed) => Math.Max(1, typed.Length / 4);

    private static int CountTypos(string from, string to)
    {
        var previousRow = Enumerable.Range(0, to.Length + 1).ToArray();

        for (var i = 1; i <= from.Length; i++)
        {
            var currentRow = new int[to.Length + 1];
            currentRow[0] = i;

            for (var j = 1; j <= to.Length; j++)
            {
                var swapCost = from[i - 1] == to[j - 1] ? 0 : 1;
                currentRow[j] = Math.Min(
                    Math.Min(currentRow[j - 1] + 1, previousRow[j] + 1),
                    previousRow[j - 1] + swapCost);
            }

            previousRow = currentRow;
        }

        return previousRow[to.Length];
    }

    private sealed record SimplifiedName(string Original, string Simplified)
    {
        public string WithoutSpaces { get; } = NameMatcher.WithoutSpaces(Simplified);
    }
}
