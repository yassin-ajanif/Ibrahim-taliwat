using GestionCommerciale.Modules.Reception.Models;
using GestionCommerciale.Shared.Models;

namespace GestionCommerciale.Modules.FactureFournisseur.Models;

public class FactureFournisseurLigne : BaseEntity
{
    public int FactureFournisseurId { get; set; }
    public FactureFournisseur? FactureFournisseur { get; set; }
    public int? BonReceptionId { get; set; }
    public BonReception? BonReception { get; set; }
    public int ProduitId { get; set; }
    public string Designation { get; set; } = string.Empty;
    public decimal Quantite { get; set; }
    public decimal PrixUnitaireHT { get; set; }
    public decimal Remise { get; set; }
    public decimal TauxTVA { get; set; }
    public string Conditionnement { get; set; } = string.Empty;
}
