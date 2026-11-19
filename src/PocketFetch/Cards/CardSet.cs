namespace PocketFetch.Cards;

public sealed record CardSet(string Code, string Name, DateOnly? ReleaseDate, int CardCount);
