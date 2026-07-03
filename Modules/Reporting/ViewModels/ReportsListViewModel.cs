using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionCommerciale.Modules.Reporting.Services;
using GestionCommerciale.Modules.Auth.Services;
using GestionCommerciale.Shared.Helpers;
using GestionCommerciale.Shared.Services;
using GestionCommerciale.Shared.ViewModels;

namespace GestionCommerciale.Modules.Reporting.ViewModels;

public partial class ReportsListViewModel : BaseViewModel
{
    private readonly IReportService _reportService;
    private readonly IDialogService _dialog;
    private readonly ICurrentUserSession _session;
    private readonly ILocaleService _locale;

    public ReportsListViewModel(
        IReportService reportService,
        IDialogService dialog,
        ICurrentUserSession session,
        ILocaleService locale)
    {
        _reportService = reportService;
        _dialog = dialog;
        _session = session;
        _locale = locale;
        _locale.CultureApplied += (_, _) => RefreshLabels();
        Pagination = new PaginationHelper(ApplyCurrentPage);
        RefreshLabels();
        Title = _locale.T("Reports_Title");
    }

    public PaginationHelper Pagination { get; }

    [ObservableProperty] private string _lblTitle = string.Empty;
    [ObservableProperty] private string _lblDateFrom = string.Empty;
    [ObservableProperty] private string _lblDateTo = string.Empty;
    [ObservableProperty] private string _lblApply = string.Empty;
    [ObservableProperty] private string _lblLoading = string.Empty;

    [ObservableProperty] private string _btnSaleByProduct = string.Empty;
    [ObservableProperty] private string _btnSaleByCustomer = string.Empty;
    [ObservableProperty] private string _btnRefunds = string.Empty;
    [ObservableProperty] private string _btnDailySales = string.Empty;
    [ObservableProperty] private string _btnUnpaid = string.Empty;
    [ObservableProperty] private string _btnStockMovements = string.Empty;

    [ObservableProperty] private int _selectedReportIndex;
    [ObservableProperty] private DateTimeOffset _dateFrom = new(DateTime.Today.AddDays(-30));
    [ObservableProperty] private DateTimeOffset _dateTo = new(DateTime.Today);

    // visible columns for each report — used in view
    [ObservableProperty] private bool _showSaleByProduct;
    [ObservableProperty] private bool _showSaleByCustomer;
    [ObservableProperty] private bool _showRefunds;
    [ObservableProperty] private bool _showDailySales;
    [ObservableProperty] private bool _showUnpaid;
    [ObservableProperty] private bool _showStockMovements;

    [ObservableProperty] private bool _showEmpty;
    [ObservableProperty] private bool _showDateFilter = true;
    [ObservableProperty] private string _emptyMessage = string.Empty;
    [ObservableProperty] private string _lblSaleByCustomerTotalHt = string.Empty;
    [ObservableProperty] private string _lblSaleByCustomerTotalTtc = string.Empty;
    [ObservableProperty] private string _lblSaleByCustomerLabelHt = string.Empty;
    [ObservableProperty] private string _lblSaleByCustomerLabelTtc = string.Empty;
    [ObservableProperty] private string _lblSaleByCustomerLabelProfit = string.Empty;
    [ObservableProperty] private string _lblSaleByCustomerTotalProfit = string.Empty;
    [ObservableProperty] private string _lblDailySalesTotalProfit = string.Empty;
    [ObservableProperty] private string _lblStockValHtLabel = string.Empty;
    [ObservableProperty] private string _lblStockValTtcLabel = string.Empty;
    [ObservableProperty] private string _lblStockValHt = string.Empty;
    [ObservableProperty] private string _lblStockValTtc = string.Empty;
    [ObservableProperty] private bool _showPagination;

    private List<ReportSaleByProductRow> _allSalesByProduct = [];
    private List<ReportSaleByCustomerRow> _allSalesByCustomer = [];
    private List<ReportRefundRow> _allRefunds = [];
    private List<ReportDailySaleRow> _allDailySales = [];
    private List<ReportUnpaidRow> _allUnpaidSales = [];
    private List<ReportStockMovementRow> _allStockMovements = [];

