using GestionCommerciale.Shared.Models;

namespace GestionCommerciale.Modules.Stock.Models;

public class Produit : BaseEntity
{
    public string Reference { get; set; } = string.Empty;
    /// <summary>EAN / UPC / code interne, optionnel.</summary>
    public string? CodeBarre { get; set; }
    public string Designation { get; set; } = string.Empty;
    public string Unite { get; set; } = "U";
    public decimal PrixAchatHT { get; set; }
    public decimal PrixVenteHT { get; set; }
    public decimal TauxTVA { get; set; }
    public decimal StockActuel { get; set; }
    public decimal StockMinimum { get; set; }
    public int? CategorieId { get; set; }
    public Categorie? Categorie { get; set; }
    public bool Actif { get; set; } = true;

    /// <summary>Compressed product photo (JPEG), optional.</summary>
    public byte[]? ImageData { get; set; }
}
