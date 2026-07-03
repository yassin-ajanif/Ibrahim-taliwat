using Avalonia.Controls;
using Avalonia.Input;
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

    private void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (DataContext is PosViewModel vm)
            vm.SearchProductsCommand.Execute(null);
    }

    private void OnSearchKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter) return;
        if (DataContext is not PosViewModel vm) return;
        e.Handled = true;
        var text = vm.SearchText?.Trim();
        if (string.IsNullOrEmpty(text))
        {
            vm.SearchText = string.Empty;
            return;
        }

        var match = vm.SearchResults.FirstOrDefault(r =>
            !string.IsNullOrEmpty(r.CodeBarre) &&
            r.CodeBarre.Equals(text, StringComparison.OrdinalIgnoreCase));

        if (match is not null)
            vm.AddProductCommand.Execute(match);

        vm.SearchText = string.Empty;
    }
}
