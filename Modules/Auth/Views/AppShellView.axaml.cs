using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using GestionCommerciale.Shared.Controls;
using GestionCommerciale.Shared.Services;
using Microsoft.Extensions.DependencyInjection;

namespace GestionCommerciale.Modules.Auth.Views;

public partial class AppShellView : UserControl
{
    private VirtualKeyboardService _keyboardService = null!;
    private Grid? _overlay;
    private VirtualKeyboard? _keyboard;

    public AppShellView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        _keyboardService = App.Services.GetRequiredService<VirtualKeyboardService>();
        _keyboardService.PropertyChanged += OnKeyboardServicePropertyChanged;
        _overlay = this.FindControl<Grid>("KeyboardOverlay");
        _keyboard = this.FindControl<VirtualKeyboard>("AppKeyboard");
        _overlay!.IsVisible = false;
    }

    private void OnKeyboardServicePropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(VirtualKeyboardService.IsOpen) && _overlay != null && _keyboard != null)
        {
            _overlay.IsVisible = _keyboardService.IsOpen;
            _keyboard.IsAlphaVisible = _keyboardService.IsAlphaVisible;
        }
    }

    private void OnKeyboardOverlayPointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        _keyboardService.Hide();
    }

    private void OnKeyboardKeyPressed(string ch)
    {
        _keyboardService.PressKey(ch);
    }

    private void OnKeyboardBackspace()
    {
        _keyboardService.Backspace();
    }

    private void OnKeyboardClear()
    {
        _keyboardService.Clear();
    }

    private void OnKeyboardClose()
    {
        _keyboardService.Hide();
    }
}
