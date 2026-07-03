using GestionCommerciale.Shared.Models;

namespace GestionCommerciale.Modules.Tiers.Models;

public class Tiers : BaseEntity
{
    public TypeTiers Type { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string ICE { get; set; } = string.Empty;
    public string Adresse { get; set; } = string.Empty;
    public string Ville { get; set; } = string.Empty;
    public string Telephone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string ConditionsPaiement { get; set; } = string.Empty;
    public bool Actif { get; set; } = true;
}
