using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using GestionCommerciale.Modules.Stock.Models;

namespace GestionCommerciale.Shared.Converters;

public sealed class StockMinForegroundConverter : IValueConverter
{
    public static readonly StockMinForegroundConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Produit p && p.StockMinimum > 0 && p.StockActuel < p.StockMinimum)
            return Brushes.DarkRed;
        return Brushes.Black;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
