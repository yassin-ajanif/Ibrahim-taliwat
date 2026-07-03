using System;
using System.Globalization;
using Avalonia.Data.Converters;
using GestionCommerciale.Shared.Services;

namespace GestionCommerciale.Shared.Converters;

public sealed class DevisProduitIdLabelConverter : IValueConverter
{
    public static readonly DevisProduitIdLabelConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not int id)
            return value?.ToString() ?? string.Empty;
        var lang = culture.TwoLetterISOLanguageName.Equals("ar", StringComparison.OrdinalIgnoreCase) ? "ar" : "fr";
        var fmt = UiTranslations.Get("Devis_ProduitNum", lang);
        return string.Format(culture, fmt, id);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
