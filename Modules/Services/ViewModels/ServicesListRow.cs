using GestionCommerciale.Modules.Services.Models;

namespace GestionCommerciale.Modules.Services.ViewModels;

public sealed class ServicesListRow
{
    private ServicesListRow(Service service, string prixLabel, string tvaLabel)
    {
        Service = service;
        PrixLabel = prixLabel;
        TvaLabel = tvaLabel;
        ActifLabel = service.Actif ? "✓" : "—";
    }

    public Service Service { get; }
    public string PrixLabel { get; }
    public string TvaLabel { get; }
    public string ActifLabel { get; }

    public static ServicesListRow Create(Service service, string devise) =>
        new(service, $"{service.PrixVenteHT:N2} {devise}", $"{service.TauxTVA:N0}%");
}
