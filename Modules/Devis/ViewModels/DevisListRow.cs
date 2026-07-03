using System.Globalization;
using GestionCommerciale.Shared.Helpers;
using GestionCommerciale.Shared.Services;
using DevisEntity = GestionCommerciale.Modules.Devis.Models.Devis;

namespace GestionCommerciale.Modules.Devis.ViewModels;

public sealed class DevisListRow
{
    public required DevisEntity Devis { get; init; }
    public string ClientNom { get; init; } = string.Empty;
    public string DateShort { get; init; } = string.Empty;
    public string ValidUntilShort { get; init; } = string.Empty;
    public string TtcLabel { get; init; } = string.Empty;
    public bool IsExpired { get; init; }
    public string NotePreview { get; init; } = string.Empty;

    public static DevisListRow Create(DevisEntity devis, string clientNom, string devise, ILocaleService locale)
    {
        var lines = devis.Lignes ?? [];
        var (_, _, ttc) = DocumentTotalsHelper.DevisTotals(lines, devis.RemiseGlobale);
        var today = DateTime.Today;
        return new DevisListRow
        {
            Devis = devis,
            ClientNom = clientNom,
            DateShort = devis.Date.ToString("d", CultureInfo.CurrentCulture),
            ValidUntilShort = devis.DateValidite.ToString("d", CultureInfo.CurrentCulture),
            TtcLabel = $"{ttc:N2} {devise}",
            IsExpired = devis.DateValidite.Date < today,
            NotePreview = DocumentListFormat.NotePreview(devis.Note),
        };
    }
}
