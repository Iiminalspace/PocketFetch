namespace PocketFetch.Cards;

public sealed record Card
{
    public required string Name { get; init; }
    public required CardPrint Print { get; init; }
    public required string Category { get; init; }

    /// <summary>The type of a Pokémon ("Grass"), or the kind of Trainer ("Supporter").</summary>
    public required string Subtype { get; init; }

    public string? SecondType { get; init; }
    public string? Stage { get; init; }
    public string? EvolvesFrom { get; init; }
    public int? Health { get; init; }
    public int? RetreatCost { get; init; }
    public string? Weakness { get; init; }

    public Ability? Ability { get; init; }
    public IReadOnlyList<Attack> Attacks { get; init; } = [];
    public string? TrainerEffect { get; init; }

    public string? FlavorText { get; init; }
    public string? Artist { get; init; }
    public string? ImageUrl { get; init; }

    public string? Pack { get; init; }
    public DateOnly? ReleaseDate { get; init; }
    public int? PackPoints { get; init; }
    public bool IsTradable { get; init; }
    public int? TradeCost { get; init; }
}
