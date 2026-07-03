using GestionCommerciale.Shared.Models;

namespace GestionCommerciale.Modules.Devis.Models;

public class Devis : BaseEntity
{
    public string Numero { get; set; } = string.Empty;
    public int ClientId { get; set; }
    public DateTime Date { get; set; }
    public DateTime DateValidite { get; set; }
    public decimal RemiseGlobale { get; set; }
    public string Note { get; set; } = string.Empty;
    public List<DevisLigne> Lignes { get; set; } = [];
    public List<DevisCondition> Conditions { get; set; } = [];
}
