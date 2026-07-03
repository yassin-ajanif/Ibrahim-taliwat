using GestionCommerciale.Shared.Models;

namespace GestionCommerciale.Modules.Devis.Models;

public class DevisLigne : BaseEntity
{
    public int DevisId { get; set; }
    public Devis? Devis { get; set; }
    public int ProduitId { get; set; }
    public string Designation { get; set; } = string.Empty;
    public decimal Quantite { get; set; }
    public decimal PrixUnitaireHT { get; set; }
    public decimal Remise { get; set; }
    public decimal TauxTVA { get; set; }
    /// <summary>Unit / packaging label (e.g. carton, pièce).</summary>
    public string Conditionnement { get; set; } = string.Empty;
}
