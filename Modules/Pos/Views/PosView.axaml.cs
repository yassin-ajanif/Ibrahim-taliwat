using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using GestionCommerciale.Modules.Pos.ViewModels;

namespace GestionCommerciale.Modules.Pos.Views;

public partial class PosView : UserControl
{
    private PosViewModel? _vm;
    private AutoCompleteBox? _clientBox;

    public PosView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
        Loaded += OnLoaded;
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        _clientBox ??= this.FindControl<AutoCompleteBox>("ClientBox");
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (_vm is not null)
            _vm.ClientFieldReset -= OnClientFieldReset;

        _vm = DataContext as PosViewModel;
        if (_vm is not null)
            _vm.ClientFieldReset += OnClientFieldReset;
    }

    private void OnClientFieldReset()
    {
        Dispatcher.UIThread.Post(() =>
        {
            _clientBox ??= this.FindControl<AutoCompleteBox>("ClientBox");
            if (_clientBox is null) return;

            _clientBox.SelectedItem = null;
            _clientBox.SetCurrentValue(AutoCompleteBox.TextProperty, string.Empty);
            _clientBox.Text = string.Empty;
        }, DispatcherPriority.Background);
    }

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
