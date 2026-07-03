using System.Globalization;
using FactureFournisseurEntity = GestionCommerciale.Modules.FactureFournisseur.Models.FactureFournisseur;
using GestionCommerciale.Shared.Helpers;
using GestionCommerciale.Shared.Services;

namespace GestionCommerciale.Modules.FactureFournisseur.ViewModels;

public sealed class FactureFournisseurListRow
{
    public required FactureFournisseurEntity FactureFournisseur { get; init; }
    public string FournisseurNom { get; init; } = string.Empty;
    public string DateShort { get; init; } = string.Empty;
    public string EcheanceShort { get; init; } = string.Empty;
    public string StatutLabel { get; init; } = string.Empty;
    public string HtLabel { get; init; } = string.Empty;
    public string TtcLabel { get; init; } = string.Empty;
    public string NotePreview { get; init; } = string.Empty;

    public static FactureFournisseurListRow Create(FactureFournisseurEntity f, string fournisseurNom, string devise, ILocaleService locale)
    {
        var (ht, _, ttc) = DocumentTotalsHelper.FactureFournisseurTotals(f.Lignes ?? [], f.RemiseGlobale);
        return new FactureFournisseurListRow
        {
            FactureFournisseur = f,
            FournisseurNom = fournisseurNom,
            DateShort = f.Date.ToString("d", CultureInfo.CurrentCulture),
            EcheanceShort = f.DateEcheance.ToString("d", CultureInfo.CurrentCulture),
            StatutLabel = f.EstPayee ? locale.T("Faf_Paid") : locale.T("Faf_Unpaid"),
            HtLabel = locale.Tf("Doc_FmtHt", ht, devise),
            TtcLabel = $"{ttc:N2} {devise}",
            NotePreview = DocumentListFormat.NotePreview(f.Note),
        };
    }
}
