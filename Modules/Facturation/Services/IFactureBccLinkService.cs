using GestionCommerciale.Modules.CommandeClient.Models;
using GestionCommerciale.Shared.Database;

namespace GestionCommerciale.Modules.Facturation.Services;

public interface IFactureBccLinkService
{
    Task<List<BonCommandeClient>> GetAvailableBccsForClientAsync(int clientId, int? excludeFactureId = null, CancellationToken cancellationToken = default);
    Task<List<string>> ValidateBccsForFactureAsync(int clientId, IReadOnlyList<int> bccIds, CancellationToken cancellationToken = default);
    Task AssignBccsToFactureAsync(AppDbContext db, int factureId, IReadOnlyList<int> bccIds, CancellationToken cancellationToken = default);
    Task<List<BonCommandeClient>> GetLinkedBccsAsync(int factureId, CancellationToken cancellationToken = default);
}
