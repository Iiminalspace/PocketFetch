namespace PocketFetch.Cards;

public sealed class CardCatalog
{
    private volatile IReadOnlyList<Card> _cards = [];

    public bool IsLoaded => _cards.Count > 0;
    public int CardCount => _cards.Count;
    public int SetCount => Sets.Count;

    public IReadOnlyList<CardSet> Sets =>
        _cards
            .GroupBy(card => card.Print.SetCode)
            .Select(set => new CardSet(
                Code: set.Key,
                Name: set.First().Print.SetName,
                ReleaseDate: set.Min(card => card.ReleaseDate),
                CardCount: set.Count()))
            .OrderBy(set => set.ReleaseDate ?? DateOnly.MaxValue)
            .ThenBy(set => set.Code)
            .ToList();

    public void Load(IEnumerable<Card> cards)
    {
        // Regular prints before promos and special art, then the oldest print first.
        _cards = cards
            .OrderBy(card => PrintDesirability(card.Print.Rarity))
            .ThenBy(card => card.ReleaseDate ?? DateOnly.MaxValue)
            .ThenBy(card => card.Print.SetCode)
            .ThenBy(card => card.Print.Number)
            .ToList();
    }

    public Card? Find(string name, string? set = null, int? number = null)
    {
        var cards = _cards;

        var setCode = set is null ? null : FindSetCode(set);
        if (set is not null && setCode is null) return null;

        var cardsToSearch = setCode is null
            ? cards
            : cards.Where(card => card.Print.SetCode == setCode).ToList();

        var matchedName = NameMatcher.FindBestMatch(name, cardsToSearch.Select(card => card.Name));
        var cardsWithName = cardsToSearch.Where(card => card.Name == matchedName).ToList();

        var exactPrint = setCode is not null && number is not null
            ? cardsWithName.FirstOrDefault(card => card.Print.Number == number)
            : null;

        return exactPrint ?? cardsWithName.FirstOrDefault();
    }

    public IReadOnlyList<CardPrint> PrintsOf(string name) =>
        _cards
            .Where(card => card.Name == name)
            .OrderBy(card => card.ReleaseDate ?? DateOnly.MaxValue)
            .ThenBy(card => card.Print.SetCode)
            .ThenBy(card => card.Print.Number)
            .Select(card => card.Print)
            .ToList();

    public IReadOnlyList<string> SuggestNames(string name, int count) =>
        NameMatcher.FindClosest(name, _cards.Select(card => card.Name), count);

    private string? FindSetCode(string codeOrName)
    {
        var sets = Sets;

        var byCode = sets.FirstOrDefault(set => SetCode.AreSame(set.Code, codeOrName));
        if (byCode is not null) return byCode.Code;

        var matchedName = NameMatcher.FindBestMatch(codeOrName, sets.Select(set => set.Name));
        return sets.FirstOrDefault(set => set.Name == matchedName)?.Code;
    }

    private static int PrintDesirability(string rarity) => rarity switch
    {
        _ when rarity.StartsWith('◊') => 0,
        "Promo" => 1,
        _ => 2,
    };
}
