using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace GestionCommerciale.Shared.Converters;

/// <summary>Highlights past validity dates; otherwise uses subtle list text color.</summary>
public sealed class ExpiredValidityForegroundConverter : IValueConverter
{
    public static readonly ExpiredValidityForegroundConverter Instance = new();

    private static readonly IBrush Warning = new SolidColorBrush(Color.Parse("#D97706"));
    private static readonly IBrush Subtle = new SolidColorBrush(Color.Parse("#5E7296"));

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is true ? Warning : Subtle;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
