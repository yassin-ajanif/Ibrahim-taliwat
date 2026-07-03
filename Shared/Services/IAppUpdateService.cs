namespace GestionCommerciale.Shared.Services;

public interface IAppUpdateService
{
    string? CurrentVersion { get; }
    string DisplayVersion { get; }
    bool IsInstalled { get; }
    bool IsUpdateAvailable { get;}
    string? AvailableVersion { get; }
    bool IsUpdateDownloaded { get; }
    bool IsCheckingForUpdates { get; }

    event EventHandler? UpdateStateChanged;

    Task CheckForUpdatesAsync(CancellationToken cancellationToken = default);
    Task DownloadAndApplyUpdateAsync(CancellationToken cancellationToken = default);
}
