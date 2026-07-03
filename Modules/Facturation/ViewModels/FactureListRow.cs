using System.Globalization;
using FactureEntity = GestionCommerciale.Modules.Facturation.Models.Facture;
using GestionCommerciale.Shared.Helpers;
using GestionCommerciale.Shared.Services;

namespace GestionCommerciale.Modules.Facturation.ViewModels;

public sealed class FactureListRow
{
    public required FactureEntity Facture { get; init; }
    public string ClientNom { get; init; } = string.Empty;
    public string DateShort { get; init; } = string.Empty;
    public string EcheanceShort { get; init; } = string.Empty;
    public string StatutLabel { get; init; } = string.Empty;
    public string HtLabel { get; init; } = string.Empty;
    public string TtcLabel { get; init; } = string.Empty;
    public string NotePreview { get; init; } = string.Empty;
    public bool IsOverdue { get; init; }

    public static FactureListRow Create(FactureEntity f, string clientNom, string devise, ILocaleService locale)
    {
        var (ht, _, ttc) = DocumentTotalsHelper.FactureTotals(f.Lignes ?? [], f.RemiseGlobale);
        var isOverdue = !f.EstPayee && f.DateEcheance.Date < DateTime.Today;
        return new FactureListRow
        {
            Facture = f,
            ClientNom = clientNom,
            DateShort = f.Date.ToString("d", CultureInfo.CurrentCulture),
            EcheanceShort = f.DateEcheance.ToString("d", CultureInfo.CurrentCulture),
            StatutLabel = f.EstPayee ? locale.T("Fact_Paid") : locale.T("Fact_Unpaid"),
            HtLabel = locale.Tf("Doc_FmtHt", ht, devise),
            TtcLabel = $"{ttc:N2} {devise}",
            NotePreview = DocumentListFormat.NotePreview(f.Note),
            IsOverdue = isOverdue,
        };
    }
}
