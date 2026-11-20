using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using PocketFetch.CardData;
using PocketFetch.Cards;

namespace PocketFetch.Tests;

public sealed class CatalogRefreshServiceTests : IDisposable
{
    private readonly string _cacheFilePath =
        Path.Combine(Path.GetTempPath(), $"pocketfetch-test-{Guid.NewGuid():N}", "card-cache.json");

    private readonly CardCatalog _catalog = new();

    public void Dispose()
    {
        var testFolder = Path.GetDirectoryName(_cacheFilePath)!;
        if (Directory.Exists(testFolder)) Directory.Delete(testFolder, recursive: true);
    }

    [Fact]
    public async Task A_good_download_fills_the_catalog_and_is_saved()
    {
        var service = CreateService(download: () => Respond(SampleCardData.ButterfreeAndPotionJson));

        var refreshed = await service.TryRefreshFromDownloadAsync(CancellationToken.None);

        Assert.True(refreshed);
        Assert.Equal(2, _catalog.CardCount);
        Assert.Equal(SampleCardData.ButterfreeAndPotionJson, File.ReadAllText(_cacheFilePath));
    }

    [Fact]
    public async Task After_a_restart_the_saved_copy_is_used_while_the_download_fails()
    {
        SaveCopy(SampleCardData.ButterfreeAndPotionJson);
        var service = CreateService(download: () => throw new HttpRequestException("Site unreachable"));

        await service.LoadFromCacheAsync(CancellationToken.None);
        var refreshed = await service.TryRefreshFromDownloadAsync(CancellationToken.None);

        Assert.False(refreshed);
        Assert.Equal(2, _catalog.CardCount);
    }

    [Fact]
    public async Task A_download_that_times_out_is_retried_rather_than_crashing()
    {
        // HttpClient reports a timeout as a cancellation, even though nobody canceled.
        var service = CreateService(download: () => throw new TaskCanceledException("The request timed out"));

        var refreshed = await service.TryRefreshFromDownloadAsync(CancellationToken.None);

        Assert.False(refreshed);
    }

    [Fact]
    public async Task An_empty_download_keeps_the_cards_already_loaded_and_the_saved_copy()
    {
        SaveCopy(SampleCardData.ButterfreeAndPotionJson);
        var service = CreateService(download: () => Respond("[]"));
        await service.LoadFromCacheAsync(CancellationToken.None);

        var refreshed = await service.TryRefreshFromDownloadAsync(CancellationToken.None);

        Assert.False(refreshed);
        Assert.Equal(2, _catalog.CardCount);
        Assert.Equal(SampleCardData.ButterfreeAndPotionJson, File.ReadAllText(_cacheFilePath));
    }

    [Fact]
    public async Task With_no_saved_copy_the_catalog_waits_for_a_download()
    {
        var service = CreateService(download: () => Respond(SampleCardData.ButterfreeAndPotionJson));

        await service.LoadFromCacheAsync(CancellationToken.None);

        Assert.False(_catalog.IsLoaded);
    }

    [Fact]
    public async Task A_damaged_saved_copy_is_ignored()
    {
        SaveCopy("this is not card data");
        var service = CreateService(download: () => Respond(SampleCardData.ButterfreeAndPotionJson));

        await service.LoadFromCacheAsync(CancellationToken.None);

        Assert.False(_catalog.IsLoaded);
    }

    private CatalogRefreshService CreateService(Func<HttpResponseMessage> download)
    {
        var options = Options.Create(new CardDataOptions
        {
            DatasetUrl = "https://example.test/cards.json",
            CacheFilePath = _cacheFilePath,
        });

        var pocketDecks = new PocketDecksClient(new HttpClient(new FakeDownload(download)), options);

        return new CatalogRefreshService(
            pocketDecks, new CardCacheFile(options), _catalog, options, NullLogger<CatalogRefreshService>.Instance);
    }

    private void SaveCopy(string json)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_cacheFilePath)!);
        File.WriteAllText(_cacheFilePath, json);
    }

    private static HttpResponseMessage Respond(string json) =>
        new(HttpStatusCode.OK) { Content = new StringContent(json) };

    private sealed class FakeDownload(Func<HttpResponseMessage> respond) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(respond());
    }
}
