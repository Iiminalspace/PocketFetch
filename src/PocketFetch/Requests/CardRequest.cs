namespace PocketFetch.Requests;

public sealed record CardRequest(
    CardRequestKind Kind,
    string Name,
    string? Set = null,
    int? Number = null);
