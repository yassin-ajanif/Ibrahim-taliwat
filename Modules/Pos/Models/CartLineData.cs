namespace GestionCommerciale.Modules.Pos.Models;

public class CartLineData
{
    public int? ProduitId { get; set; }
    public int? ServiceId { get; set; }
    public string Designation { get; set; } = string.Empty;
    public string Conditionnement { get; set; } = string.Empty;
    public decimal Quantite { get; set; }
    public decimal PrixUnitaireHt { get; set; }
    public decimal TauxTva { get; set; }
    public decimal Remise { get; set; }

    public bool IsService => ServiceId is > 0;
}
