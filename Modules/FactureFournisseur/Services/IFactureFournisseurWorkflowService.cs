using GestionCommerciale.Modules.FactureFournisseur.Models;
using GestionCommerciale.Shared.Database;
using Microsoft.EntityFrameworkCore;

namespace GestionCommerciale.Modules.FactureFournisseur.Services;

public interface IFactureFournisseurWorkflowService
{
    Task AddPaiementAsync(int factureFournisseurId, PaiementFournisseur paiement, CancellationToken cancellationToken = default);
    Task UpdatePaiementAsync(int factureFournisseurId, int paiementId, decimal montant, DateTime date, GestionCommerciale.Modules.Facturation.Models.ModePaiement mode, string reference, CancellationToken cancellationToken = default);
    Task DeletePaiementAsync(int factureFournisseurId, int paiementId, CancellationToken cancellationToken = default);
}
