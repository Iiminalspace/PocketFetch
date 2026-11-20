using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Options;
using PocketFetch.Cards;

namespace PocketFetch.Monitoring;

public sealed class HeartbeatService(
    IHttpClientFactory httpClientFactory,
    DiscordSocketClient discord,
    CardCatalog catalog,
    IOptions<HeartbeatOptions> options,
    ILogger<HeartbeatService> logger) : BackgroundService
{
    private bool CanAnswerRequests => discord.ConnectionState == ConnectionState.Connected && catalog.IsLoaded;

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var pingUrl = options.Value.PingUrl;
        if (pingUrl.Length == 0) return;

        logger.LogInformation("Sending a heartbeat every {Minutes} minutes", options.Value.Interval.TotalMinutes);

        using var timer = new PeriodicTimer(options.Value.Interval);
        while (await timer.WaitForNextTickAsync(cancellationToken))
        {
            if (CanAnswerRequests)
                await PingAsync(pingUrl, cancellationToken);
        }
    }

    private async Task PingAsync(string pingUrl, CancellationToken cancellationToken)
    {
        try
        {
            using var httpClient = httpClientFactory.CreateClient();
            using var response = await httpClient.GetAsync(pingUrl, cancellationToken);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning("Could not send the heartbeat: {Problem}", exception.Message);
        }
    }
}
