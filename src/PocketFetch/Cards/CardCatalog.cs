namespace PocketFetch.Cards;

public sealed class CardCatalog
{
    private volatile IReadOnlyList<Card> _cards = [];

    public bool IsLoaded => _cards.Count > 0;
    public int CardCount => _cards.Count;
    public int SetCount => _cards.Select(card => card.Print.SetCode).Distinct().Count();

    public void Load(IEnumerable<Card> cards)
    {
        // A bare name finds the everyday card: regular prints before promos and special art,
        // then the oldest first. Early promos have no release date, so date alone is not enough.
        _cards = cards
            .OrderBy(card => HowSpecialIsThePrint(card.Print.Rarity))
            .ThenBy(card => card.ReleaseDate ?? DateOnly.MaxValue)
            .ThenBy(card => card.Print.SetCode)
            .ThenBy(card => card.Print.Number)
            .ToList();
    }

    public Card? Find(string name, string? setCode = null, int? number = null)
    {
        var cards = _cards;

        var cardsToSearch = setCode is null
            ? cards
            : cards.Where(card => SetCode.AreSame(card.Print.SetCode, setCode)).ToList();

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

    private static int HowSpecialIsThePrint(string rarity) => rarity switch
    {
        _ when rarity.StartsWith('◊') => 0,
        "Promo" => 1,
        _ => 2,
    };
}
