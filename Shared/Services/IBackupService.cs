namespace GestionCommerciale.Shared.Services;

public interface IBackupService
{
    Task<int> GetBackupCountAsync(string backupDir, CancellationToken cancellationToken = default);
    Task<string?> CreateBackupAsync(string backupDir, CancellationToken cancellationToken = default);
    Task CleanupOldBackupsAsync(string backupDir, int retentionDays, CancellationToken cancellationToken = default);
}