    public ObservableCollection<ReportSaleByProductRow> SalesByProduct { get; } = [];
    public ObservableCollection<ReportSaleByCustomerRow> SalesByCustomer { get; } = [];
    public ObservableCollection<ReportRefundRow> Refunds { get; } = [];
    public ObservableCollection<ReportDailySaleRow> DailySales { get; } = [];
    public ObservableCollection<ReportUnpaidRow> UnpaidSales { get; } = [];
    public ObservableCollection<ReportStockMovementRow> StockMovements { get; } = [];

    private void RefreshLabels()
    {
        Title = _locale.T("Reports_Title");
        LblTitle = _locale.T("Reports_Title");
        LblDateFrom = _locale.T("Reports_From");
        LblDateTo = _locale.T("Reports_To");
        LblApply = _locale.T("Reports_Apply");
        LblLoading = _locale.T("Report_Loading");
        BtnSaleByProduct = _locale.T("Reports_BtnSaleByProduct");
        BtnSaleByCustomer = _locale.T("Reports_BtnSaleByCustomer");
        BtnRefunds = _locale.T("Reports_BtnRefunds");
        BtnDailySales = _locale.T("Reports_BtnDailySales");
        BtnUnpaid = _locale.T("Reports_BtnUnpaid");
        BtnStockMovements = _locale.T("Reports_BtnStockMovements");
        EmptyMessage = _locale.T("Reports_Empty");
        LblSaleByCustomerLabelHt = _locale.T("Reports_LblTotalHt");
        LblSaleByCustomerLabelTtc = _locale.T("Reports_LblTotalTtc");
        LblSaleByCustomerLabelProfit = _locale.T("Reports_LblTotalProfit");
        LblStockValHtLabel = _locale.T("Reports_LblStockValHt");
        LblStockValTtcLabel = _locale.T("Reports_LblStockValTtc");
    }

    partial void OnSelectedReportIndexChanged(int value)
    {
        ShowSaleByProduct = value == 0;
        ShowSaleByCustomer = value == 1;
        ShowRefunds = value == 2;
        ShowDailySales = value == 3;
        ShowUnpaid = value == 4;
        ShowStockMovements = value == 5;
        ShowDateFilter = value != 4;
        LoadReportCommand.Execute(null);
    }

    [RelayCommand] private void GoSaleByProduct() => SelectedReportIndex = 0;
    [RelayCommand] private void GoSaleByCustomer() => SelectedReportIndex = 1;
    [RelayCommand] private void GoRefunds() => SelectedReportIndex = 2;
    [RelayCommand] private void GoDailySales() => SelectedReportIndex = 3;
    [RelayCommand] private void GoUnpaid() => SelectedReportIndex = 4;
    [RelayCommand] private void GoStockMovements() => SelectedReportIndex = 5;

    [RelayCommand]
    private void ToggleCustomerExpand(ReportSaleByCustomerRow? row)
    {
        if (row != null)
            row.IsExpanded = !row.IsExpanded;
    }

    [RelayCommand]
    private void ToggleDailyExpand(ReportDailySaleRow? row)
    {
        if (row != null)
            row.IsExpanded = !row.IsExpanded;
    }

