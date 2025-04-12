using Microsoft.Extensions.Logging;
using Serilog;

namespace ModelContextProtocolServer.Stdio;

internal static class LoggerHelper
{
    internal static ILoggerFactory CreateLoggerFactory(string applicationName)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", $"{applicationName}.log"),
                rollingInterval: RollingInterval.Day,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        return LoggerFactory.Create(builder =>
        {
            //builder.AddConsole(options =>
            //{
            //    options.LogToStandardErrorThreshold = LogLevel.Trace;
            //});
            builder.AddSerilog();
        });
    }
}