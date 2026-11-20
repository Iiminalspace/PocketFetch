namespace PocketFetch.CardData;

public sealed class CardDataOptions
{
    public string DatasetUrl { get; set; } =
        "https://cdn.jsdelivr.net/npm/pokemon-tcg-pocket-cards@5/data/v5/cards.min.json";

    /// <summary>The newest published package. Its major version is the newest data version.</summary>
    public string LatestVersionUrl { get; set; } =
        "https://registry.npmjs.org/pokemon-tcg-pocket-cards/latest";

    public TimeSpan RefreshInterval { get; set; } = TimeSpan.FromHours(24);

    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromMinutes(5);

    public string CacheFilePath { get; set; } = "card-cache.json";
}
