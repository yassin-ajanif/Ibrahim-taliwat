using GestionCommerciale.Modules.Reception.Models;

namespace GestionCommerciale.Modules.Reception.Services;

public interface IBonReceptionWorkflowService
{
    Task ValiderAsync(int bonReceptionId, int? userId, CancellationToken cancellationToken = default);

    /// <summary>Rejoue les entrées stock et le PUMP à partir des lignes BR lorsque le document est déjà validé (idempotent).</summary>
    Task ResyncStockFromLinesAsync(int bonReceptionId, int? userId, CancellationToken cancellationToken = default);
}
