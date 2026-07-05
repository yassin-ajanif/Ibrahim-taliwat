using System.Globalization;
using GestionCommerciale.Modules.Charges.Models;
using GestionCommerciale.Shared.Helpers;
using GestionCommerciale.Shared.Services;

namespace GestionCommerciale.Modules.Charges.ViewModels;

public sealed class ChargesListRow
{
    public required Charge Charge { get; init; }
    public string TypeNom { get; init; } = string.Empty;
    public string BeneficiaireLabel { get; init; } = string.Empty;
    public string DateShort { get; init; } = string.Empty;
    public string TtcLabel { get; init; } = string.Empty;
    public string NotePreview { get; init; } = string.Empty;

    public static ChargesListRow Create(Charge charge, string typeNom, string? fournisseurNom, string devise, ILocaleService locale)
    {
        var beneficiary = !string.IsNullOrWhiteSpace(fournisseurNom)
            ? fournisseurNom
            : charge.BeneficiaireLibre;

        return new ChargesListRow
        {
            Charge = charge,
            TypeNom = typeNom,
            BeneficiaireLabel = beneficiary,
            DateShort = charge.Date.ToString("d", CultureInfo.CurrentCulture),
            TtcLabel = $"{charge.MontantTtc:N2} {devise}",
            NotePreview = DocumentListFormat.NotePreview(charge.Note),
        };
    }
}
