using GestionCommerciale.Shared.Models;

namespace GestionCommerciale.Modules.CommandeClient.Models;

public class BonCommandeClientLigne : BaseEntity
{
    public int BonCommandeClientId { get; set; }
    public BonCommandeClient? BonCommandeClient { get; set; }
    public int ProduitId { get; set; }
    public string Designation { get; set; } = string.Empty;
    public decimal QuantiteCommandee { get; set; }
    public decimal PrixUnitaireHT { get; set; }
    public decimal Remise { get; set; }
    public decimal TauxTVA { get; set; }
    public string Conditionnement { get; set; } = string.Empty;
}
