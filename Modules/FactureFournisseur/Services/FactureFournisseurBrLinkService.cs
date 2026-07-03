using GestionCommerciale.Modules.FactureFournisseur.ViewModels;
using GestionCommerciale.Modules.Reception.Models;
using GestionCommerciale.Shared.Database;
using Microsoft.EntityFrameworkCore;

namespace GestionCommerciale.Modules.FactureFournisseur.Services;

public sealed class FactureFournisseurBrLinkService : IFactureFournisseurBrLinkService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public FactureFournisseurBrLinkService(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<List<BonReception>> GetAvailableBrsForFournisseurAsync(int fournisseurId, int? excludeFactureFournisseurId = null, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        var query = db.BonsReception.AsNoTracking()
            .Where(b => b.FournisseurId == fournisseurId && b.FactureFournisseurId == null);

        if (excludeFactureFournisseurId.HasValue)
            query = query.Where(b => b.FactureFournisseurId != excludeFactureFournisseurId.Value);

        return await query
            .Include(b => b.Lignes)
            .OrderBy(b => b.Date).ThenBy(b => b.Numero)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<string>> ValidateBrsForFactureFournisseurAsync(int fournisseurId, IReadOnlyList<int> brIds, CancellationToken cancellationToken = default)
    {
        var errors = new List<string>();

        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        var brs = await db.BonsReception.AsNoTracking()
            .Where(b => brIds.Contains(b.Id))
            .ToListAsync(cancellationToken);

        var foundIds = brs.Select(b => b.Id).ToHashSet();
        foreach (var id in brIds)
        {
            if (!foundIds.Contains(id))
                errors.Add($"BR #{id} introuvable.");
        }

        foreach (var b in brs.Where(b => b.FournisseurId != fournisseurId))
            errors.Add($"{b.Numero} : le fournisseur ne correspond pas à celui de la facture.");

        foreach (var b in brs.Where(b => b.FactureFournisseurId != null))
            errors.Add($"{b.Numero} est déjà facturé.");

        return errors;
    }

    public async Task<List<FactureFournisseurLineRow>> LoadBrLinesAsync(int brId, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        var lines = await db.BonReceptionLignes.AsNoTracking()
            .Where(l => l.BRId == brId)
            .ToListAsync(cancellationToken);

        return lines.Select(l => new FactureFournisseurLineRow
        {
            BonReceptionId = brId,
            ProduitId = l.ProduitId,
            Designation = l.Designation,
            Conditionnement = string.Empty,
            Quantite = l.QuantiteRecue,
            PrixUnitaireHt = l.PrixUnitaireHT,
            Remise = 0,
            TauxTva = l.TauxTVA
        }).ToList();
    }

    public async Task AssignBrsToFactureFournisseurAsync(AppDbContext db, int factureFournisseurId, IReadOnlyList<int> brIds, CancellationToken cancellationToken = default)
    {
        var currentBrs = await db.BonsReception.Where(b => b.FactureFournisseurId == factureFournisseurId).ToListAsync(cancellationToken);
        var targetSet = brIds.ToHashSet();

        foreach (var br in currentBrs)
        {
            if (!targetSet.Contains(br.Id))
                br.FactureFournisseurId = null;
        }

        foreach (var brId in brIds)
        {
            var br = await db.BonsReception.FindAsync(new object[] { brId }, cancellationToken);
            if (br != null)
                br.FactureFournisseurId = factureFournisseurId;
        }
    }

    public async Task<List<BonReception>> GetLinkedBrsAsync(int factureFournisseurId, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        return await db.BonsReception.AsNoTracking()
            .Where(b => b.FactureFournisseurId == factureFournisseurId)
            .OrderBy(b => b.Date).ThenBy(b => b.Numero)
            .ToListAsync(cancellationToken);
    }

    public async Task<string?> GetInvoicingStatusAsync(int brId, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        var br = await db.BonsReception.AsNoTracking()
            .Where(b => b.Id == brId)
            .Select(b => new { b.FactureFournisseurId })
            .FirstOrDefaultAsync(cancellationToken);

        if (br?.FactureFournisseurId is not { } ffId) return null;

        return await db.FacturesFournisseurs.AsNoTracking()
            .Where(f => f.Id == ffId)
            .Select(f => f.Numero)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
