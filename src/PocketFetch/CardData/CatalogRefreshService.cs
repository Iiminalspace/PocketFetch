using Microsoft.Extensions.Options;
using PocketFetch.Cards;

namespace PocketFetch.CardData;

public sealed class CatalogRefreshService(
    PocketDecksClient pocketDecksClient,
    CardCacheFile cacheFile,
    CardCatalog catalog,
    IOptions<CardDataOptions> options,
    ILogger<CatalogRefreshService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await LoadFromCacheAsync(cancellationToken);

        while (!cancellationToken.IsCancellationRequested)
        {
            var refreshed = await TryRefreshFromDownloadAsync(cancellationToken);
            var waitTime = refreshed ? options.Value.RefreshInterval : options.Value.RetryDelay;

            await Task.Delay(waitTime, cancellationToken);
        }
    }

    public async Task LoadFromCacheAsync(CancellationToken cancellationToken)
    {
        try
        {
            var json = await cacheFile.ReadAsync(cancellationToken);
            if (json is not null)
                LoadCatalog(json, source: "saved copy");
        }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(exception, "Could not read the saved card data in {CacheFile}; waiting for a download instead",
                cacheFile.FilePath);
        }
    }

    public async Task<bool> TryRefreshFromDownloadAsync(CancellationToken cancellationToken)
    {
        string json;
        try
        {
            json = await pocketDecksClient.DownloadJsonAsync(cancellationToken);
            LoadCatalog(json, source: "download");
        }
        // Catching exceptions here prevents the service from stopping if the download fails.
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogError(exception, "Could not download card data; trying again in {RetryMinutes} minutes",
                options.Value.RetryDelay.TotalMinutes);
            return false;
        }

        await TrySaveToCacheAsync(json, cancellationToken);
        return true;
    }

    private void LoadCatalog(string json, string source)
    {
        var cards = PocketDecksClient.ParseCards(json);
        if (cards.Count == 0)
            throw new InvalidDataException($"The card data {source} contained no cards, so the current cards were kept");

        catalog.Load(cards);
        logger.LogInformation("Loaded {CardCount:N0} cards from {SetCount} sets using the {Source}",
            catalog.CardCount, catalog.SetCount, source);
    }

    private async Task TrySaveToCacheAsync(string json, CancellationToken cancellationToken)
    {
        try
        {
            await cacheFile.SaveAsync(json, cancellationToken);
        }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(exception, "Could not save card data to {CacheFile}.",
                cacheFile.FilePath);
        }
    }
}
