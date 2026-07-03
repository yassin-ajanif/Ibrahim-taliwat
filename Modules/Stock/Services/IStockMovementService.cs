using GestionCommerciale.Modules.Stock.Models;
using GestionCommerciale.Shared.Database;

namespace GestionCommerciale.Modules.Stock.Services;

public interface IStockMovementService
{
    Task ApplyMovementAsync(
        AppDbContext db,
        int produitId,
        TypeMouvement type,
        decimal quantite,
        string origineType,
        int? origineId,
        string? note,
        int? createdByUserId,
        CancellationToken cancellationToken = default);

    Task ResyncBonLivraisonStockAsync(
        AppDbContext db,
        int bonLivraisonId,
        string noteDetail,
        IEnumerable<(int ProduitId, decimal QuantiteLivree)> lines,
        int? createdByUserId,
        CancellationToken cancellationToken = default);

    Task SyncBonReceptionStockAsync(
        AppDbContext db,
        int bonReceptionId,
        string noteDetail,
        IEnumerable<(int ProduitId, decimal QuantiteRecue, decimal PrixUnitaireHT)> lines,
        int? createdByUserId,
        CancellationToken cancellationToken = default);

    Task SyncAvoirStockAsync(
        AppDbContext db,
        int avoirId,
        string noteDetail,
        bool retourMarchandise,
        IEnumerable<(int ProduitId, decimal Quantite)> lines,
        int? createdByUserId,
        CancellationToken cancellationToken = default);

    Task SyncAvoirFournisseurStockAsync(
        AppDbContext db,
        int avoirFournisseurId,
        string noteDetail,
        bool retourMarchandise,
        IEnumerable<(int ProduitId, decimal Quantite)> lines,
        int? createdByUserId,
        CancellationToken cancellationToken = default);
}
