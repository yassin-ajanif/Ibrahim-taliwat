using GestionCommerciale.Shared.Models;

namespace GestionCommerciale.Modules.Reception.Models;

public class BonReceptionLigne : BaseEntity
{
    public int BRId { get; set; }
    public BonReception? BonReception { get; set; }
    public int ProduitId { get; set; }
    public string Designation { get; set; } = string.Empty;
    public decimal QuantiteRecue { get; set; }
    public decimal PrixUnitaireHT { get; set; }
    public decimal TauxTVA { get; set; }
}
