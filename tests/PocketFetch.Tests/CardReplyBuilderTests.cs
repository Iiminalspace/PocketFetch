using Microsoft.Extensions.Options;
using PocketFetch.Cards;
using PocketFetch.Replies;
using PocketFetch.Requests;

namespace PocketFetch.Tests;

public class CardReplyBuilderTests
{
    private static readonly Card Butterfree = new()
    {
        Name = "Butterfree",
        Print = new CardPrint("A1", "Genetic Apex", 7, "◊◊◊"),
        Category = "Pokémon",
        Subtype = "Grass",
        Stage = "Stage 2",
        EvolvesFrom = "Metapod",
        Health = 120,
        RetreatCost = 1,
        Weakness = "Fire",
        Ability = new Ability("Powder Heal", "Heal 20 damage from each of your [G] Pokémon."),
        Attacks = [new Attack("Gust", "GCC", 60, null)],
        ImageUrl = "https://example.test/a1/007.png",
        Pack = "Pikachu",
        ReleaseDate = new DateOnly(2024, 10, 30),
        PackPoints = 150,
        IsTradable = true,
        TradeCost = 1200,
    };

    private static readonly Card ButterfreePromo =
        Butterfree with { Print = new CardPrint("P-A", "Promo-A", 13, "Promo"), ReleaseDate = null };

    private static readonly Card Potion = new()
    {
        Name = "Potion",
        Print = new CardPrint("P-A", "Promo-A", 1, "Promo"),
        Category = "Trainer",
        Subtype = "Item",
        TrainerEffect = "Heal 20 damage from 1 of your Pokémon.",
    };

    private readonly CardReplyBuilder _replyBuilder = CreateReplyBuilder(sourceUrl: "");

    [Fact]
    public void Card_text_shows_stats_ability_and_attacks()
    {
        const string grass = "<:Grass:1572366901420753088>";
        const string colorless = "<:Colorless:1572367172456423465>";
        const string fire = "<:Fire:1572367177842041043>";

        var reply = _replyBuilder.Build(CardRequestKind.Text, Butterfree);

        Assert.Equal($"Butterfree — ʜᴘ120 {grass}", reply.Title);
        Assert.Contains("Stage 2 Pokémon · Grass", reply.Description);
        Assert.Contains("Evolves from Metapod", reply.Description);
        Assert.Contains("**Ability: Powder Heal**", reply.Description);
        Assert.Contains($"your {grass} Pokémon", reply.Description);
        Assert.Contains($"{grass}{colorless}{colorless} **Gust** 60", reply.Description);
        Assert.Contains($"-# weakness: {fire} +20 · retreat: {colorless}", reply.Description);
        Assert.Equal("https://example.test/a1/007.png", reply.Thumbnail?.Url);
    }

    [Fact]
    public void A_dual_type_title_shows_both_types()
    {
        var reply = _replyBuilder.Build(CardRequestKind.Text, Butterfree with { SecondType = "Darkness" });

        Assert.EndsWith("<:Grass:1572366901420753088><:Darkness:1572367173622431755>", reply.Title);
    }

    [Fact]
    public void Trainer_text_shows_its_effect_without_battle_stats()
    {
        var reply = _replyBuilder.Build(CardRequestKind.Text, Potion);

        Assert.Equal("Potion", reply.Title);
        Assert.Contains("Trainer · Item", reply.Description);
        Assert.Contains("Heal 20 damage from 1 of your Pokémon.", reply.Description);
        Assert.DoesNotContain("weakness", reply.Description);
    }

    [Fact]
    public void An_image_reply_shows_the_full_card()
    {
        var reply = _replyBuilder.Build(CardRequestKind.Image, Butterfree);

        Assert.Equal("https://example.test/a1/007.png", reply.Image?.Url);
        Assert.Null(reply.Description);
    }

