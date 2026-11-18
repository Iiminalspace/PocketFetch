using Discord.WebSocket;
using PocketFetch.Bot;
using PocketFetch.CardData;
using PocketFetch.Cards;
using PocketFetch.Replies;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddUserSecrets<Program>(optional: true);
builder.Services.Configure<DiscordOptions>(builder.Configuration.GetSection("Discord"));
builder.Services.Configure<CardDataOptions>(builder.Configuration.GetSection("CardData"));
builder.Services.Configure<ReplyOptions>(builder.Configuration.GetSection("Replies"));

builder.Services.AddSingleton<CardCatalog>();

builder.Services.AddHttpClient<PocketDecksClient>();
builder.Services.AddSingleton<CardCacheFile>();
builder.Services.AddHostedService<CatalogRefreshService>();

builder.Services.AddSingleton<CardReplyBuilder>();

builder.Services.AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
{
    GatewayIntents = DiscordBotService.RequiredIntents,
}));
builder.Services.AddSingleton<MessageHandler>();
builder.Services.AddHostedService<DiscordBotService>();

builder.Build().Run();
