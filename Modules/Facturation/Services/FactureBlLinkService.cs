using GestionCommerciale.Modules.Services.Models;
using GestionCommerciale.Modules.Facturation.ViewModels;
using GestionCommerciale.Modules.Livraison.Models;
using GestionCommerciale.Shared.Database;
using Microsoft.EntityFrameworkCore;

namespace GestionCommerciale.Modules.Facturation.Services;

public sealed class FactureBlLinkService : IFactureBlLinkService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public FactureBlLinkService(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<List<BonLivraison>> GetAvailableBlsForClientAsync(int clientId, int? excludeFactureId = null, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        var query = db.BonsLivraison.AsNoTracking()
            .Where(b => b.ClientId == clientId && b.FactureId == null);

        if (excludeFactureId.HasValue)
            query = query.Where(b => b.FactureId != excludeFactureId.Value);

        return await query
            .Include(b => b.Lignes)
            .OrderBy(b => b.Date).ThenBy(b => b.Numero)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<string>> ValidateBlsForFactureAsync(int clientId, IReadOnlyList<int> blIds, CancellationToken cancellationToken = default)
    {
        var errors = new List<string>();

        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        var bls = await db.BonsLivraison.AsNoTracking()
            .Where(b => blIds.Contains(b.Id))
            .ToListAsync(cancellationToken);

        var foundIds = bls.Select(b => b.Id).ToHashSet();
        foreach (var id in blIds)
        {
            if (!foundIds.Contains(id))
                errors.Add($"BL #{id} introuvable.");
        }

        var wrongClient = bls.Where(b => b.ClientId != clientId).ToList();
        foreach (var b in wrongClient)
            errors.Add($"{b.Numero} : le client ne correspond pas à celui de la facture.");

        var alreadyInvoiced = bls.Where(b => b.FactureId != null).ToList();
        foreach (var b in alreadyInvoiced)
            errors.Add($"{b.Numero} est déjà facturé.");

        return errors;
    }

    public async Task<List<FactureLineRow>> LoadBlLinesAsync(int blId, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        var lines = await db.BonLivraisonLignes.AsNoTracking()
            .Where(l => l.BLId == blId)
            .ToListAsync(cancellationToken);

        var serviceIds = lines.Where(l => l.ServiceId.HasValue).Select(l => l.ServiceId!.Value).Distinct().ToList();
        var services = serviceIds.Count == 0
            ? new Dictionary<int, Service>()
            : await db.Services.AsNoTracking()
                .Where(s => serviceIds.Contains(s.Id))
                .ToDictionaryAsync(s => s.Id, cancellationToken);

        return lines.Select(l =>
        {
            var row = new FactureLineRow
            {
                BonLivraisonId = blId,
                ProduitId = l.ProduitId,
                ServiceId = l.ServiceId,
                Designation = l.Designation,
                Quantite = l.QuantiteLivree,
                PrixUnitaireHt = l.PrixUnitaireHT,
                Remise = l.Remise,
                TauxTva = l.TauxTVA
            };
            if (l.ServiceId is int sid && services.TryGetValue(sid, out var svc))
            {
                row.Reference = svc.Reference;
                row.Conditionnement = svc.Unite;
            }
            else
            {
                row.Conditionnement = string.Empty;
            }
            return row;
        }).ToList();
    }

    public async Task AssignBlsToFactureAsync(AppDbContext db, int factureId, IReadOnlyList<int> blIds, CancellationToken cancellationToken = default)
    {
        var currentBls = await db.BonsLivraison.Where(b => b.FactureId == factureId).ToListAsync(cancellationToken);
        var targetSet = blIds.ToHashSet();

        foreach (var bl in currentBls)
        {
            if (!targetSet.Contains(bl.Id))
                bl.FactureId = null;
        }

        foreach (var blId in blIds)
        {
            var bl = await db.BonsLivraison.FindAsync(new object[] { blId }, cancellationToken);
            if (bl != null)
                bl.FactureId = factureId;
        }
    }

    public async Task<List<BonLivraison>> GetLinkedBlsAsync(int factureId, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        return await db.BonsLivraison.AsNoTracking()
            .Where(b => b.FactureId == factureId)
            .OrderBy(b => b.Date).ThenBy(b => b.Numero)
            .ToListAsync(cancellationToken);
    }

    public async Task<string?> GetInvoicingStatusAsync(int blId, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        var bl = await db.BonsLivraison.AsNoTracking()
            .Include(b => b.Facture)
            .FirstOrDefaultAsync(b => b.Id == blId, cancellationToken);

        return bl?.Facture?.Numero;
    }
}
