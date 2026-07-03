using GestionCommerciale.Shared.Models;

namespace GestionCommerciale.Modules.AvoirFournisseur.Models;

public class AvoirFournisseur : BaseEntity
{
    public string Numero { get; set; } = string.Empty;
    public int FournisseurId { get; set; }
    public DateTime Date { get; set; }
    public string Motif { get; set; } = string.Empty;
    public bool RetourMarchandise { get; set; }
    public List<AvoirFournisseurLigne> Lignes { get; set; } = [];
}
