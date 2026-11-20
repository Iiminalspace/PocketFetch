using System.Text.Json.Serialization;

namespace PocketFetch.CardData;

// Mirrors https://github.com/PocketDecks/pokemon-tcg-pocket-cards data/v5/cards.json (snake_case in the JSON).

public sealed record PocketDecksCardDto
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public string SetCode { get; init; } = "";
    public string SetName { get; init; } = "";
    public string? Pack { get; init; }
    public DateOnly? ReleaseDate { get; init; }
    public string Type { get; init; } = "";
    public string Subtype { get; init; } = "";

    [JsonPropertyName("subtype_2")]
    public string? SecondSubtype { get; init; }

    public string? Stage { get; init; }
    public string? EvolvesFrom { get; init; }
    public string Rarity { get; init; } = "";
    public int? PackPoints { get; init; }
    public int? Health { get; init; }
    public int? Retreat { get; init; }
    public string? Weakness { get; init; }
    public PocketDecksAbilityDto? Ability { get; init; }
    public string? CardText { get; init; }
    public Dictionary<string, PocketDecksAttackDto> Attacks { get; init; } = [];
    public bool Tradable { get; init; }
    public int? TradeCost { get; init; }
    public string? Artist { get; init; }
    public string? ImagePng { get; init; }
    [JsonPropertyName("flavour_text")]
    public string? FlavorText { get; init; }
}

public sealed record PocketDecksAbilityDto(bool Exists, string? Name, string? Effect);

public sealed record PocketDecksAttackDto(string? Cost, string? Name, int? Damage, string? Effect);
