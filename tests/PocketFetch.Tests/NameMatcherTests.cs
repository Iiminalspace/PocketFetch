using PocketFetch.Cards;

namespace PocketFetch.Tests;

public class NameMatcherTests
{
    private static readonly string[] CardNames =
    [
        "Pikachu", "Pikachu ex", "Charizard", "Charizard ex", "Mewtwo ex",
        "Pokémon Center Lady", "Professor's Research", "Nidoran♀", "Nidoran♂", "Type: Null",
        "Porygon", "Porygon-Z", "Ho-Oh", "Poké Ball", "Landorus", "Slakoth",
    ];

    [Theory]
    [InlineData("Pikachu", "Pikachu")]
    [InlineData("pikachu EX", "Pikachu ex")]
    [InlineData("pokemon center lady", "Pokémon Center Lady")]
    [InlineData("professors research", "Professor's Research")]
    [InlineData("type null", "Type: Null")]
    [InlineData("nidoran f", "Nidoran♀")]
    [InlineData("nidoran m", "Nidoran♂")]
    public void Case_accents_and_punctuation_do_not_matter(string typed, string expected)
    {
        Assert.Equal(expected, NameMatcher.FindBestMatch(typed, CardNames));
    }

    [Theory]
    [InlineData("porygonz", "Porygon-Z")]
    [InlineData("porygon z", "Porygon-Z")]
    [InlineData("hooh", "Ho-Oh")]
    [InlineData("pokeball", "Poké Ball")]
    [InlineData("nidoranf", "Nidoran♀")]
    public void Spaces_and_hyphens_do_not_matter(string typed, string expected)
    {
        Assert.Equal(expected, NameMatcher.FindBestMatch(typed, CardNames));
    }

    [Theory]
    [InlineData("mewtwo", "Mewtwo ex")]
    [InlineData("chari", "Charizard")]
    [InlineData("center lady", "Pokémon Center Lady")]
    public void Part_of_a_name_finds_the_shortest_name_containing_it(string typed, string expected)
    {
        Assert.Equal(expected, NameMatcher.FindBestMatch(typed, CardNames));
    }

    [Theory]
    [InlineData("charizrd ex", "Charizard ex")]
    [InlineData("pikachuu", "Pikachu")]
    [InlineData("mewtoo ex", "Mewtwo ex")]
    public void Small_typos_are_forgiven(string typed, string expected)
    {
        Assert.Equal(expected, NameMatcher.FindBestMatch(typed, CardNames));
    }

    [Theory]
    [InlineData("Black Lotus")]
    [InlineData("")]
    [InlineData("!!!")]
    public void Names_that_match_nothing_return_null(string typed)
    {
        Assert.Null(NameMatcher.FindBestMatch(typed, CardNames));
    }

    [Fact]
    public void A_missing_ex_is_not_treated_as_a_typo()
    {
        Assert.Null(NameMatcher.FindBestMatch("Pikachu ex", ["Pikachu", "Raichu"]));
    }

    [Fact]
    public void An_empty_card_list_matches_nothing()
    {
        Assert.Null(NameMatcher.FindBestMatch("Pikachu", []));
    }

    [Fact]
    public void Closest_names_come_back_nearest_first()
    {
        var suggestions = NameMatcher.FindClosest("Charzard", CardNames, count: 2);

        Assert.Equal(["Charizard", "Charizard ex"], suggestions);
    }

    [Fact]
    public void A_wild_guess_gets_no_suggestions()
    {
        Assert.Empty(NameMatcher.FindClosest("Black Lotus", CardNames, count: 3));
    }
}
