using GestionCommerciale.Modules.Services;
using GestionCommerciale.Modules.Services.Models;
using GestionCommerciale.Modules.Stock;
using GestionCommerciale.Modules.Stock.Models;
using GestionCommerciale.Shared.Database;
using GestionCommerciale.Shared.Helpers;
using Microsoft.EntityFrameworkCore;

namespace GestionCommerciale.Shared.Services;

public sealed class CatalogSearchService : ICatalogSearchService
{
    public const int DefaultLimit = 20;

    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public CatalogSearchService(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<IReadOnlyList<Produit>> SearchProductsAsync(
        string query, int limit = DefaultLimit, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query) || limit < 1)
            return [];

        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        return await db.Produits.AsNoTracking()
            .Where(p => p.Actif)
            .WhereSearchMatches(query)
            .SelectForListWithoutImageData()
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Service>> SearchServicesAsync(
        string query, int limit = DefaultLimit, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query) || limit < 1)
            return [];

        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        return await db.Services.AsNoTracking()
            .Where(s => s.Actif)
            .WhereSearchMatches(query)
            .OrderBy(s => s.Reference)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DocumentCatalogItem>> SearchCatalogAsync(
        string query, int limit = DefaultLimit, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query) || limit < 1)
            return [];

        var productsTask = SearchProductsAsync(query, limit, cancellationToken);
        var servicesTask = SearchServicesAsync(query, limit, cancellationToken);
        await Task.WhenAll(productsTask, servicesTask);
        return DocumentCatalogItem.MergeSearchResults(productsTask.Result, servicesTask.Result, limit);
    }
}
