using GestionCommerciale.Modules.Services.Models;
using GestionCommerciale.Modules.Stock.Models;

namespace GestionCommerciale.Shared.Helpers;

public enum DocumentCatalogKind
{
    Product,
    Service
}

public sealed class DocumentCatalogItem
{
    public DocumentCatalogKind Kind { get; init; }
    public int Id { get; init; }
    public string Reference { get; init; } = string.Empty;
    public string Designation { get; init; } = string.Empty;
    public string? CodeBarre { get; init; }
    public string Unite { get; init; } = "U";
    public decimal PrixVenteHT { get; init; }
    public decimal PrixAchatHT { get; init; }
    public decimal TauxTVA { get; init; }

    public string DisplayLabel => $"{Reference} — {Designation}";

    public static DocumentCatalogItem FromProduct(Produit p) => new()
    {
        Kind = DocumentCatalogKind.Product,
        Id = p.Id,
        Reference = p.Reference,
        Designation = p.Designation,
        CodeBarre = p.CodeBarre,
        Unite = p.Unite,
        PrixVenteHT = p.PrixVenteHT,
        PrixAchatHT = p.PrixAchatHT,
        TauxTVA = p.TauxTVA
    };

    public static DocumentCatalogItem FromService(Service s) => new()
    {
        Kind = DocumentCatalogKind.Service,
        Id = s.Id,
        Reference = s.Reference,
        Designation = s.Designation,
        Unite = s.Unite,
        PrixVenteHT = s.PrixVenteHT,
        PrixAchatHT = s.CoutHT,
        TauxTVA = s.TauxTVA
    };

    public static IReadOnlyList<DocumentCatalogItem> MergeSearchResults(
        IReadOnlyList<Produit> products,
        IReadOnlyList<Service> services,
        int limit = 20)
        => products.Select(FromProduct)
            .Concat(services.Select(FromService))
            .Take(limit)
            .ToList();
}
