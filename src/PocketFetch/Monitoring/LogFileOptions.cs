namespace PocketFetch.Monitoring;

public sealed class LogFileOptions
{
    /// <summary>Leave empty to log to the console only.</summary>
    public string Folder { get; set; } = "";

    public TimeSpan KeepFor { get; set; } = TimeSpan.FromDays(30);
}
