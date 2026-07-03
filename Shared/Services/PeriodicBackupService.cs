using System.Timers;
using Timer = System.Timers.Timer;

namespace GestionCommerciale.Shared.Services;

public sealed class PeriodicBackupService : IPeriodicBackupService, IDisposable
{
    private readonly IAppSettingsService _settings;
    private readonly IBackupService _backup;
    private Timer? _timer;

    public PeriodicBackupService(IAppSettingsService settings, IBackupService backup)
    {
        _settings = settings;
        _backup = backup;
    }

    public void Start()
    {
        Stop();
        _timer = new Timer(TimeSpan.FromSeconds(30)) { AutoReset = true };
        _timer.Elapsed += OnTimerElapsed;
        _timer.Start();
    }

    public void Stop()
    {
        if (_timer is not null)
        {
            _timer.Stop();
            _timer.Elapsed -= OnTimerElapsed;
            _timer.Dispose();
            _timer = null;
        }
    }

    private async void OnTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        try
        {
            var settings = await _settings.GetAsync(default);
            if (!settings.BackupEnabled)
                return;

            var now = DateTime.UtcNow;
            TimeSpan threshold = settings.BackupIntervalUnit == "Minutes"
                ? TimeSpan.FromMinutes(settings.BackupIntervalHours)
                : TimeSpan.FromHours(settings.BackupIntervalHours);

            if (settings.LastBackupDate.HasValue &&
                (now - settings.LastBackupDate.Value) < threshold)
                return;

            var result = await _backup.CreateBackupAsync(settings.BackupDirectory, default);
            if (result is not null)
            {
                settings.LastBackupDate = now;
                await _settings.SaveAsync(settings, default);
                await _backup.CleanupOldBackupsAsync(settings.BackupDirectory, settings.BackupRetentionDays, default);
            }
        }
        catch
        {
            // silent — don't crash the app on backup failure
        }
    }

    public void Dispose()
    {
        Stop();
    }
}
