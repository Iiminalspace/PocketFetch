using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Options;
using PocketFetch.Cards;
using PocketFetch.Replies;
using PocketFetch.Requests;

namespace PocketFetch.Bot;

public sealed class MessageHandler(
    CardCatalog catalog,
    CardReplyBuilder replyBuilder,
    IOptions<ReplyOptions> options,
    ILogger<MessageHandler> logger)
{
    public async Task HandleAsync(SocketMessage message)
    {
        var isFromPerson = message is SocketUserMessage && !message.Author.IsBot;
        if (!isFromPerson) return;

        var requests = CardRequestParser.Parse(message.Content)
            .Take(options.Value.MaxCardsPerMessage)
            .ToList();
        if (requests.Count == 0) return;

        // Discord.NET swallows exceptions thrown from event handlers, so they must be logged here.
        try
        {
            if (!catalog.IsLoaded)
            {
                await message.Channel.SendMessageAsync("Card data is loading. Please try again in a moment.");
                return;
            }

            // One message per card: Discord merges embeds in a single message when they share a link.
            foreach (var request in requests)
                await message.Channel.SendMessageAsync(embed: BuildReply(request), allowedMentions: AllowedMentions.None);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Could not answer message {MessageId}", message.Id);
        }
    }

    private Embed BuildReply(CardRequest request)
    {
        if (request.Kind == CardRequestKind.SetList) return replyBuilder.BuildSetList();
        if (request.Kind == CardRequestKind.Help) return replyBuilder.BuildHelp();

        var card = catalog.Find(request.Name, request.Set, request.Number);

        return card is not null
            ? replyBuilder.Build(request.Kind, card)
            : replyBuilder.BuildNotFound(request, catalog.SuggestNames(request.Name, count: 3));
    }
}
