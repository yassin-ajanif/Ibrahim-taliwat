using GestionCommerciale.Shared.Database;

namespace GestionCommerciale.Shared.Services;

public interface IAppSettingsService
{
    Task<AppSettingsRow> GetAsync(CancellationToken cancellationToken = default);
    Task SaveAsync(AppSettingsRow row, CancellationToken cancellationToken = default);
}
