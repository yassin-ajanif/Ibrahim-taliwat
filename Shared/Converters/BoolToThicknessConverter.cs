using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace GestionCommerciale.Shared.Converters;

/// <summary>Maps bool to Thickness: true → 2 (selected), false → 1 (default).</summary>
public sealed class BoolToThicknessConverter : IValueConverter
{
    public static readonly BoolToThicknessConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var active = value is true;
        return active ? new Thickness(2) : new Thickness(1);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
