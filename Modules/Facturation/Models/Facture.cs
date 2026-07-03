using GestionCommerciale.Shared.Models;

namespace GestionCommerciale.Modules.Facturation.Models;

public class Facture : BaseEntity
{
    public string Numero { get; set; } = string.Empty;
    public int ClientId { get; set; }
    public int? DevisId { get; set; }
    public DateTime Date { get; set; }
    public DateTime DateEcheance { get; set; }
    public bool EstPayee { get; set; }
    public decimal RemiseGlobale { get; set; }
    public decimal TotalTtc { get; set; }
    public string Note { get; set; } = string.Empty;
    /// <summary>Editable bon de commande reference shown on the invoice (free text).</summary>
    public string BonCommandeReference { get; set; } = string.Empty;
    public List<FactureLigne> Lignes { get; set; } = [];
    public List<Paiement> Paiements { get; set; } = [];
}
