using GestionCommerciale.Modules.Services.Models;
using GestionCommerciale.Modules.Stock.Models;
using GestionCommerciale.Shared.Helpers;

namespace GestionCommerciale.Shared.Services;

public interface ICatalogSearchService
{
    Task<IReadOnlyList<Produit>> SearchProductsAsync(
        string query, int limit = 20, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Service>> SearchServicesAsync(
        string query, int limit = 20, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DocumentCatalogItem>> SearchCatalogAsync(
        string query, int limit = 20, CancellationToken cancellationToken = default);
}
