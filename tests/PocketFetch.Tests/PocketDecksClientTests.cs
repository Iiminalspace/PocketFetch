using PocketFetch.CardData;
using PocketFetch.Cards;

namespace PocketFetch.Tests;

public class PocketDecksClientTests
{
    private readonly IReadOnlyList<Card> _cards = PocketDecksClient.ParseCards(SampleCardData.ButterfreeAndPotionJson);

    private Card Butterfree => _cards[0];
    private Card Potion => _cards[1];

    [Fact]
    public void Every_record_becomes_a_card()
    {
        Assert.Equal(["Butterfree", "Potion"], _cards.Select(card => card.Name));
    }

    [Fact]
    public void The_print_is_read_from_the_id_and_set()
    {
        Assert.Equal(new CardPrint("A1", "Genetic Apex", 7, "◊◊◊"), Butterfree.Print);
        Assert.Equal(new CardPrint("P-A", "Promo-A", 1, "Promo"), Potion.Print);
    }

    [Fact]
    public void Pokemon_stats_are_read()
    {
        Assert.Equal("Pokémon", Butterfree.Category);
        Assert.Equal("Grass", Butterfree.Subtype);
        Assert.Equal("Stage 2", Butterfree.Stage);
        Assert.Equal("Metapod", Butterfree.EvolvesFrom);
        Assert.Equal(120, Butterfree.Health);
        Assert.Equal(1, Butterfree.RetreatCost);
        Assert.Equal("Fire", Butterfree.Weakness);
        Assert.Equal(new DateOnly(2024, 10, 30), Butterfree.ReleaseDate);
    }

    [Fact]
    public void A_real_ability_is_kept()
    {
        Assert.Equal(new Ability("Powder Heal", "Heal 20 damage from each of your Pokémon."), Butterfree.Ability);
    }

    [Fact]
    public void Flavor_text_is_read_from_the_british_spelling()
    {
        Assert.Equal("It flaps its wings at great speed.", Butterfree.FlavorText);
    }

    [Fact]
    public void Empty_attack_slots_are_dropped()
    {
        var attack = Assert.Single(Butterfree.Attacks);

        Assert.Equal(new Attack("Gust", "GCC", 60, null), attack);
    }

    [Fact]
    public void A_trainer_has_an_effect_and_no_battle_stats()
    {
        Assert.Equal("Trainer", Potion.Category);
        Assert.Equal("Heal 20 damage from 1 of your Pokémon.", Potion.TrainerEffect);
        Assert.Null(Potion.Ability);
        Assert.Empty(Potion.Attacks);
        Assert.Null(Potion.Health);
        Assert.Null(Potion.ReleaseDate);
        Assert.False(Potion.IsTradable);
    }

    [Theory]
    [InlineData("a1", "A1")]
    [InlineData("a4b", "A4b")]
    [InlineData("pa", "P-A")]
    [InlineData("pc", "P-C")]
    public void Set_codes_are_shown_the_way_players_write_them(string dataSetCode, string expected)
    {
        Assert.Equal(expected, PocketDecksCardMapper.ToDisplaySetCode(dataSetCode));
    }
}
