using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace GestionCommerciale.Modules.Charges.Views;

public partial class ChargeEditView : UserControl
{
    public ChargeEditView()
    {
        InitializeComponent();
    }

    private void OnTypeContextMenuOpening(object? sender, CancelEventArgs e)
    {
        if (sender is ContextMenu cm && cm.PlacementTarget is { DataContext: { } dc })
            cm.DataContext = dc;
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}
