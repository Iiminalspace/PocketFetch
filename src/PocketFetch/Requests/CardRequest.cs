namespace PocketFetch.Requests;

public sealed record CardRequest(
    CardRequestKind Kind,
    string Name,
    string? SetCode = null,
    int? Number = null);
