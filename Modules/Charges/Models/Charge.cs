using GestionCommerciale.Shared.Models;
using TiersEntity = GestionCommerciale.Modules.Tiers.Models.Tiers;

namespace GestionCommerciale.Modules.Charges.Models;

public class Charge : BaseEntity
{
    public int TypeChargeId { get; set; }
    public TypeCharge? TypeCharge { get; set; }

    public DateTime Date { get; set; }
    public string Libelle { get; set; } = string.Empty;

    public int? FournisseurId { get; set; }
    public TiersEntity? Fournisseur { get; set; }
    public string BeneficiaireLibre { get; set; } = string.Empty;

    public decimal MontantTtc { get; set; }
    public string Note { get; set; } = string.Empty;
}
