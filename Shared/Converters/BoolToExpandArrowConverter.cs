using System.Globalization;
using Avalonia.Data.Converters;

namespace GestionCommerciale.Shared.Converters;

public sealed class BoolToExpandArrowConverter : IValueConverter
{
    public static readonly BoolToExpandArrowConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool expanded)
            return expanded ? "\u25BC" : "\u25B6";
        return "\u25B6";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
