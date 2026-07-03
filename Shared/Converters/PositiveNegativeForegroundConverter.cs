using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace GestionCommerciale.Shared.Converters;

public sealed class PositiveNegativeForegroundConverter : IValueConverter
{
    public static readonly PositiveNegativeForegroundConverter Instance = new();

    private static readonly IBrush Green = new SolidColorBrush(Color.Parse("#16A34A"));
    private static readonly IBrush Red = new SolidColorBrush(Color.Parse("#DC2626"));

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is decimal d ? (d >= 0 ? Green : Red) : null;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
