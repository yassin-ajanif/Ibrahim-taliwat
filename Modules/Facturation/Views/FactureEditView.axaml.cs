using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using GestionCommerciale.Modules.Facturation.ViewModels;

namespace GestionCommerciale.Modules.Facturation.Views;

public partial class FactureEditView : UserControl
{
    public FactureEditView()
    {
        InitializeComponent();
    }

    private void OnHeaderContextMenuOpening(object? sender, CancelEventArgs e)
    {
        if (sender is ContextMenu cm && cm.PlacementTarget is { DataContext: { } dc })
            cm.DataContext = dc;
    }

    private void OnLinkedBlNumeroTapped(object? sender, TappedEventArgs e)
    {
        if (DataContext is not FactureEditViewModel vm) return;
        if (sender is not Control { DataContext: LinkedBlRow bl }) return;
        e.Handled = true;
        vm.OpenLinkedBlCommand.Execute(bl);
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}
