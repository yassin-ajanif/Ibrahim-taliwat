using GestionCommerciale.Modules.Facturation.Models;
using GestionCommerciale.Modules.Livraison.Models;
using GestionCommerciale.Modules.Pos.Models;
using GestionCommerciale.Modules.Stock.Models;
using GestionCommerciale.Modules.Stock.Services;
using TiersEntity = GestionCommerciale.Modules.Tiers.Models.Tiers;
using GestionCommerciale.Modules.Tiers.Models;
using GestionCommerciale.Shared.Database;
using GestionCommerciale.Shared.Helpers;
using Microsoft.EntityFrameworkCore;

namespace GestionCommerciale.Modules.Pos.Services;

public sealed class PosService : IPosService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly IStockMovementService _stock;

    public PosService(IDbContextFactory<AppDbContext> dbFactory, IStockMovementService stock)
    {
        _dbFactory = dbFactory;
        _stock = stock;
    }

    public async Task<List<Produit>> SearchProductsAsync(string query, CancellationToken cancellationToken = default)
    {
        var q = query?.Trim().ToLowerInvariant() ?? string.Empty;
        if (q.Length < 1)
            return [];

        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        return await db.Produits
            .Where(p => p.Actif && (p.Reference.ToLower().Contains(q) || p.Designation.ToLower().Contains(q) || p.CodeBarre!.ToLower().Contains(q)))
            .Take(20)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Facture>> SearchFacturesAsync(string query, CancellationToken cancellationToken = default)
    {
        var q = query?.Trim().ToLowerInvariant() ?? string.Empty;
        if (q.Length < 1) return [];

        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        return await db.Factures
            .Where(f => f.Numero.ToLower().Contains(q))
            .OrderByDescending(f => f.Date)
            .Take(20)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<TiersEntity>> GetActiveClientsAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        return await db.Tiers
            .Where(t => t.Actif && (t.Type == TypeTiers.Client || t.Type == TypeTiers.LesDeux))
            .OrderBy(t => t.Nom)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetDefaultClientIdAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        var client = await db.Tiers
            .Where(t => t.Nom == DbSeeder.DefaultClientName && t.Actif)
            .FirstOrDefaultAsync(cancellationToken);

        if (client is not null)
            return client.Id;

        db.Tiers.Add(new TiersEntity
        {
            Nom = DbSeeder.DefaultClientName,
            Type = TypeTiers.Client,
            Actif = true
        });
        await db.SaveChangesAsync(cancellationToken);
        return db.Tiers.Local.First(t => t.Nom == DbSeeder.DefaultClientName).Id;
    }

    public async Task<Facture> CheckoutAsync(int clientId, List<CartLineData> cart, IReadOnlyList<(ModePaiement Mode, decimal Montant)> payments, decimal remiseGlobale = 0, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        await using var trx = await db.Database.BeginTransactionAsync(cancellationToken);

        var numero = "POS-" + DateTime.Now.ToString("yyyyMMdd-HHmmss");

        var bl = new BonLivraison
        {
            Numero = numero,
            ClientId = clientId,
            Date = DateTime.Today,
            Note = "Vente POS"
        };
        db.BonsLivraison.Add(bl);
        await db.SaveChangesAsync(cancellationToken);

        foreach (var line in cart)
        {
            db.BonLivraisonLignes.Add(new BonLivraisonLigne
            {
                BLId = bl.Id,
                ProduitId = line.ProduitId,
                Designation = line.Designation,
                QuantiteCommandee = line.Quantite,
                QuantiteLivree = line.Quantite,
                PrixUnitaireHT = line.PrixUnitaireHt,
                TauxTVA = line.TauxTva
            });
        }
        await db.SaveChangesAsync(cancellationToken);

        await _stock.ResyncBonLivraisonStockAsync(
            db, bl.Id, bl.Numero,
            cart.Select(l => (l.ProduitId, l.Quantite)),
            null, cancellationToken);

        var facture = new Facture
        {
            Numero = numero,
            ClientId = clientId,
            Date = DateTime.Today,
            DateEcheance = DateTime.Today.AddDays(30),
            EstPayee = payments.All(p => p.Mode == ModePaiement.Especes),
            RemiseGlobale = remiseGlobale,
            Note = "Vente POS"
        };
        db.Factures.Add(facture);
        await db.SaveChangesAsync(cancellationToken);

        bl.FactureId = facture.Id;
        await db.SaveChangesAsync(cancellationToken);

        foreach (var line in cart)
        {
            db.FactureLignes.Add(new FactureLigne
            {
                FactureId = facture.Id,
                ProduitId = line.ProduitId,
                Designation = line.Designation,
                Quantite = line.Quantite,
                PrixUnitaireHT = line.PrixUnitaireHt,
                Remise = line.Remise,
                TauxTVA = line.TauxTva,
                Conditionnement = string.Empty
            });
        }
        facture.TotalTtc = DocumentTotalsHelper.FactureTtc(
            cart.Select(line => new FactureLigne
            {
                Quantite = line.Quantite,
                PrixUnitaireHT = line.PrixUnitaireHt,
                Remise = line.Remise,
                TauxTVA = line.TauxTva
            }),
            remiseGlobale);
        await db.SaveChangesAsync(cancellationToken);

        foreach (var (mode, montant) in payments.Where(p => p.Montant > 0))
        {
            db.Paiements.Add(new Paiement
            {
                FactureId = facture.Id,
                Montant = montant,
                Date = DateTime.Today,
                Mode = mode,
                Reference = string.Empty
            });
        }
        await db.SaveChangesAsync(cancellationToken);

        await trx.CommitAsync(cancellationToken);

        return facture;
    }
}
