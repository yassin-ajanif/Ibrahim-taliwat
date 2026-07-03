using GestionCommerciale.Modules.Facturation.Models;
using GestionCommerciale.Modules.FactureFournisseur.Models;
using GestionCommerciale.Shared.Database;
using GestionCommerciale.Shared.Helpers;
using Microsoft.EntityFrameworkCore;

namespace GestionCommerciale.Modules.FactureFournisseur.Services;

public sealed class FactureFournisseurWorkflowService : IFactureFournisseurWorkflowService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public FactureFournisseurWorkflowService(IDbContextFactory<AppDbContext> dbFactory) => _dbFactory = dbFactory;

    public async Task AddPaiementAsync(int factureFournisseurId, PaiementFournisseur paiement, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        var f = await db.FacturesFournisseurs
            .Include(x => x.Paiements)
            .Include(x => x.Lignes)
            .FirstAsync(x => x.Id == factureFournisseurId, cancellationToken);

        DocumentTotalsHelper.SyncFactureFournisseurTotalTtc(f);
        var ttc = f.TotalTtc;
        var totalApres = f.Paiements.Sum(p => p.Montant) + paiement.Montant;
        DocumentTotalsHelper.EnsurePaymentsNotOverTtc(ttc, totalApres);

        paiement.FactureFournisseurId = factureFournisseurId;
        db.PaiementsFournisseurs.Add(paiement);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdatePaiementAsync(int factureFournisseurId, int paiementId, decimal montant, DateTime date, ModePaiement mode, string reference, CancellationToken cancellationToken = default)
    {
        if (montant <= 0)
            throw new InvalidOperationException("Le montant doit être supérieur à 0.");

        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        var f = await db.FacturesFournisseurs
            .Include(x => x.Paiements)
            .Include(x => x.Lignes)
            .FirstAsync(x => x.Id == factureFournisseurId, cancellationToken);

        DocumentTotalsHelper.SyncFactureFournisseurTotalTtc(f);
        var ttc = f.TotalTtc;
        var totalApres = f.Paiements.Where(x => x.Id != paiementId).Sum(x => x.Montant) + montant;
        DocumentTotalsHelper.EnsurePaymentsNotOverTtc(ttc, totalApres);

        var p = await db.PaiementsFournisseurs.FirstAsync(x => x.Id == paiementId && x.FactureFournisseurId == factureFournisseurId, cancellationToken);
        p.Montant = montant;
        p.Date = date;
        p.Mode = mode;
        p.Reference = reference;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeletePaiementAsync(int factureFournisseurId, int paiementId, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        var p = await db.PaiementsFournisseurs.FirstAsync(x => x.Id == paiementId && x.FactureFournisseurId == factureFournisseurId, cancellationToken);
        db.PaiementsFournisseurs.Remove(p);
        await db.SaveChangesAsync(cancellationToken);
    }
}
