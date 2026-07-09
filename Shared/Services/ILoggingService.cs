namespace GestionCommerciale.Shared.Services;

public interface ILoggingService
{
    void LogError(string message, Exception? exception = null, string? context = null);
}
