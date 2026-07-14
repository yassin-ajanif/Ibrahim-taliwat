using GestionCommerciale.Modules.Facturation.Models;
using GestionCommerciale.Modules.Livraison.Models;
using GestionCommerciale.Modules.Pos.Models;
using GestionCommerciale.Modules.Services;
using GestionCommerciale.Modules.Stock;
using GestionCommerciale.Modules.Stock.Models;
using GestionCommerciale.Modules.Stock.Services;
using TiersEntity = GestionCommerciale.Modules.Tiers.Models.Tiers;
using GestionCommerciale.Modules.Tiers.Models;
using GestionCommerciale.Shared.Database;
using GestionCommerciale.Shared.Helpers;
using GestionCommerciale.Shared.Services;
using Microsoft.EntityFrameworkCore;

namespace GestionCommerciale.Modules.Pos.Services;

public sealed class PosService : IPosService
{
    private readonly ICatalogSearchService _catalogSearch;
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly IStockMovementService _stock;

    public PosService(
        ICatalogSearchService catalogSearch,
        IDbContextFactory<AppDbContext> dbFactory,
        IStockMovementService stock)
    {
        _catalogSearch = catalogSearch;
        _dbFactory = dbFactory;
        _stock = stock;
    }

    public async Task<List<DocumentCatalogItem>> SearchCatalogAsync(string query, CancellationToken cancellationToken = default)
    {
        var items = await _catalogSearch.SearchCatalogAsync(query, cancellationToken: cancellationToken);
        return items.ToList();
    }

    public async Task<(List<DocumentCatalogItem> Items, int TotalCount)> BrowseCatalogAsync(
        string? query, int skip, int take, CancellationToken cancellationToken = default)
    {
        if (take < 1)
            return ([], 0);

        skip = Math.Max(0, skip);

        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

        var productsQ = db.Produits.AsNoTracking()
            .Where(p => p.Actif)
            .WhereSearchMatches(query);
        var servicesQ = db.Services.AsNoTracking()
            .Where(s => s.Actif)
            .WhereSearchMatches(query);

        var prodCount = await productsQ.CountAsync(cancellationToken);
        var svcCount = await servicesQ.CountAsync(cancellationToken);
        var total = prodCount + svcCount;

        var items = new List<DocumentCatalogItem>(take);
        var remainingSkip = skip;
        var remainingTake = take;

        if (remainingSkip < prodCount && remainingTake > 0)
        {
            var prodTake = Math.Min(remainingTake, prodCount - remainingSkip);
            var products = await productsQ
                .OrderBy(p => p.Reference)
                .Skip(remainingSkip)
                .Take(prodTake)
                .Select(p => new Produit
                {
                    Id = p.Id,
                    Reference = p.Reference,
                    CodeBarre = p.CodeBarre,
                    Designation = p.Designation,
                    Unite = p.Unite,
                    PrixAchatHT = p.PrixAchatHT,
                    PrixVenteHT = p.PrixVenteHT,
                    TauxTVA = p.TauxTVA,
                    Actif = p.Actif,
                    ImageData = p.ImageData
                })
                .ToListAsync(cancellationToken);
            items.AddRange(products.Select(DocumentCatalogItem.FromProduct));
            remainingTake -= products.Count;
            remainingSkip = 0;
        }
        else
        {
            remainingSkip -= prodCount;
        }

        if (remainingTake > 0)
        {
            var services = await servicesQ
                .OrderBy(s => s.Reference)
                .Skip(remainingSkip)
                .Take(remainingTake)
                .Select(s => new GestionCommerciale.Modules.Services.Models.Service
                {
                    Id = s.Id,
                    Reference = s.Reference,
                    Designation = s.Designation,
                    Unite = s.Unite,
                    PrixVenteHT = s.PrixVenteHT,
                    CoutHT = s.CoutHT,
                    TauxTVA = s.TauxTVA,
                    Actif = s.Actif,
                    ImageData = s.ImageData
                })
                .ToListAsync(cancellationToken);
            items.AddRange(services.Select(DocumentCatalogItem.FromService));
        }

        return (items, total);
    }

    public async Task<DocumentCatalogItem?> FindByBarcodeAsync(string barcode, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(barcode))
            return null;

        var code = barcode.Trim();
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        var produit = await db.Produits.AsNoTracking()
            .Where(p => p.Actif && p.CodeBarre != null && p.CodeBarre.ToLower() == code.ToLower())
            .SelectForListWithoutImageData()
            .FirstOrDefaultAsync(cancellationToken);

        return produit is null ? null : DocumentCatalogItem.FromProduct(produit);
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
                ProduitId = line.IsService ? null : line.ProduitId,
                ServiceId = line.IsService ? line.ServiceId : null,
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
            cart.Where(l => l.ProduitId is > 0).Select(l => (l.ProduitId!.Value, l.Quantite)),
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
                ProduitId = line.IsService ? null : line.ProduitId,
                ServiceId = line.IsService ? line.ServiceId : null,
                Designation = line.Designation,
                Quantite = line.Quantite,
                PrixUnitaireHT = line.PrixUnitaireHt,
                Remise = line.Remise,
                TauxTVA = line.TauxTva,
                Conditionnement = line.Conditionnement
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
