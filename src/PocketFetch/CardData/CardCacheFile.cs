using Microsoft.Extensions.Options;

namespace PocketFetch.CardData;

public sealed class CardCacheFile(IOptions<CardDataOptions> options)
{
    public string FilePath => Path.Combine(AppContext.BaseDirectory, options.Value.CacheFilePath);

    public async Task<string?> ReadAsync(CancellationToken cancellationToken) =>
        File.Exists(FilePath)
            ? await File.ReadAllTextAsync(FilePath, cancellationToken)
            : null;

    public async Task SaveAsync(string json, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);

        var tempPath = FilePath + ".tmp";
        await File.WriteAllTextAsync(tempPath, json, cancellationToken);
        File.Move(tempPath, FilePath, overwrite: true);
    }
}
