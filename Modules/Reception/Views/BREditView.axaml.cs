using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using GestionCommerciale.Modules.Reception.ViewModels;

namespace GestionCommerciale.Modules.Reception.Views;

public partial class BREditView : UserControl
{
    public BREditView()
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
        if (DataContext is not BREditViewModel vm) return;
        e.Handled = true;
        vm.OpenLinkedFactureCommand.Execute(null);
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}
