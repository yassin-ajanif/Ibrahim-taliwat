using GestionCommerciale.Shared.Database;
using Microsoft.EntityFrameworkCore;

namespace GestionCommerciale.Shared.Services;

public sealed class AppSettingsService : IAppSettingsService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public AppSettingsService(IDbContextFactory<AppDbContext> dbFactory) => _dbFactory = dbFactory;

    public async Task<AppSettingsRow> GetAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        var row = await db.AppSettings.AsNoTracking().FirstOrDefaultAsync(a => a.Id == 1, cancellationToken);
        return row ?? new AppSettingsRow { Id = 1 };
    }

    public async Task SaveAsync(AppSettingsRow row, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        row.Id = 1;
        var existing = await db.AppSettings.FindAsync([1], cancellationToken);
        if (existing == null)
            db.AppSettings.Add(row);
        else
        {
            db.Entry(existing).CurrentValues.SetValues(row);
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
