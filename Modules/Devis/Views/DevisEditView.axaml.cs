using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace GestionCommerciale.Modules.Devis.Views;

public partial class DevisEditView : UserControl
{
    public DevisEditView()
    {
        InitializeComponent();
    }

    /// <summary>Context menus open in a popup without inheriting the anchor DataContext; sync so bindings resolve.</summary>
    private void OnHeaderContextMenuOpening(object? sender, CancelEventArgs e)
    {
        if (sender is ContextMenu cm && cm.PlacementTarget is { DataContext: { } dc })
            cm.DataContext = dc;
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}
