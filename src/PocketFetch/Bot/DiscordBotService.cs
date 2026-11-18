using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Options;

namespace PocketFetch.Bot;

public sealed class DiscordBotService(
    DiscordSocketClient discord,
    MessageHandler messageHandler,
    IOptions<DiscordOptions> options,
    ILogger<DiscordBotService> logger) : IHostedService
{
    // MessageContent must also be switched on for the bot in the Discord Developer Portal.
    // Without it the bot connects normally but receives empty messages, with no error.
    public const GatewayIntents RequiredIntents =
        GatewayIntents.Guilds |
        GatewayIntents.GuildMessages |
        GatewayIntents.MessageContent;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var token = options.Value.Token;
        if (string.IsNullOrWhiteSpace(token))
        {
            logger.LogError("No Discord token found");
            return;
        }

        discord.Log += ForwardDiscordLog;
        discord.Ready += AnnounceConnected;
        discord.MessageReceived += messageHandler.HandleAsync;

        await discord.LoginAsync(TokenType.Bot, token);
        await discord.StartAsync();
    }

    public Task StopAsync(CancellationToken cancellationToken) => discord.StopAsync();

    private Task AnnounceConnected()
    {
        logger.LogInformation("Connected to Discord as {BotName} in {ServerCount} servers",
            discord.CurrentUser.Username, discord.Guilds.Count);
        return Task.CompletedTask;
    }

    private Task ForwardDiscordLog(LogMessage message)
    {
        var level = message.Severity switch
        {
            LogSeverity.Critical => LogLevel.Critical,
            LogSeverity.Error => LogLevel.Error,
            LogSeverity.Warning => LogLevel.Warning,
            LogSeverity.Info => LogLevel.Information,
            _ => LogLevel.Debug,
        };

        logger.Log(level, message.Exception, "Discord: {Message}", message.Message);
        return Task.CompletedTask;
    }
}
