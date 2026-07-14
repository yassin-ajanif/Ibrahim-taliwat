using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using GestionCommerciale.Modules.Reporting.ViewModels;

namespace GestionCommerciale.Modules.Reporting.Views;

public partial class ReportsListView : UserControl
{
    public ReportsListView()
    {
        InitializeComponent();
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    private void OnProfitFilterCardTapped(object? sender, TappedEventArgs e)
    {
        if (DataContext is not ReportsListViewModel vm || sender is not Border border)
            return;

        switch (border.Tag as string)
        {
            case "Margin":
                vm.FilterProfitMarginCommand.Execute(null);
                break;
            case "AvoirsClient":
                vm.FilterProfitAvoirsClientCommand.Execute(null);
                break;
            case "Purchases":
                vm.FilterProfitPurchasesCommand.Execute(null);
                break;
            case "AvoirsFournisseur":
                vm.FilterProfitAvoirsFournisseurCommand.Execute(null);
                break;
            case "Charges":
                vm.FilterProfitChargesCommand.Execute(null);
                break;
            case "All":
                vm.FilterProfitAllCommand.Execute(null);
                break;
        }
    }
}
