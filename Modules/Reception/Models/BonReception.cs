using GestionCommerciale.Modules.CommandeFournisseur.Models;
using GestionCommerciale.Shared.Models;

namespace GestionCommerciale.Modules.Reception.Models;

public class BonReception : BaseEntity
{
    public string Numero { get; set; } = string.Empty;
    /// <summary>Optional link to the purchase order this reception fulfills (or partially fulfills).</summary>
    public int? BonCommandeId { get; set; }
    public BonCommande? BonCommande { get; set; }
    public int FournisseurId { get; set; }
    public DateTime Date { get; set; }
    public int? FactureFournisseurId { get; set; }
    public decimal TotalTtc { get; set; }
    public string Note { get; set; } = string.Empty;
    public List<BonReceptionLigne> Lignes { get; set; } = [];
}
