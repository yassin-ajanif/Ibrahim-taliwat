using System.Globalization;
using GestionCommerciale.Shared.Helpers;
using GestionCommerciale.Shared.Services;

namespace GestionCommerciale.Modules.AvoirFournisseur.ViewModels;

public sealed class AvoirFournisseurListRow
{
    public required Models.AvoirFournisseur Doc { get; init; }
    public string FournisseurNom { get; init; } = string.Empty;
    public string DateShort { get; init; } = string.Empty;
    public string HtLabel { get; init; } = string.Empty;
    public string TtcLabel { get; init; } = string.Empty;
    public string NotePreview { get; init; } = string.Empty;

    public static AvoirFournisseurListRow Create(Models.AvoirFournisseur doc, string fournisseurNom, string devise, ILocaleService locale)
    {
        var (ht, _, ttc) = DocumentTotalsHelper.AvoirFournisseurTotals(doc.Lignes ?? []);
        return new AvoirFournisseurListRow
        {
            Doc = doc,
            FournisseurNom = fournisseurNom,
            DateShort = doc.Date.ToString("d", CultureInfo.CurrentCulture),
            HtLabel = locale.Tf("Doc_FmtHt", ht, devise),
            TtcLabel = $"{ttc:N2} {devise}",
            NotePreview = DocumentListFormat.NotePreview(doc.Motif),
        };
    }
}