    [RelayCommand]
    private async Task LoadReportAsync(CancellationToken cancellationToken)
    {
        if (!_session.CanAccessReporting)
        {
            await _dialog.ShowErrorAsync(_locale.T("Report_Title"), _locale.T("Report_ErrDenied"), cancellationToken);
            return;
        }

        IsBusy = true;
        ShowEmpty = false;
        try
        {
            await Task.Yield();

            var from = DateFrom.Date;
            var to = DateTo.Date;

            switch (SelectedReportIndex)
            {
                case 0:
                    await LoadSalesByProductAsync(from, to, cancellationToken);
                    break;
                case 1:
                    await LoadSalesByCustomerAsync(from, to, cancellationToken);
                    break;
                case 2:
                    await LoadRefundsAsync(from, to, cancellationToken);
                    break;
                case 3:
                    await LoadDailySalesAsync(from, to, cancellationToken);
                    break;
                case 4:
                    await LoadUnpaidAsync(cancellationToken);
                    break;
                case 5:
                    await LoadStockMovementsAsync(from, to, cancellationToken);
                    break;
            }
        }
        catch (Exception ex)
        {
            await _dialog.ShowErrorAsync(_locale.T("Report_Title"), ex.Message, cancellationToken);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadSalesByProductAsync(DateTime from, DateTime to, CancellationToken ct)
    {
        _allSalesByProduct = await Task.Run(() => _reportService.GetSalesByProductAsync(from, to, ct), ct);
        FinishPagedLoad(_allSalesByProduct.Count);
    }

    private async Task LoadSalesByCustomerAsync(DateTime from, DateTime to, CancellationToken ct)
    {
        _allSalesByCustomer = await Task.Run(() => _reportService.GetSalesByCustomerAsync(from, to, ct), ct);
        var dev = _allSalesByCustomer.Count > 0 ? _allSalesByCustomer[0].Devise : "MAD";
        LblSaleByCustomerTotalHt = $"{_allSalesByCustomer.Sum(r => r.TotalHt):N2} {dev}";
        LblSaleByCustomerTotalTtc = $"{_allSalesByCustomer.Sum(r => r.TotalTtc):N2} {dev}";
        LblSaleByCustomerTotalProfit = $"{_allSalesByCustomer.Sum(r => r.Profit):N2} {dev}";
        FinishPagedLoad(_allSalesByCustomer.Count);
    }

    private async Task LoadRefundsAsync(DateTime from, DateTime to, CancellationToken ct)
    {
        _allRefunds = await Task.Run(() => _reportService.GetRefundsAsync(from, to, ct), ct);
        FinishPagedLoad(_allRefunds.Count);
    }

    private async Task LoadDailySalesAsync(DateTime from, DateTime to, CancellationToken ct)
    {
        _allDailySales = await Task.Run(() => _reportService.GetDailySalesAsync(from, to, ct), ct);
        var dev = _allDailySales.Count > 0 ? _allDailySales[0].Devise : "MAD";
        LblDailySalesTotalProfit = $"{_allDailySales.Sum(r => r.Profit):N2} {dev}";
        FinishPagedLoad(_allDailySales.Count);
    }

    private async Task LoadUnpaidAsync(CancellationToken ct)
    {
        _allUnpaidSales = await Task.Run(() => _reportService.GetUnpaidSalesAsync(ct), ct);
        FinishPagedLoad(_allUnpaidSales.Count);
    }

    private async Task LoadStockMovementsAsync(DateTime from, DateTime to, CancellationToken ct)
    {
        _allStockMovements = await Task.Run(() => _reportService.GetStockMovementsAsync(from, to, ct), ct);
        var valuation = await Task.Run(() => _reportService.GetStockValuationAsync(ct), ct);
        LblStockValHt = $"{valuation.ht:N2} {valuation.devise}";
        LblStockValTtc = $"{valuation.ttc:N2} {valuation.devise}";
        FinishPagedLoad(_allStockMovements.Count);
    }

    private void FinishPagedLoad(int totalCount)
    {
        Pagination.CurrentPage = 1;
        Pagination.TotalCount = totalCount;
        ShowEmpty = totalCount == 0;
        ShowPagination = totalCount > 0;
        ApplyCurrentPage();
    }

    private void ApplyCurrentPage()
    {
        switch (SelectedReportIndex)
        {
            case 0:
                ApplyPage(SalesByProduct, _allSalesByProduct);
                break;
            case 1:
                ApplyPage(SalesByCustomer, _allSalesByCustomer);
                break;
            case 2:
                ApplyPage(Refunds, _allRefunds);
                break;
            case 3:
                ApplyPage(DailySales, _allDailySales);
                break;
            case 4:
                ApplyPage(UnpaidSales, _allUnpaidSales);
                break;
            case 5:
                ApplyPage(StockMovements, _allStockMovements);
                break;
        }
    }

    private void ApplyPage<T>(ObservableCollection<T> target, IReadOnlyList<T> source)
    {
        target.Clear();
        foreach (var item in source.Skip(Pagination.Skip).Take(Pagination.PageSize))
            target.Add(item);
    }
}
