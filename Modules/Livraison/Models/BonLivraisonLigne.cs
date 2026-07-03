using GestionCommerciale.Shared.Models;

namespace GestionCommerciale.Modules.Livraison.Models;

public class BonLivraisonLigne : BaseEntity
{
    public int BLId { get; set; }
    public BonLivraison? BonLivraison { get; set; }
    public int ProduitId { get; set; }
    public string Designation { get; set; } = string.Empty;
    public decimal QuantiteCommandee { get; set; }
    public decimal QuantiteLivree { get; set; }
    public decimal PrixUnitaireHT { get; set; }
    public decimal Remise { get; set; }
    public decimal TauxTVA { get; set; }
}
