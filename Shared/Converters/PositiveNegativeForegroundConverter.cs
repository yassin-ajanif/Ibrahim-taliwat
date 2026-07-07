using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace GestionCommerciale.Shared.Converters;

public sealed class PositiveNegativeForegroundConverter : IValueConverter
{
    public static readonly PositiveNegativeForegroundConverter Instance = new();

    private static readonly IBrush Green = new SolidColorBrush(Color.Parse("#16A34A"));
    private static readonly IBrush Red = new SolidColorBrush(Color.Parse("#DC2626"));
    private static readonly IBrush GreenBg = new SolidColorBrush(Color.Parse("#DCFCE7"));
    private static readonly IBrush RedBg = new SolidColorBrush(Color.Parse("#FEE2E2"));

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var isPositive = value switch
        {
            bool b => b,
            decimal d => d >= 0,
            _ => (bool?)null
        };
        if (isPositive is null) return null;

        var kind = (parameter?.ToString() ?? "fg").Trim().ToLowerInvariant();
        return kind switch
        {
            "bg" => isPositive.Value ? GreenBg : RedBg,
            _ => isPositive.Value ? Green : Red,
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
