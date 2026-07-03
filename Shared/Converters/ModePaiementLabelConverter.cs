using System;
using System.Globalization;
using Avalonia.Data.Converters;
using GestionCommerciale.Modules.Facturation.Models;
using GestionCommerciale.Shared.Services;

namespace GestionCommerciale.Shared.Converters;

public sealed class ModePaiementLabelConverter : IValueConverter
{
    public static readonly ModePaiementLabelConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not ModePaiement m)
            return value?.ToString() ?? string.Empty;
        var lang = culture.TwoLetterISOLanguageName.Equals("ar", StringComparison.OrdinalIgnoreCase) ? "ar" : "fr";
        var key = m switch
        {
            ModePaiement.Credit => "ModePaiement_Credit",
            ModePaiement.Cheque => "ModePaiement_Cheque",
            ModePaiement.Especes => "ModePaiement_Especes",
            ModePaiement.TPE => "ModePaiement_TPE",
            ModePaiement.Virement => "ModePaiement_Virement",
            ModePaiement.Effet => "ModePaiement_Effet",
            _ => "ModePaiement_Especes"
        };
        return UiTranslations.Get(key, lang);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
