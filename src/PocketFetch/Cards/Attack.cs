namespace PocketFetch.Cards;

/// <param name="EnergyCost">One letter per energy: "GCC" is Grass, Colorless, Colorless.</param>
public sealed record Attack(string Name, string EnergyCost, int? Damage, string? Effect);
