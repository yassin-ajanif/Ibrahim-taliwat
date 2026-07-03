using GestionCommerciale.Shared.Models;

namespace GestionCommerciale.Modules.AvoirFournisseur.Models;

public class AvoirFournisseurLigne : BaseEntity
{
    public int AvoirFournisseurId { get; set; }
    public AvoirFournisseur? AvoirFournisseur { get; set; }
    public int ProduitId { get; set; }
    public string Designation { get; set; } = string.Empty;
    public decimal Quantite { get; set; }
    public decimal PrixUnitaireHT { get; set; }
    public decimal Remise { get; set; }
    public decimal TauxTVA { get; set; }
    public string Conditionnement { get; set; } = string.Empty;
}
