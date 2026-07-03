using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace GestionCommerciale.Shared.Converters;

/// <summary>Maps bool (nav tab active) to brush; use ConverterParameter: bg | fg | border.</summary>
public sealed class NavTabHighlightConverter : IValueConverter
{
    public static readonly NavTabHighlightConverter Instance = new();

    private static readonly SolidColorBrush Primary = new(Color.Parse("#2563EB"));
    private static readonly SolidColorBrush Surface = new(Color.Parse("#FFFFFF"));
    private static readonly SolidColorBrush Text = new(Color.Parse("#1F2937"));
    private static readonly SolidColorBrush White = new(Colors.White);
    private static readonly SolidColorBrush Border = new(Color.Parse("#CFDCF7"));

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var active = value is true;
        var kind = (parameter?.ToString() ?? "bg").Trim().ToLowerInvariant();
        return kind switch
        {
            "fg" => active ? White : Text,
            "border" => active ? Primary : Border,
            _ => active ? Primary : Surface,
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
