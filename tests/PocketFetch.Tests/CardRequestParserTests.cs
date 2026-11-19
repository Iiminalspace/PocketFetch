using PocketFetch.Requests;

namespace PocketFetch.Tests;

public class CardRequestParserTests
{
    [Fact]
    public void A_plain_name_asks_for_card_text()
    {
        var request = Assert.Single(CardRequestParser.Parse("is {{Pikachu ex}} still good?"));

        Assert.Equal(new CardRequest(CardRequestKind.Text, "Pikachu ex"), request);
    }

    [Theory]
    [InlineData("{{!Mewtwo ex}}", CardRequestKind.Image)]
    [InlineData("{{?Mewtwo ex}}", CardRequestKind.HowToGet)]
    [InlineData("{{#Mewtwo ex}}", CardRequestKind.Prints)]
    public void A_prefix_chooses_what_to_show(string message, CardRequestKind expectedKind)
    {
        var request = Assert.Single(CardRequestParser.Parse(message));

        Assert.Equal(new CardRequest(expectedKind, "Mewtwo ex"), request);
    }

    [Fact]
    public void A_set_code_can_follow_the_name()
    {
        var request = Assert.Single(CardRequestParser.Parse("{{Pikachu ex|A1}}"));

        Assert.Equal(new CardRequest(CardRequestKind.Text, "Pikachu ex", Set: "A1"), request);
    }

    [Fact]
    public void A_set_code_and_number_point_at_one_print()
    {
        var request = Assert.Single(CardRequestParser.Parse("{{!Pikachu ex | A1 | 96}}"));

        Assert.Equal(new CardRequest(CardRequestKind.Image, "Pikachu ex", Set: "A1", Number: 96), request);
    }

    [Fact]
    public void Several_cards_in_one_message_are_all_found_in_order()
    {
        var requests = CardRequestParser.Parse("{{Bulbasaur}} evolves into {{Ivysaur}} then {{!Venusaur}}");

        Assert.Equal(["Bulbasaur", "Ivysaur", "Venusaur"], requests.Select(request => request.Name));
    }

    [Fact]
    public void The_same_request_written_twice_is_answered_once()
    {
        var requests = CardRequestParser.Parse("{{Erika}} and again {{Erika}}");

        Assert.Single(requests);
    }

    [Theory]
    [InlineData("no brackets here")]
    [InlineData("[single brackets]")]
    [InlineData("{{}}")]
    [InlineData("{{   }}")]
    [InlineData("{{unclosed")]
    public void Messages_without_a_lookup_give_nothing(string message)
    {
        Assert.Empty(CardRequestParser.Parse(message));
    }

    [Fact]
    public void Square_brackets_are_left_to_the_Scryfall_bot()
    {
        Assert.Empty(CardRequestParser.Parse("[[Pikachu ex]] and [[Lightning Bolt]]"));
    }

    [Fact]
    public void A_set_name_can_follow_the_name()
    {
        var request = Assert.Single(CardRequestParser.Parse("{{Pikachu|Genetic Apex}}"));

        Assert.Equal(new CardRequest(CardRequestKind.Text, "Pikachu", Set: "Genetic Apex"), request);
    }

    [Theory]
    [InlineData("{{sets}}", CardRequestKind.SetList)]
    [InlineData("{{ Sets }}", CardRequestKind.SetList)]
    [InlineData("{{help}}", CardRequestKind.Help)]
    [InlineData("{{HELP}}", CardRequestKind.Help)]
    public void Command_words_ask_for_lists_instead_of_cards(string message, CardRequestKind expectedKind)
    {
        var request = Assert.Single(CardRequestParser.Parse(message));

        Assert.Equal(expectedKind, request.Kind);
    }

    [Theory]
    [InlineData("{{#sets}}")]
    [InlineData("{{sets|A1}}")]
    [InlineData("{{!help}}")]
    public void Command_words_with_a_prefix_or_set_are_card_lookups(string message)
    {
        var request = Assert.Single(CardRequestParser.Parse(message));

        Assert.True(request.Kind is not (CardRequestKind.SetList or CardRequestKind.Help));
    }

    [Fact]
    public void Price_lookups_are_not_a_feature()
    {
        var request = Assert.Single(CardRequestParser.Parse("{{$Charizard ex}}"));

        Assert.Equal(new CardRequest(CardRequestKind.Text, "$Charizard ex"), request);
    }
}
