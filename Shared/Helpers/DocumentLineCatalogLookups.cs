using GestionCommerciale.Shared.Database;
using Microsoft.EntityFrameworkCore;

namespace GestionCommerciale.Shared.Helpers;

public sealed class DocumentLineCatalogLookups
{
    private readonly Dictionary<int, string> _productReferences;
    private readonly Dictionary<int, string> _serviceReferences;

    private DocumentLineCatalogLookups(
        Dictionary<int, string> productReferences,
        Dictionary<int, string> serviceReferences)
    {
        _productReferences = productReferences;
        _serviceReferences = serviceReferences;
    }

    public string GetReference(int? produitId, int? serviceId)
    {
        if (serviceId is > 0 and int sid && _serviceReferences.TryGetValue(sid, out var serviceRef))
            return serviceRef;
        if (produitId is > 0 and int pid && _productReferences.TryGetValue(pid, out var productRef))
            return productRef;
        return string.Empty;
    }

    public static async Task<DocumentLineCatalogLookups> LoadAsync(
        AppDbContext db,
        IEnumerable<(int? ProduitId, int? ServiceId)> lines,
        CancellationToken cancellationToken = default)
    {
        var productIds = lines
            .Where(x => x.ProduitId is > 0)
            .Select(x => x.ProduitId!.Value)
            .Distinct()
            .ToList();
        var serviceIds = lines
            .Where(x => x.ServiceId is > 0)
            .Select(x => x.ServiceId!.Value)
            .Distinct()
            .ToList();

        var productReferences = productIds.Count == 0
            ? new Dictionary<int, string>()
            : await db.Produits.AsNoTracking()
                .Where(p => productIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, p => p.Reference, cancellationToken);

        var serviceReferences = serviceIds.Count == 0
            ? new Dictionary<int, string>()
            : await db.Services.AsNoTracking()
                .Where(s => serviceIds.Contains(s.Id))
                .ToDictionaryAsync(s => s.Id, s => s.Reference, cancellationToken);

        return new DocumentLineCatalogLookups(productReferences, serviceReferences);
    }
}
