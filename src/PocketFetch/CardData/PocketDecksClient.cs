using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using PocketFetch.Cards;

namespace PocketFetch.CardData;

public sealed class PocketDecksClient(HttpClient http, IOptions<CardDataOptions> options)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
    };

    public Task<string> DownloadJsonAsync(CancellationToken cancellationToken) =>
        http.GetStringAsync(options.Value.DatasetUrl, cancellationToken);

    public static IReadOnlyList<Card> ParseCards(string json)
    {
        var dtos = JsonSerializer.Deserialize<List<PocketDecksCardDto>>(json, JsonOptions) ?? [];
        return dtos.Select(PocketDecksCardMapper.ToCard).ToList();
    }
}
