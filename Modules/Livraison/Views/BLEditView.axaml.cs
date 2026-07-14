using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using GestionCommerciale.Modules.Livraison.ViewModels;

namespace GestionCommerciale.Modules.Livraison.Views;

public partial class BLEditView : UserControl
{
    public BLEditView()
    {
        InitializeComponent();
    }

    private void OnHeaderContextMenuOpening(object? sender, CancelEventArgs e)
    {
        if (sender is ContextMenu cm && cm.PlacementTarget is { DataContext: { } dc })
            cm.DataContext = dc;
    }

    private void OnInvoicedChipTapped(object? sender, TappedEventArgs e)
    {
        if (DataContext is not BLEditViewModel vm) return;
        e.Handled = true;
        vm.OpenLinkedFactureCommand.Execute(null);
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}
