using GestionCommerciale.Modules.Stock.Models;
using GestionCommerciale.Shared.Database;
using GestionCommerciale.Shared.Services;
using Microsoft.EntityFrameworkCore;

namespace GestionCommerciale.Modules.Stock.Services;

public sealed class StockMovementService : IStockMovementService
{
    public const string OrigineTypeBonLivraison = "BL";
    public const string OrigineTypeBonReception = "BR";
    public const string OrigineTypeAvoir = "Avoir";
    public const string OrigineTypeAvoirFournisseur = "AvoirFournisseur";
    public const string OrigineTypeImport = "Import";

    private readonly ILocaleService _locale;

    public StockMovementService(ILocaleService locale)
    {
        _locale = locale;
    }

    public async Task ApplyMovementAsync(
        AppDbContext db,
        int produitId,
        TypeMouvement type,
        decimal quantite,
        string origineType,
        int? origineId,
        string? note,
        int? createdByUserId,
        CancellationToken cancellationToken = default)
    {
        var produit = await db.Produits.FirstAsync(p => p.Id == produitId, cancellationToken);
        decimal delta = type switch
        {
            TypeMouvement.Entree => quantite,
            TypeMouvement.Sortie => -quantite,
            TypeMouvement.Ajustement => quantite,
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };

        var stockAvant = produit.StockActuel;
        produit.StockActuel = stockAvant + delta;

        db.MouvementsStock.Add(new MouvementStock
        {
            ProduitId = produitId,
            Type = type,
            StockAvant = stockAvant,
            Quantite = quantite,
            OrigineType = origineType,
            OrigineId = origineId,
            Note = note ?? string.Empty,
            CreatedByUserId = createdByUserId
        });
    }

    public Task ResyncBonLivraisonStockAsync(
        AppDbContext db,
        int bonLivraisonId,
        string noteDetail,
        IEnumerable<(int ProduitId, decimal QuantiteLivree)> lines,
        int? createdByUserId,
        CancellationToken cancellationToken = default)
    {
        var desired = lines
            .Where(l => l.ProduitId > 0 && l.QuantiteLivree > 0)
            .GroupBy(l => l.ProduitId)
            .ToDictionary(g => g.Key, g => -g.Sum(l => l.QuantiteLivree));

        return SyncDocumentStockAsync(
            db,
            OrigineTypeBonLivraison,
            bonLivraisonId,
            noteDetail,
            desired,
            createdByUserId,
            useModificationNoteOnEdit: true,
            onPositiveEntreeDelta: null,
            cancellationToken);
    }

    public Task SyncBonReceptionStockAsync(
        AppDbContext db,
        int bonReceptionId,
        string noteDetail,
        IEnumerable<(int ProduitId, decimal QuantiteRecue, decimal PrixUnitaireHT)> lines,
        int? createdByUserId,
        CancellationToken cancellationToken = default)
    {
        var lineList = lines.Where(l => l.ProduitId > 0 && l.QuantiteRecue > 0).ToList();
        var desired = lineList
            .GroupBy(l => l.ProduitId)
            .ToDictionary(g => g.Key, g => g.Sum(l => l.QuantiteRecue));

        var prixByProduit = lineList
            .GroupBy(l => l.ProduitId)
            .ToDictionary(
                g => g.Key,
                g =>
                {
                    var totalQty = g.Sum(l => l.QuantiteRecue);
                    var weighted = g.Sum(l => l.QuantiteRecue * l.PrixUnitaireHT);
                    return totalQty > 0 ? weighted / totalQty : 0m;
                });

        return SyncDocumentStockAsync(
            db,
            OrigineTypeBonReception,
            bonReceptionId,
            noteDetail,
            desired,
            createdByUserId,
            useModificationNoteOnEdit: true,
            onPositiveEntreeDelta: async (produitId, entreeDelta, ct) =>
            {
                if (!prixByProduit.TryGetValue(produitId, out var newPrice)) return;
                var produit = await db.Produits.FirstAsync(p => p.Id == produitId, ct);
                var oldQty = produit.StockActuel - entreeDelta;
                var oldPrice = produit.PrixAchatHT;
                var totalQty = oldQty + entreeDelta;
                if (totalQty > 0)
                    produit.PrixAchatHT = (oldQty * oldPrice + entreeDelta * newPrice) / totalQty;
            },
            cancellationToken);
    }

