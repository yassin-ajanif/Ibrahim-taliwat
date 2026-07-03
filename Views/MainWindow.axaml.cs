using Avalonia.Controls;
using GestionCommerciale.Shared.Services;
using Microsoft.Extensions.DependencyInjection;

namespace GestionCommerciale.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        var locale = App.Services.GetRequiredService<ILocaleService>();
        ApplyFlowDirection(locale);
        locale.CultureApplied += (_, _) => ApplyFlowDirection(locale);
    }

    private void ApplyFlowDirection(ILocaleService locale) =>
        FlowDirection = locale.IsRightToLeft
            ? global::Avalonia.Media.FlowDirection.RightToLeft
            : global::Avalonia.Media.FlowDirection.LeftToRight;
}