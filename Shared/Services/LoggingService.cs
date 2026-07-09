using System.Text;
using GestionCommerciale.Shared.Database;

namespace GestionCommerciale.Shared.Services;

public sealed class LoggingService : ILoggingService
{
    private static readonly object WriteLock = new();
    private static readonly string LogFilePath =
        Path.Combine(DatabasePath.GetDirectory(), "logError");

    public void LogError(string message, Exception? exception = null, string? context = null)
    {
        try
        {
            var entry = BuildEntry(message, exception, context);
            lock (WriteLock)
            {
                Directory.CreateDirectory(DatabasePath.GetDirectory());
                File.AppendAllText(LogFilePath, entry, Encoding.UTF8);
            }
        }
        catch
        {
            // Logging must never crash the application.
        }
    }

    private static string BuildEntry(string message, Exception? exception, string? context)
    {
        var sb = new StringBuilder();
        sb.Append('[').Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")).Append("] ");

        if (!string.IsNullOrWhiteSpace(context))
            sb.Append('[').Append(context).Append("] ");

        sb.AppendLine(message);

        if (exception != null)
        {
            sb.AppendLine(exception.ToString());
            for (var inner = exception.InnerException; inner != null; inner = inner.InnerException)
                sb.AppendLine("Inner: " + inner);
        }

        sb.AppendLine(new string('-', 80));
        return sb.ToString();
    }
}
