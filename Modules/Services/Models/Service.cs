using GestionCommerciale.Shared.Models;

namespace GestionCommerciale.Modules.Services.Models;

public class Service : BaseEntity
{
    public string Reference { get; set; } = string.Empty;
    public string Designation { get; set; } = string.Empty;
    public string Unite { get; set; } = "U";
    public decimal PrixVenteHT { get; set; }
    public decimal CoutHT { get; set; }
    public decimal TauxTVA { get; set; }
    public bool Actif { get; set; } = true;
    public string Note { get; set; } = string.Empty;
    
    /// <summary>Compressed service photo (JPEG), optional.</summary>
    public byte[]? ImageData { get; set; }
}
