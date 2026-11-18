using PocketFetch.Cards;

namespace PocketFetch.Tests;

public class CardCatalogTests
{
    private static readonly DateOnly GeneticApexRelease = new(2024, 10, 30);
    private static readonly DateOnly MythicalIslandRelease = new(2024, 12, 17);

    private readonly CardCatalog _catalog = new();

    public CardCatalogTests()
    {
        // Loaded newest-first on purpose, to prove the catalog does its own ordering.
        _catalog.Load(
        [
            CreateCard("Pikachu ex", "P-A", 15, releaseDate: null),
            CreateCard("Pikachu", "A1a", 25, MythicalIslandRelease),
            CreateCard("Pikachu ex", "A1", 285, GeneticApexRelease),
            CreateCard("Pikachu ex", "A1", 96, GeneticApexRelease),
            CreateCard("Pikachu", "A1", 94, GeneticApexRelease),
            CreateCard("Mewtwo ex", "A1", 129, GeneticApexRelease),
        ]);
    }

    [Fact]
    public void A_new_catalog_is_not_loaded()
    {
        Assert.False(new CardCatalog().IsLoaded);
    }

    [Fact]
    public void A_loaded_catalog_knows_its_size()
    {
        Assert.True(_catalog.IsLoaded);
        Assert.Equal(6, _catalog.CardCount);
        Assert.Equal(3, _catalog.SetCount);
    }

    [Fact]
    public void A_bare_name_finds_the_earliest_print()
    {
        var card = _catalog.Find("Pikachu ex");

        Assert.Equal(new CardPrint("A1", "Set A1", 96, "◊"), card?.Print);
    }

    [Fact]
    public void A_bare_name_prefers_a_regular_print_over_promos_and_special_art()
    {
        var catalog = new CardCatalog();
        catalog.Load(
        [
            CreateCard("Poké Ball", "A2b", 111, GeneticApexRelease, rarity: "Crown Rare"),
            CreateCard("Poké Ball", "P-A", 5, releaseDate: null, rarity: "Promo"),
            CreateCard("Charizard ex", "A1", 280, GeneticApexRelease, rarity: "☆☆☆"),
            CreateCard("Charizard ex", "A2b", 10, MythicalIslandRelease, rarity: "◊◊◊◊"),
        ]);

        Assert.Equal("P-A", catalog.Find("Poké Ball")?.Print.SetCode);
        Assert.Equal("A2b", catalog.Find("Charizard ex")?.Print.SetCode);
    }

    [Fact]
    public void A_set_code_narrows_the_search_to_that_set()
    {
        var card = _catalog.Find("Pikachu", setCode: "A1a");

        Assert.Equal(25, card?.Print.Number);
    }

    [Theory]
    [InlineData("P-A")]
    [InlineData("p-a")]
    [InlineData("PA")]
    public void Set_codes_can_be_written_any_way(string setCode)
    {
        var card = _catalog.Find("Pikachu ex", setCode);

        Assert.Equal("P-A", card?.Print.SetCode);
    }

    [Fact]
    public void A_set_code_and_number_find_one_exact_print()
    {
        var card = _catalog.Find("Pikachu ex", setCode: "A1", number: 285);

        Assert.Equal(285, card?.Print.Number);
    }

    [Fact]
    public void A_number_belonging_to_another_card_does_not_change_the_card_found()
    {
        var card = _catalog.Find("Mewtwo ex", setCode: "A1", number: 94);

        Assert.Equal("Mewtwo ex", card?.Name);
    }

    [Fact]
    public void A_number_without_a_set_is_ignored()
    {
        var card = _catalog.Find("Mewtwo ex", setCode: null, number: 94);

        Assert.Equal("Mewtwo ex", card?.Name);
    }

    [Theory]
    [InlineData("Black Lotus", null)]
    [InlineData("Pikachu", "ZZ9")]
    public void Unknown_names_and_sets_find_nothing(string name, string? setCode)
    {
        Assert.Null(_catalog.Find(name, setCode));
    }

    [Fact]
    public void Prints_of_a_name_cover_every_set_oldest_first()
    {
        var prints = _catalog.PrintsOf("Pikachu ex");

        Assert.Equal(["A1 #96", "A1 #285", "P-A #15"], prints.Select(print => $"{print.SetCode} #{print.Number}"));
    }

    [Fact]
    public void Suggestions_offer_the_nearest_names_once_each()
    {
        var suggestions = _catalog.SuggestNames("Pikachoo", count: 2);

        Assert.Equal(["Pikachu", "Pikachu ex"], suggestions);
    }

    private static Card CreateCard(string name, string setCode, int number, DateOnly? releaseDate, string rarity = "◊") => new()
    {
        Name = name,
        Print = new CardPrint(setCode, $"Set {setCode}", number, rarity),
        Category = "Pokémon",
        Subtype = "Lightning",
        ReleaseDate = releaseDate,
    };
}
