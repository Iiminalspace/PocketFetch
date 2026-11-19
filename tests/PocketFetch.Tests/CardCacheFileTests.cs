using Microsoft.Extensions.Options;
using PocketFetch.CardData;

namespace PocketFetch.Tests;

public class CardCacheFileTests
{
    [Fact]
    public void A_relative_path_is_kept_next_to_the_app()
    {
        var cacheFile = new CardCacheFile(Options.Create(new CardDataOptions { CacheFilePath = "card-cache.json" }));

        Assert.Equal(Path.Combine(AppContext.BaseDirectory, "card-cache.json"), cacheFile.FilePath);
    }

    [Fact]
    public void An_absolute_path_is_used_as_given()
    {
        var dataFolderPath = Path.Combine(Path.GetTempPath(), "pocketfetch", "card-cache.json");

        var cacheFile = new CardCacheFile(Options.Create(new CardDataOptions { CacheFilePath = dataFolderPath }));

        Assert.Equal(dataFolderPath, cacheFile.FilePath);
    }
}
