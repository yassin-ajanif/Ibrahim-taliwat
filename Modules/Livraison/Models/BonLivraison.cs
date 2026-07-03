using GestionCommerciale.Modules.Facturation.Models;
using GestionCommerciale.Shared.Models;

namespace GestionCommerciale.Modules.Livraison.Models;

public class BonLivraison : BaseEntity
{
    public string Numero { get; set; } = string.Empty;
    public int ClientId { get; set; }
    public int? DevisId { get; set; }
    public int? BonCommandeClientId { get; set; }
    public int? FactureId { get; set; }
    public Facture? Facture { get; set; }
    public DateTime Date { get; set; }
    public string Note { get; set; } = string.Empty;
    public List<BonLivraisonLigne> Lignes { get; set; } = [];
}
