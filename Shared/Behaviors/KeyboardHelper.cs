using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using GestionCommerciale.Shared.Services;
using Microsoft.Extensions.DependencyInjection;

namespace GestionCommerciale.Shared.Behaviors;

public static class KeyboardHelper
{
    public static readonly AttachedProperty<bool> EnableKeyboardProperty =
        AvaloniaProperty.RegisterAttached<InputElement, bool>(
            "EnableKeyboard",
            typeof(KeyboardHelper));

    public static void SetEnableKeyboard(InputElement element, bool value) =>
        element.SetValue(EnableKeyboardProperty, value);

    public static bool GetEnableKeyboard(InputElement element) =>
        element.GetValue(EnableKeyboardProperty);

    static KeyboardHelper()
    {
        EnableKeyboardProperty.Changed.AddClassHandler<InputElement>(OnEnableKeyboardChanged);
    }

    private static void OnEnableKeyboardChanged(InputElement element, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is true)
        {
            element.DoubleTapped += OnDoubleTapped;
            element.DetachedFromVisualTree += OnDetached;
        }
        else
        {
            element.DoubleTapped -= OnDoubleTapped;
            element.DetachedFromVisualTree -= OnDetached;
        }
    }

    private static void OnDetached(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (sender is InputElement element)
        {
            element.DoubleTapped -= OnDoubleTapped;
            element.DetachedFromVisualTree -= OnDetached;
        }
    }

    private static void OnDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (sender is not InputElement element) return;

        var svc = App.Services.GetRequiredService<VirtualKeyboardService>();
        if (!svc.IsEnabled) return;

        var isNumeric = sender is NumericUpDown;
        svc.Show(element, isNumeric);
        e.Handled = true;
    }
}
