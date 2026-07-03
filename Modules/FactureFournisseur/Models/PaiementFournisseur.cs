using GestionCommerciale.Modules.Facturation.Models;
using GestionCommerciale.Shared.Models;

namespace GestionCommerciale.Modules.FactureFournisseur.Models;

public class PaiementFournisseur : BaseEntity
{
    public int FactureFournisseurId { get; set; }
    public FactureFournisseur? FactureFournisseur { get; set; }
    public decimal Montant { get; set; }
    public DateTime Date { get; set; }
    public ModePaiement Mode { get; set; }
    public string Reference { get; set; } = string.Empty;
}
