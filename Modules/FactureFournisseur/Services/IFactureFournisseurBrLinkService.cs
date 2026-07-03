using GestionCommerciale.Modules.FactureFournisseur.ViewModels;
using GestionCommerciale.Modules.Reception.Models;

namespace GestionCommerciale.Modules.FactureFournisseur.Services;

public interface IFactureFournisseurBrLinkService
{
    Task<List<BonReception>> GetAvailableBrsForFournisseurAsync(int fournisseurId, int? excludeFactureFournisseurId = null, CancellationToken cancellationToken = default);
    Task<List<string>> ValidateBrsForFactureFournisseurAsync(int fournisseurId, IReadOnlyList<int> brIds, CancellationToken cancellationToken = default);
    Task<List<FactureFournisseurLineRow>> LoadBrLinesAsync(int brId, CancellationToken cancellationToken = default);
    Task AssignBrsToFactureFournisseurAsync(Shared.Database.AppDbContext db, int factureFournisseurId, IReadOnlyList<int> brIds, CancellationToken cancellationToken = default);
    Task<List<BonReception>> GetLinkedBrsAsync(int factureFournisseurId, CancellationToken cancellationToken = default);
    Task<string?> GetInvoicingStatusAsync(int brId, CancellationToken cancellationToken = default);
}
