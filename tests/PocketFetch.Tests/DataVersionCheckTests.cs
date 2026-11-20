using System.Net;
using Microsoft.Extensions.Options;
using PocketFetch.CardData;

namespace PocketFetch.Tests;

public class DataVersionCheckTests
{
    private const string Version5Url = "https://cdn.jsdelivr.net/npm/pokemon-tcg-pocket-cards@5/data/v5/cards.min.json";

    [Fact]
    public async Task A_newer_major_version_is_reported()
    {
        var client = CreateClient(Version5Url, latestPackageJson: """{"name":"pokemon-tcg-pocket-cards","version":"6.0.1"}""");

        var versions = await client.CheckNewerDataSourceVersionAsync(CancellationToken.None);

        Assert.Equal(new DataVersions(InUse: 5, Newest: 6), versions);
    }

    [Theory]
    [InlineData("5.0.0")]
    [InlineData("5.9.3")]
    public async Task Newer_releases_of_the_same_version_are_not_reported(string latestVersion)
    {
        var client = CreateClient(Version5Url, latestPackageJson: $$"""{"version":"{{latestVersion}}"}""");

        Assert.Null(await client.CheckNewerDataSourceVersionAsync(CancellationToken.None));
    }

    [Fact]
    public async Task A_dataset_url_without_a_package_version_is_not_checked()
    {
        var client = CreateClient("https://example.test/cards.json", latestPackageJson: """{"version":"9.0.0"}""");

        Assert.Null(await client.CheckNewerDataSourceVersionAsync(CancellationToken.None));
    }

    private static PocketDecksClient CreateClient(string datasetUrl, string latestPackageJson)
    {
        var options = Options.Create(new CardDataOptions { DatasetUrl = datasetUrl });
        var registry = new FakeRegistry(latestPackageJson);

        return new PocketDecksClient(new HttpClient(registry), options);
    }

    private sealed class FakeRegistry(string latestPackageJson) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(latestPackageJson, System.Text.Encoding.UTF8, "application/json"),
            });
    }
}
