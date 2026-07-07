using GestionCommerciale.Shared.Helpers;

namespace GestionCommerciale.Modules.Pos.ViewModels;

public sealed class CatalogSearchRow
{
    public DocumentCatalogItem Item { get; }

    public CatalogSearchRow(DocumentCatalogItem item) => Item = item;

    public string Reference => Item.Reference;
    public string Designation => Item.Designation;
    public string? CodeBarre => Item.CodeBarre;
    public bool IsService => Item.Kind == DocumentCatalogKind.Service;
    public decimal PrixVenteTtc => Item.PrixVenteHT * (1 + Item.TauxTVA / 100m);
}
