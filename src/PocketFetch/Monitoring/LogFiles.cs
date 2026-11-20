using Serilog;
using Serilog.Extensions.Logging;

namespace PocketFetch.Monitoring;

public static class LogFiles
{
    private const long MaxFileSizeBytes = 10 * 1024 * 1024;
    private const int MaxFilesKept = 100;

    public static void AddDailyLogFiles(this ILoggingBuilder logging, IConfiguration configuration)
    {
        var options = configuration.GetSection("LogFiles").Get<LogFileOptions>() ?? new LogFileOptions();
        if (options.Folder.Length == 0) return;

        // Added as a plain provider so the Logging section's levels apply to the files too.
        var fileLogger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.File(
                path: Path.Combine(options.Folder, "pocketfetch-.log"),
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}",
                rollingInterval: RollingInterval.Day,
                retainedFileTimeLimit: options.KeepFor,
                retainedFileCountLimit: MaxFilesKept,
                fileSizeLimitBytes: MaxFileSizeBytes,
                rollOnFileSizeLimit: true)
            .CreateLogger();

        logging.AddProvider(new SerilogLoggerProvider(fileLogger, dispose: true));
    }
}
