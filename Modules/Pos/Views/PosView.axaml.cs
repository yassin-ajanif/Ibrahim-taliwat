using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using GestionCommerciale.Modules.Pos.ViewModels;

namespace GestionCommerciale.Modules.Pos.Views;

public partial class PosView : UserControl
{
    public PosView()
    {
        InitializeComponent();
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    private async void OnSearchKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter) return;
        if (DataContext is not PosViewModel vm) return;
        e.Handled = true;

        var text = vm.SearchText?.Trim();
        if (string.IsNullOrEmpty(text))
            return;

        await vm.TryAddByBarcodeAsync(text);
        vm.SearchText = string.Empty;
    }

    private void OnProductCardTapped(object? sender, TappedEventArgs e)
    {
        if (DataContext is not PosViewModel vm || sender is not Border border)
            return;

        if (border.Tag is CatalogSearchRow row)
            vm.AddProductCommand.Execute(row);
    }
}
