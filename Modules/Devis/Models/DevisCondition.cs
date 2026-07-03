using GestionCommerciale.Shared.Models;

namespace GestionCommerciale.Modules.Devis.Models;

public class DevisCondition : BaseEntity
{
    public int DevisId { get; set; }
    public Devis? Devis { get; set; }
    public string Titre { get; set; } = string.Empty;
    public string Valeur { get; set; } = string.Empty;
    public int Ordre { get; set; }
}
