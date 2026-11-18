using System.Text.RegularExpressions;
using Discord;

namespace PocketFetch.Replies;

public sealed record EnergyType(char Letter, string Name, string Symbol, Color Color);

public static partial class EnergyTypes
{
    public static readonly IReadOnlyList<EnergyType> All =
    [
        new('G', "Grass", "<:Grass:1572366901420753088>", new Color(0x5DBB63)),
        new('R', "Fire", "<:Fire:1572367177842041043>", new Color(0xE0533D)),
        new('W', "Water", "<:Water:1572367182212374609>", new Color(0x4A90D9)),
        new('L', "Lightning", "<:Lightning:1572367179037544650>", new Color(0xF2C94C)),
        new('P', "Psychic", "<:Psychic:1572367181176373390>", new Color(0x9B59B6)),
        new('F', "Fighting", "<:Fighting:1572367176583610531>", new Color(0xC8752F)),
        new('D', "Darkness", "<:Darkness:1572367173622431755>", new Color(0x2F3A4A)),
        new('M', "Metal", "<:Metal:1572367180211683370>", new Color(0x8A9BA8)),
        new('N', "Dragon", "<:Dragon:1572367174540984491>", new Color(0xB8A038)),
        new('C', "Colorless", "<:Colorless:1572367172456423465>", new Color(0xD9D9D9)),
    ];

    private static readonly Color TrainerColor = new(0x7F8C8D);

    [GeneratedRegex(@"\[\s*(?<letter>[A-Z])\s*\]")]
    private static partial Regex BracketedEnergyLetter();

    public static string ToSymbols(string energyCost) =>
        string.Concat(energyCost.Select(ToSymbol));

    public static string ShowSymbolsIn(string cardText) =>
        BracketedEnergyLetter().Replace(cardText, match => ToSymbol(match.Groups["letter"].Value[0]));

    public static string SymbolOf(string typeName) =>
        All.FirstOrDefault(energy => energy.Name == typeName)?.Symbol ?? typeName;

    public static Color ColorOf(string typeName) =>
        All.FirstOrDefault(energy => energy.Name == typeName)?.Color ?? TrainerColor;

    private static string ToSymbol(char letter) =>
        All.FirstOrDefault(energy => energy.Letter == letter)?.Symbol ?? $"[{letter}]";
}
