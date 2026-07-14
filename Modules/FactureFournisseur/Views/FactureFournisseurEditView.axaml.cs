using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using GestionCommerciale.Modules.FactureFournisseur.ViewModels;

namespace GestionCommerciale.Modules.FactureFournisseur.Views;

public partial class FactureFournisseurEditView : UserControl
{
    public FactureFournisseurEditView()
    {
        InitializeComponent();
    }

    private void OnHeaderContextMenuOpening(object? sender, CancelEventArgs e)
    {
        if (sender is ContextMenu cm && cm.PlacementTarget is { DataContext: { } dc })
            cm.DataContext = dc;
    }

    private void OnLinkedBrNumeroTapped(object? sender, TappedEventArgs e)
    {
        if (DataContext is not FactureFournisseurEditViewModel vm) return;
        if (sender is not Control { DataContext: LinkedBrRow br }) return;
        e.Handled = true;
        vm.OpenLinkedBrCommand.Execute(br);
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}
