using GestionCommerciale.Modules.Facturation.Services;

namespace GestionCommerciale.Modules.Reception.Services;

public interface ISupplierAccountStatementService
{
    Task<ClientAccountStatementResult> GetStatementAsync(int fournisseurId, CancellationToken cancellationToken = default);
}
