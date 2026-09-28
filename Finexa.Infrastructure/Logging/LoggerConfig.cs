using Serilog;
using System;
using System.IO;

namespace Finexa.Infrastructure.Logging
{
    public static class LoggerConfig
    {
        public static void ConfigureLogging()
        {
            var folder = Environment.SpecialFolder.LocalApplicationData;
            var path = Environment.GetFolderPath(folder);
            var logDir = Path.Combine(path, "Finexa", "Logs");
            Directory.CreateDirectory(logDir);
            var logPath = Path.Combine(logDir, "log-.txt");

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(
                    logPath, 
                    rollingInterval: RollingInterval.Day, 
                    retainedFileCountLimit: 30,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            Log.Information("Logging initialized. Logs are stored in {LogDir}", logDir);
        }
    }
}
