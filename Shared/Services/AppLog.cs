namespace GestionCommerciale.Shared.Services;

public static class AppLog
{
    private static ILoggingService? _loggingService;

    public static void Initialize(ILoggingService loggingService) =>
        _loggingService = loggingService;

    public static void Error(string message, Exception? exception = null, string? context = null) =>
        _loggingService?.LogError(message, exception, context);

    public static void Error(Exception exception, string? context = null) =>
        Error(exception.Message, exception, context);
}
