using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;
using GestionCommerciale.Shared.Services;

namespace GestionCommerciale.Shared.Converters;

public sealed class DevisLineSummaryMultiConverter : IMultiValueConverter
{
    public static readonly DevisLineSummaryMultiConverter Instance = new();

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count < 4)
            return string.Empty;
        var lang = culture.TwoLetterISOLanguageName.Equals("ar", StringComparison.OrdinalIgnoreCase) ? "ar" : "fr";
        var fmt = UiTranslations.Get("Devis_LineFmt", lang);
        try
        {
            var q = System.Convert.ToDecimal(values[0]);
            var pu = System.Convert.ToDecimal(values[1]);
            var rem = System.Convert.ToDecimal(values[2]);
            var tva = System.Convert.ToDecimal(values[3]);
            return string.Format(culture, fmt, q, pu, rem, tva);
        }
        catch
        {
            return string.Empty;
        }
    }

    public object[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
