namespace PocketFetch.Monitoring;

public sealed class HeartbeatOptions
{
    /// <summary>A healthchecks.io ping URL. Leave empty to turn the heartbeat off.</summary>
    public string PingUrl { get; set; } = "";

    public TimeSpan Interval { get; set; } = TimeSpan.FromMinutes(5);
}