    [Fact]
    public void How_to_get_lists_pack_crafting_and_trading()
    {
        var reply = _replyBuilder.Build(CardRequestKind.HowToGet, Butterfree);
        var fields = reply.Fields.ToDictionary(field => field.Name, field => field.Value);

        Assert.Equal("Genetic Apex (A1) #7", fields["Set"]);
        Assert.Equal("Pikachu", fields["Pack"]);
        Assert.Equal("150 pack points", fields["Crafting"]);
        Assert.Equal("Tradable (cost 1,200)", fields["Trading"]);
    }

    [Fact]
    public void A_promo_cannot_be_crafted_or_traded()
    {
        var reply = _replyBuilder.Build(CardRequestKind.HowToGet, Potion);
        var fields = reply.Fields.ToDictionary(field => field.Name, field => field.Value);

        Assert.Equal("Cannot be crafted", fields["Crafting"]);
        Assert.Equal("Not tradable", fields["Trading"]);
    }

    [Fact]
    public void Prints_list_every_card_with_the_name_oldest_first()
    {
        var reply = _replyBuilder.Build(CardRequestKind.Prints, ButterfreePromo);
        var lines = reply.Description.Split('\n');

        Assert.Equal(2, lines.Length);
        Assert.Contains("A1 #7", lines[0]);
        Assert.Contains("P-A #13", lines[1]);
        Assert.Null(reply.Footer);
    }

    [Fact]
    public void No_weakness_and_a_free_retreat_are_left_blank()
    {
        var reply = _replyBuilder.Build(CardRequestKind.Text, Butterfree with { Weakness = null, RetreatCost = 0 });

        Assert.EndsWith("weakness: · retreat:", reply.Description.TrimEnd());
    }

    [Fact]
    public void A_long_prints_list_is_cut_short()
    {
        var catalog = new CardCatalog();
        catalog.Load(Enumerable.Range(1, 35).Select(number => Butterfree with { Print = Butterfree.Print with { Number = number } }));
        var replyBuilder = new CardReplyBuilder(catalog, Options.Create(new ReplyOptions()));

        var reply = replyBuilder.Build(CardRequestKind.Prints, Butterfree);
        var lines = reply.Description.Split('\n');

        Assert.Equal(31, lines.Length);
        Assert.Equal("…and 5 more", lines[^1]);
    }

    [Fact]
    public void Every_card_reply_links_to_the_card_and_names_the_print()
    {
        var reply = _replyBuilder.Build(CardRequestKind.Text, Butterfree);

        Assert.Equal("https://pocket.limitlesstcg.com/cards/A1/7", reply.Url);
        Assert.Equal("Genetic Apex · A1 #7 · ◊◊◊", reply.Footer?.Text);
    }

    [Fact]
    public void The_footer_names_the_source_code_when_configured()
    {
        var replyBuilder = CreateReplyBuilder(sourceUrl: "https://example.test/pocketfetch");

        var reply = replyBuilder.Build(CardRequestKind.Text, Butterfree);

        Assert.EndsWith("· Source: https://example.test/pocketfetch", reply.Footer?.Text);
    }

    [Fact]
    public void Not_found_names_what_was_asked_and_offers_suggestions()
    {
        var request = new CardRequest(CardRequestKind.Text, "Buterfre", Set: "A1");

        var reply = _replyBuilder.BuildNotFound(request, ["Butterfree", "Butterfree ex"]);

        Assert.Contains("No card found for \"Buterfre\" in set A1.", reply.Description);
        Assert.Contains("Did you mean **Butterfree**, **Butterfree ex**?", reply.Description);
    }

    [Fact]
    public void The_set_list_names_each_set_with_its_code_and_link()
    {
        var reply = _replyBuilder.BuildSetList();
        var lines = reply.Description.Split('\n');

        Assert.Equal("Sets", reply.Title);
        Assert.Equal("[**A1**](https://pocket.limitlesstcg.com/cards/A1) · Genetic Apex · 1 card", lines[0]);
        Assert.Equal(2, lines.Length);
    }

    private static CardReplyBuilder CreateReplyBuilder(string sourceUrl)
    {
        var catalog = new CardCatalog();
        catalog.Load([Butterfree, ButterfreePromo, Potion]);

        return new CardReplyBuilder(catalog, Options.Create(new ReplyOptions { SourceUrl = sourceUrl }));
    }
}
