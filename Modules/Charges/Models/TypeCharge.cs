using GestionCommerciale.Shared.Models;

namespace GestionCommerciale.Modules.Charges.Models;

public class TypeCharge : BaseEntity
{
    public string Nom { get; set; } = string.Empty;
    public bool Actif { get; set; } = true;
}
