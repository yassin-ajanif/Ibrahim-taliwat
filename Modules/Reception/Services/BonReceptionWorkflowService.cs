using GestionCommerciale.Modules.Reception.Models;
using GestionCommerciale.Modules.Stock.Services;
using GestionCommerciale.Shared.Database;
using Microsoft.EntityFrameworkCore;

namespace GestionCommerciale.Modules.Reception.Services;

public sealed class BonReceptionWorkflowService : IBonReceptionWorkflowService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly IStockMovementService _stock;

    public BonReceptionWorkflowService(IDbContextFactory<AppDbContext> dbFactory, IStockMovementService stock)
    {
        _dbFactory = dbFactory;
        _stock = stock;
    }

    public async Task ValiderAsync(int bonReceptionId, int? userId, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        await using var trx = await db.Database.BeginTransactionAsync(cancellationToken);

        var br = await db.BonsReception
            .Include(b => b.Lignes)
            .FirstAsync(b => b.Id == bonReceptionId, cancellationToken);

        await ReplayBonReceptionLinesIntoStockAsync(db, br, userId, cancellationToken);

        await db.SaveChangesAsync(cancellationToken);
        await trx.CommitAsync(cancellationToken);
    }

    public async Task ResyncStockFromLinesAsync(int bonReceptionId, int? userId, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        await using var trx = await db.Database.BeginTransactionAsync(cancellationToken);

        var br = await db.BonsReception
            .Include(b => b.Lignes)
            .FirstAsync(b => b.Id == bonReceptionId, cancellationToken);

        await ReplayBonReceptionLinesIntoStockAsync(db, br, userId, cancellationToken);

        await db.SaveChangesAsync(cancellationToken);
        await trx.CommitAsync(cancellationToken);
    }

    private Task ReplayBonReceptionLinesIntoStockAsync(
        AppDbContext db,
        BonReception br,
        int? userId,
        CancellationToken cancellationToken)
    {
        return _stock.SyncBonReceptionStockAsync(
            db,
            br.Id,
            br.Numero,
            br.Lignes
                .Where(l => l.QuantiteRecue > 0)
                .Select(l => (l.ProduitId, l.QuantiteRecue, l.PrixUnitaireHT)),
            userId,
            cancellationToken);
    }
}
