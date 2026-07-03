using GestionCommerciale.Modules.Facturation.Models;

namespace GestionCommerciale.Modules.Facturation.Services;

public interface IFactureWorkflowService
{
    Task AddPaiementAsync(int factureId, Paiement paiement, CancellationToken cancellationToken = default);
    Task UpdatePaiementAsync(int factureId, int paiementId, decimal montant, DateTime date, ModePaiement mode, string reference, CancellationToken cancellationToken = default);
    Task DeletePaiementAsync(int factureId, int paiementId, CancellationToken cancellationToken = default);
}
