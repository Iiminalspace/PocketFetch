using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using PocketFetch.Cards;

namespace PocketFetch.CardData;

public sealed partial class PocketDecksClient(HttpClient http, IOptions<CardDataOptions> options)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
    };

    public Task<string> DownloadJsonAsync(CancellationToken cancellationToken) =>
        http.GetStringAsync(options.Value.DatasetUrl, cancellationToken);

    public async Task<DataVersions?> CheckNewerDataSourceVersionAsync(CancellationToken cancellationToken)
    {
        var versionInUse = PackageMajorVersion().Match(options.Value.DatasetUrl);
        if (!versionInUse.Success) return null;

        var latestPackage = await http.GetFromJsonAsync<NpmPackage>(options.Value.LatestVersionUrl, cancellationToken)
            ?? throw new InvalidDataException("The package registry returned no version");

        var versions = new DataVersions(
            InUse: int.Parse(versionInUse.Groups["major"].Value),
            Newest: int.Parse(latestPackage.Version.Split('.')[0]));

        return versions.Newest > versions.InUse ? versions : null;
    }

    public static IReadOnlyList<Card> ParseCards(string json)
    {
        var dtos = JsonSerializer.Deserialize<List<PocketDecksCardDto>>(json, JsonOptions) ?? [];
        return dtos.Select(PocketDecksCardMapper.ToCard).ToList();
    }

    [GeneratedRegex(@"pokemon-tcg-pocket-cards@(?<major>\d+)")]
    private static partial Regex PackageMajorVersion();

    private sealed record NpmPackage(string Version);
}