    public Task SyncAvoirStockAsync(
        AppDbContext db,
        int avoirId,
        string noteDetail,
        bool retourMarchandise,
        IEnumerable<(int ProduitId, decimal Quantite)> lines,
        int? createdByUserId,
        CancellationToken cancellationToken = default)
    {
        var desired = retourMarchandise
            ? lines
                .Where(l => l.ProduitId > 0 && l.Quantite > 0)
                .GroupBy(l => l.ProduitId)
                .ToDictionary(g => g.Key, g => g.Sum(l => l.Quantite))
            : [];

        return SyncDocumentStockAsync(
            db,
            OrigineTypeAvoir,
            avoirId,
            noteDetail,
            desired,
            createdByUserId,
            useModificationNoteOnEdit: true,
            onPositiveEntreeDelta: null,
            cancellationToken);
    }

    public Task SyncAvoirFournisseurStockAsync(
        AppDbContext db,
        int avoirFournisseurId,
        string noteDetail,
        bool retourMarchandise,
        IEnumerable<(int ProduitId, decimal Quantite)> lines,
        int? createdByUserId,
        CancellationToken cancellationToken = default)
    {
        var desired = retourMarchandise
            ? lines
                .Where(l => l.ProduitId > 0 && l.Quantite > 0)
                .GroupBy(l => l.ProduitId)
                .ToDictionary(g => g.Key, g => -g.Sum(l => l.Quantite))
            : [];

        return SyncDocumentStockAsync(
            db,
            OrigineTypeAvoirFournisseur,
            avoirFournisseurId,
            noteDetail,
            desired,
            createdByUserId,
            useModificationNoteOnEdit: true,
            onPositiveEntreeDelta: null,
            cancellationToken);
    }

    private async Task SyncDocumentStockAsync(
        AppDbContext db,
        string origineType,
        int origineId,
        string noteDetail,
        IReadOnlyDictionary<int, decimal> desiredSignedByProduit,
        int? createdByUserId,
        bool useModificationNoteOnEdit,
        Func<int, decimal, CancellationToken, Task>? onPositiveEntreeDelta,
        CancellationToken cancellationToken)
    {
        var movements = await db.MouvementsStock
            .Where(m => m.OrigineType == origineType && m.OrigineId == origineId)
            .ToListAsync(cancellationToken);

        var documentHasPriorMovements = movements.Count > 0;

        var currentSignedByProduit = movements
            .GroupBy(m => m.ProduitId)
            .ToDictionary(g => g.Key, g => g.Sum(SignedQuantite));

        var produitIds = currentSignedByProduit.Keys
            .Union(desiredSignedByProduit.Keys)
            .ToList();

        foreach (var produitId in produitIds)
        {
            currentSignedByProduit.TryGetValue(produitId, out var current);
            desiredSignedByProduit.TryGetValue(produitId, out var desired);
            var delta = desired - current;
            if (delta == 0) continue;

            var isAnnulation = desired == 0 && current != 0;
            var isModification = useModificationNoteOnEdit && !isAnnulation && documentHasPriorMovements;
            var note = isAnnulation
                ? _locale.Tf("Stock_AnnulationNote", noteDetail)
                : isModification
                    ? _locale.Tf("Stock_ModificationNote", noteDetail)
                    : noteDetail;

            if (delta > 0)
            {
                await ApplyMovementAsync(
                    db,
                    produitId,
                    TypeMouvement.Entree,
                    delta,
                    origineType,
                    origineId,
                    note,
                    createdByUserId,
                    cancellationToken);

                if (onPositiveEntreeDelta != null)
                    await onPositiveEntreeDelta(produitId, delta, cancellationToken);
            }
            else
            {
                await ApplyMovementAsync(
                    db,
                    produitId,
                    TypeMouvement.Sortie,
                    -delta,
                    origineType,
                    origineId,
                    note,
                    createdByUserId,
                    cancellationToken);
            }
        }
    }

    private static decimal SignedQuantite(MouvementStock m) => m.Type switch
    {
        TypeMouvement.Sortie => -Math.Abs(m.Quantite),
        TypeMouvement.Entree => Math.Abs(m.Quantite),
        TypeMouvement.Ajustement => m.Quantite,
        _ => m.Quantite
    };
}
