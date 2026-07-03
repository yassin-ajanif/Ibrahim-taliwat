using System;
using System.Globalization;
using Avalonia.Data.Converters;
using GestionCommerciale.Shared.Services;

namespace GestionCommerciale.Shared.Converters;

public sealed class BoolActifLabelConverter : IValueConverter
{
    public static readonly BoolActifLabelConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not bool b)
            return value?.ToString() ?? string.Empty;
        var ar = culture.TwoLetterISOLanguageName.Equals("ar", StringComparison.OrdinalIgnoreCase);
        return UiTranslations.Get(b ? "Bool_Actif" : "Bool_Inactif", ar ? "ar" : "fr");
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
