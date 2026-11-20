using PocketFetch.Cards;

namespace PocketFetch.CardData;

public static class PocketDecksCardMapper
{
    public static Card ToCard(PocketDecksCardDto dto) => new()
    {
        Name = dto.Name,
        Print = new CardPrint(ToDisplaySetCode(dto.SetCode), dto.SetName, NumberFromId(dto.Id), dto.Rarity),
        Category = dto.Type,
        Subtype = dto.Subtype,
        SecondType = dto.SecondSubtype,
        Stage = dto.Stage,
        EvolvesFrom = dto.EvolvesFrom,
        Health = dto.Health,
        RetreatCost = dto.Retreat,
        Weakness = dto.Weakness is null or "none" ? null : dto.Weakness,
        Ability = ToAbility(dto.Ability),
        Attacks = ToAttacks(dto.Attacks),
        TrainerEffect = dto.CardText,
        FlavorText = dto.FlavorText,
        Artist = dto.Artist,
        ImageUrl = dto.ImagePng,
        Pack = dto.Pack,
        ReleaseDate = dto.ReleaseDate,
        PackPoints = dto.PackPoints,
        IsTradable = dto.Tradable,
        TradeCost = dto.TradeCost,
    };

    public static string ToDisplaySetCode(string setCode)
    {
        if (setCode.Length == 0) return setCode;

        var isPromoSet = setCode.Length == 2 && setCode[0] == 'p';
        return isPromoSet
            ? $"P-{char.ToUpperInvariant(setCode[1])}"
            : char.ToUpperInvariant(setCode[0]) + setCode[1..];
    }

    private static int NumberFromId(string id)
    {
        var numberText = id[(id.LastIndexOf('-') + 1)..];
        return int.TryParse(numberText, out var number) ? number : 0;
    }

    private static Ability? ToAbility(PocketDecksAbilityDto? ability) =>
        ability is { Exists: true, Name: not null }
            ? new Ability(ability.Name, ability.Effect ?? "")
            : null;

    private static IReadOnlyList<Attack> ToAttacks(Dictionary<string, PocketDecksAttackDto> attackSlots) =>
        attackSlots
            .OrderBy(slot => slot.Key)
            .Select(slot => slot.Value)
            .Where(attack => attack.Name is not null)
            .Select(attack => new Attack(
                attack.Name!,
                EnergyCost: (attack.Cost ?? "").Replace("0", ""),
                attack.Damage,
                attack.Effect))
            .ToList();
}
