using GestionCommerciale.Shared.Models;

namespace GestionCommerciale.Modules.CommandeFournisseur.Models;

public class BonCommandeLigne : BaseEntity
{
    public int BonCommandeId { get; set; }
    public BonCommande? BonCommande { get; set; }
    public int ProduitId { get; set; }
    public string Designation { get; set; } = string.Empty;
    public decimal QuantiteCommandee { get; set; }
    public decimal PrixUnitaireHT { get; set; }
    public decimal Remise { get; set; }
    public decimal TauxTVA { get; set; }
    /// <summary>Unit / packaging label (e.g. carton, pièce).</summary>
    public string Conditionnement { get; set; } = string.Empty;
}
