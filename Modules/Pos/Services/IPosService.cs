using GestionCommerciale.Modules.Facturation.Models;
using GestionCommerciale.Modules.Pos.Models;
using GestionCommerciale.Modules.Stock.Models;
using TiersEntity = GestionCommerciale.Modules.Tiers.Models.Tiers;

namespace GestionCommerciale.Modules.Pos.Services;

public interface IPosService
{
    Task<List<Produit>> SearchProductsAsync(string query, CancellationToken cancellationToken = default);
    Task<Facture> CheckoutAsync(int clientId, List<CartLineData> cart, IReadOnlyList<(ModePaiement Mode, decimal Montant)> payments, decimal remiseGlobale = 0, CancellationToken cancellationToken = default);
    Task<int> GetDefaultClientIdAsync(CancellationToken cancellationToken = default);
    Task<List<TiersEntity>> GetActiveClientsAsync(CancellationToken cancellationToken = default);
    Task<List<Facture>> SearchFacturesAsync(string query, CancellationToken cancellationToken = default);
}
