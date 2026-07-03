using GestionCommerciale.Modules.Stock.Models;

namespace GestionCommerciale.Modules.Pos.ViewModels;

public class ProductSearchRow
{
    public Produit Product { get; }
    public int Id => Product.Id;
    public string Reference => Product.Reference;
    public string? CodeBarre => Product.CodeBarre;
    public string Designation => Product.Designation;
    public decimal PrixVenteTtc => Product.PrixVenteHT * (1 + Product.TauxTVA / 100m);

    public ProductSearchRow(Produit product) => Product = product;
}
