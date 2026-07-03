using System.Globalization;
using GestionCommerciale.Modules.Facturation.Models;
using GestionCommerciale.Shared.Helpers;
using GestionCommerciale.Shared.Services;

namespace GestionCommerciale.Modules.Facturation.ViewModels;

public sealed class AvoirListRow
{
    public required Avoir Avoir { get; init; }
    public string ClientNom { get; init; } = string.Empty;
    public string FactureNumero { get; init; } = string.Empty;
    public string DateShort { get; init; } = string.Empty;
    public string MotifDisplay { get; init; } = string.Empty;
    public string HtLabel { get; init; } = string.Empty;
    public string TtcLabel { get; init; } = string.Empty;

    public static AvoirListRow Create(Avoir avoir, string clientNom, string factureNumero, string devise, ILocaleService locale)
    {
        var lines = avoir.Lignes ?? [];
        var (ht, _, ttc) = DocumentTotalsHelper.AvoirTotals(lines);
        var motif = avoir.Motif ?? string.Empty;
        const int maxMotif = 72;
        var motifDisplay = motif.Length <= maxMotif ? motif : motif[..maxMotif] + "…";
        return new AvoirListRow
        {
            Avoir = avoir,
            ClientNom = clientNom,
            FactureNumero = factureNumero,
            DateShort = avoir.Date.ToString("d", CultureInfo.CurrentCulture),
            MotifDisplay = string.IsNullOrEmpty(motifDisplay) ? factureNumero : motifDisplay,
            HtLabel = locale.Tf("Doc_FmtHt", ht, devise),
            TtcLabel = $"{ttc:N2} {devise}",
        };
    }
}
