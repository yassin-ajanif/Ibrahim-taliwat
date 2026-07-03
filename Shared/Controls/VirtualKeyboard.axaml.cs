using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace GestionCommerciale.Shared.Controls;

public partial class VirtualKeyboard : UserControl
{
    public event Action<string>? KeyPressed;
    public event Action? BackspacePressed;
    public event Action? Cleared;
    public event Action? Closed;

    public static readonly StyledProperty<bool> IsAlphaVisibleProperty =
        AvaloniaProperty.Register<VirtualKeyboard, bool>(nameof(IsAlphaVisible), true);

    public bool IsAlphaVisible
    {
        get => GetValue(IsAlphaVisibleProperty);
        set
        {
            SetValue(IsAlphaVisibleProperty, value);
            AlphaRow1.IsVisible = value;
            AlphaRow2.IsVisible = value;
            AlphaRow3.IsVisible = value;
            SpaceBtn.IsVisible = value;
        }
    }

    public VirtualKeyboard()
    {
        InitializeComponent();
    }

    private void OnKeyClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: string ch })
            KeyPressed?.Invoke(ch);
    }

    private void OnSpace(object? sender, RoutedEventArgs e)
    {
        KeyPressed?.Invoke(" ");
    }

    private void OnBackspace(object? sender, RoutedEventArgs e)
    {
        BackspacePressed?.Invoke();
    }

    private void OnClear(object? sender, RoutedEventArgs e)
    {
        Cleared?.Invoke();
    }

    private void OnClose(object? sender, RoutedEventArgs e)
    {
        Closed?.Invoke();
    }
}
