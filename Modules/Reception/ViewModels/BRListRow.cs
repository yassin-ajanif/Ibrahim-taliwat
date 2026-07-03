using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using GestionCommerciale.Modules.Reception.Models;
using GestionCommerciale.Shared.Helpers;
using GestionCommerciale.Shared.Services;

namespace GestionCommerciale.Modules.Reception.ViewModels;

public partial class BRListRow : ObservableObject
{
    public required BonReception Br { get; init; }
    public string FournisseurNom { get; init; } = string.Empty;
    public string DateShort { get; init; } = string.Empty;
    public string HtLabel { get; init; } = string.Empty;
    public string TtcLabel { get; init; } = string.Empty;
    public string NotePreview { get; init; } = string.Empty;

    [ObservableProperty] private bool _isSelected;
    [ObservableProperty] private string _invoicedLabel = string.Empty;

    public bool HasInvoicedLabel => !string.IsNullOrEmpty(InvoicedLabel);

    partial void OnInvoicedLabelChanged(string value) => OnPropertyChanged(nameof(HasInvoicedLabel));

    public bool CanInvoice => Br.FactureFournisseurId == null;

    public static BRListRow Create(BonReception br, string fournisseurNom, string devise, ILocaleService locale)
    {
        var (ht, _, ttc) = DocumentTotalsHelper.BonReceptionTotals(br.Lignes ?? []);
        return new BRListRow
        {
            Br = br,
            FournisseurNom = fournisseurNom,
            DateShort = br.Date.ToString("d", CultureInfo.CurrentCulture),
            HtLabel = locale.Tf("Doc_FmtHt", ht, devise),
            TtcLabel = $"{ttc:N2} {devise}",
            NotePreview = DocumentListFormat.NotePreview(br.Note),
        };
    }
}
