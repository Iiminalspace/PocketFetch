namespace PocketFetch.Cards;

public static class SetCode
{
    public static bool AreSame(string first, string second) =>
        LettersAndDigitsOnly(first) == LettersAndDigitsOnly(second);

    private static string LettersAndDigitsOnly(string setCode) =>
        new(setCode.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray());
}
