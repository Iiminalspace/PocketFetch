using System.Text;
using Discord;
using Microsoft.Extensions.Options;
using PocketFetch.Cards;
using PocketFetch.Requests;

namespace PocketFetch.Replies;

public sealed class CardReplyBuilder(CardCatalog catalog, IOptions<ReplyOptions> options)
{
    private const int MaxPrintsListed = 30;

    public Embed Build(CardRequestKind kind, Card card) => kind switch
    {
        CardRequestKind.Image => BuildImage(card),
        CardRequestKind.HowToGet => BuildHowToGet(card),
        CardRequestKind.Prints => BuildPrints(card),
        _ => BuildText(card),
    };

    public Embed BuildNotFound(CardRequest request, IReadOnlyList<string> suggestedNames)
    {
        var where = request.Set is null ? "" : $" in set {request.Set}";
        var suggestions = suggestedNames.Count == 0
            ? ""
            : $"\nDid you mean {string.Join(", ", suggestedNames.Select(name => $"**{name}**"))}?";

        return new EmbedBuilder()
            .WithDescription($"No card found for \"{request.Name}\"{where}.{suggestions}")
            .WithColor(Color.DarkGrey)
            .Build();
    }

    public Embed BuildSetList()
    {
        var lines = catalog.Sets.Select(set =>
            $"[**{set.Code}**](https://pocket.limitlesstcg.com/cards/{set.Code}) · {set.Name} · {set.CardCount:N0} {(set.CardCount == 1 ? "card" : "cards")}");

        return new EmbedBuilder()
            .WithTitle("Sets")
            .WithDescription(string.Join('\n', lines))
            .WithColor(Color.DarkGrey)
            .Build();
    }

    private Embed BuildText(Card card)
    {
        var text = new StringBuilder();
        text.AppendLine(DescribeTypeLine(card));

        if (card.EvolvesFrom is not null)
            text.AppendLine($"Evolves from {card.EvolvesFrom}");

        if (card.Ability is not null)
        {
            text.AppendLine();
            text.AppendLine($"**Ability: {card.Ability.Name}**");
            text.AppendLine(EnergyTypes.ShowSymbolsIn(card.Ability.Effect));
        }

        foreach (var attack in card.Attacks)
        {
            text.AppendLine();
            text.AppendLine(DescribeAttack(attack));
            if (attack.Effect is not null)
                text.AppendLine(EnergyTypes.ShowSymbolsIn(attack.Effect));
        }

        if (card.TrainerEffect is not null)
        {
            text.AppendLine();
            text.AppendLine(EnergyTypes.ShowSymbolsIn(card.TrainerEffect));
        }

        if (card.Category != "Trainer")
        {
            text.AppendLine();
            text.AppendLine(DescribeWeaknessAndRetreat(card));
        }

        return StartEmbed(card)
            .WithTitle(DescribeTitle(card))
            .WithDescription(text.ToString())
            .WithThumbnailUrl(card.ImageUrl)
            .WithFooter(DescribeFooter(card))
            .Build();
    }

    private Embed BuildImage(Card card) =>
        StartEmbed(card)
            .WithTitle(card.Name)
            .WithImageUrl(card.ImageUrl)
            .WithFooter(DescribeFooter(card))
            .Build();

    private Embed BuildHowToGet(Card card) =>
        StartEmbed(card)
            .WithTitle($"How to get {card.Name}")
            .WithThumbnailUrl(card.ImageUrl)
            .AddField("Set", $"{card.Print.SetName} ({card.Print.SetCode}) #{card.Print.Number}", inline: true)
            .AddField("Pack", card.Pack ?? "Unknown", inline: true)
            .AddField("Rarity", card.Print.Rarity, inline: true)
            .AddField("Crafting", card.PackPoints is null ? "Cannot be crafted" : $"{card.PackPoints:N0} pack points", inline: true)
            .AddField("Trading", DescribeTrading(card), inline: true)
            .AddField("Released", card.ReleaseDate?.ToString("d MMMM yyyy") ?? "Unknown", inline: true)
            .WithFooter(DescribeFooter(card))
            .Build();

    private Embed BuildPrints(Card card)
    {
        var prints = catalog.PrintsOf(card.Name);
        var lines = prints.Take(MaxPrintsListed).Select(DescribePrint).ToList();
        if (prints.Count > MaxPrintsListed)
            lines.Add($"…and {prints.Count - MaxPrintsListed} more");

        return StartEmbed(card)
            .WithTitle($"Prints of {card.Name}")
            .WithDescription(string.Join('\n', lines))
            .WithThumbnailUrl(card.ImageUrl)
            .Build();
    }

    private EmbedBuilder StartEmbed(Card card) =>
        new EmbedBuilder()
            .WithColor(EnergyTypes.ColorOf(card.Subtype))
            .WithUrl(LimitlessPageFor(card.Print));

    private static string DescribeTitle(Card card)
    {
        if (card.Health is null) return card.Name;

        var types = EnergyTypes.SymbolOf(card.Subtype)
            + (card.SecondType is null ? "" : EnergyTypes.SymbolOf(card.SecondType));

        return $"{card.Name} — ʜᴘ{card.Health} {types}";
    }

    private static string DescribeTypeLine(Card card)
    {
        var types = card.SecondType is null ? card.Subtype : $"{card.Subtype} / {card.SecondType}";

        return card.Category == "Trainer"
            ? $"Trainer · {types}"
            : $"{card.Stage} Pokémon · {types}";
    }

    private static string DescribeAttack(Attack attack)
    {
        var cost = EnergyTypes.ToSymbols(attack.EnergyCost);
        var damage = attack.Damage is null ? "" : $" {attack.Damage}";

        return $"{cost} **{attack.Name}**{damage}".TrimStart();
    }

    private static string DescribeWeaknessAndRetreat(Card card)
    {
        var weakness = card.Weakness is null ? "" : $" {EnergyTypes.SymbolOf(card.Weakness)} +20";
        var retreat = card.RetreatCost is null or 0 ? "" : $" {EnergyTypes.ToSymbols(new string('C', card.RetreatCost.Value))}";

        return $"-# weakness:{weakness} · retreat:{retreat}";
    }

    private static string DescribeTrading(Card card)
    {
        if (!card.IsTradable) return "Not tradable";

        return card.TradeCost is null or 0 ? "Tradable" : $"Tradable (cost {card.TradeCost:N0})";
    }

    private static string DescribePrint(CardPrint print) =>
        $"[**{print.SetCode} #{print.Number}**]({LimitlessPageFor(print)}) · {print.SetName} · {print.Rarity}";

    private string DescribeFooter(Card card)
    {
        var print = card.Print;
        var footer = $"{print.SetName} · {print.SetCode} #{print.Number} · {print.Rarity}";

        return options.Value.SourceUrl.Length > 0
            ? $"{footer} · Source: {options.Value.SourceUrl}"
            : footer;
    }

    private static string LimitlessPageFor(CardPrint print) =>
        $"https://pocket.limitlesstcg.com/cards/{print.SetCode}/{print.Number}";
}
