using GestionCommerciale.Modules.Livraison.Models;
using GestionCommerciale.Modules.Stock.Services;
using GestionCommerciale.Shared.Database;
using Microsoft.EntityFrameworkCore;

namespace GestionCommerciale.Modules.Livraison.Services;

public sealed class BonLivraisonWorkflowService : IBonLivraisonWorkflowService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly IStockMovementService _stock;

    public BonLivraisonWorkflowService(IDbContextFactory<AppDbContext> dbFactory, IStockMovementService stock)
    {
        _dbFactory = dbFactory;
        _stock = stock;
    }

    public async Task ValiderAsync(int bonLivraisonId, int? userId, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        await using var trx = await db.Database.BeginTransactionAsync(cancellationToken);

        var bl = await db.BonsLivraison.Include(b => b.Lignes).FirstAsync(b => b.Id == bonLivraisonId, cancellationToken);

        await _stock.ResyncBonLivraisonStockAsync(
            db,
            bonLivraisonId,
            bl.Numero,
            bl.Lignes.Select(l => (l.ProduitId, l.QuantiteLivree)),
            userId,
            cancellationToken);

        await db.SaveChangesAsync(cancellationToken);
        await trx.CommitAsync(cancellationToken);
    }

    public async Task ResyncStockFromLinesAsync(int bonLivraisonId, int? userId, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        await using var trx = await db.Database.BeginTransactionAsync(cancellationToken);

        var bl = await db.BonsLivraison.Include(b => b.Lignes).FirstAsync(b => b.Id == bonLivraisonId, cancellationToken);

        await _stock.ResyncBonLivraisonStockAsync(
            db,
            bonLivraisonId,
            bl.Numero,
            bl.Lignes.Select(l => (l.ProduitId, l.QuantiteLivree)),
            userId,
            cancellationToken);

        await db.SaveChangesAsync(cancellationToken);
        await trx.CommitAsync(cancellationToken);
    }
}
