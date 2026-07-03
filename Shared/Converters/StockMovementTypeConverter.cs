using System.Globalization;
using Avalonia.Data.Converters;
using GestionCommerciale.Modules.Stock.Models;
using GestionCommerciale.Shared.Services;

namespace GestionCommerciale.Shared.Converters;

public sealed class StockMovementTypeConverter : IValueConverter
{
    public static readonly StockMovementTypeConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not TypeMouvement t)
            return value?.ToString() ?? string.Empty;
        var tag = culture.TwoLetterISOLanguageName.Equals("ar", StringComparison.OrdinalIgnoreCase) ? "ar" : "fr";
        var key = t switch
        {
            TypeMouvement.Entree => "TypeMvt_Entree",
            TypeMouvement.Sortie => "TypeMvt_Sortie",
            TypeMouvement.Ajustement => "TypeMvt_Ajustement",
            _ => "TypeMvt_Ajustement"
        };
        return UiTranslations.Get(key, tag);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
