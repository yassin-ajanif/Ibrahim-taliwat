using GestionCommerciale.Shared.Models;

namespace GestionCommerciale.Modules.Stock.Models;

public class Categorie : BaseEntity
{
    public string Nom { get; set; } = string.Empty;
}
