namespace PocketFetch.Replies;

public sealed class ReplyOptions
{
    public int MaxCardsPerMessage { get; set; } = 5;

    /// <summary>
    /// Shown in reply footers when set. The card data is AGPL-licensed,
    /// which asks that people using the bot can find its source code.
    /// </summary>
    public string SourceUrl { get; set; } = "";
}
