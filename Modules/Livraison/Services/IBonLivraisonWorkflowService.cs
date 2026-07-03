namespace GestionCommerciale.Modules.Livraison.Services;

public interface IBonLivraisonWorkflowService
{
    Task ValiderAsync(int bonLivraisonId, int? userId, CancellationToken cancellationToken = default);

    /// <summary>Re-applies stock movements from current BL lines (idempotent).</summary>
    Task ResyncStockFromLinesAsync(int bonLivraisonId, int? userId, CancellationToken cancellationToken = default);
}
