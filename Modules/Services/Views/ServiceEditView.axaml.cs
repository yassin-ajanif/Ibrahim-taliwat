using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace GestionCommerciale.Modules.Services.Views;

public partial class ServiceEditView : UserControl
{
    public ServiceEditView()
    {
        InitializeComponent();
    }

    private void OnUniteContextMenuOpening(object? sender, CancelEventArgs e)
    {
        if (sender is ContextMenu cm && cm.PlacementTarget is { DataContext: { } dc })
            cm.DataContext = dc;
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}
