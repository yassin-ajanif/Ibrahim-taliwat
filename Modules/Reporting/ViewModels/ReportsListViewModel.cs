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
        DatePresets = new DatePresetChipsModel(_locale, (from, to) =>
        {
            DateFrom = new DateTimeOffset(from);
            DateTo = new DateTimeOffset(to);
            LoadReportCommand.Execute(null);
        });
        DatePresets.SyncSelection(DateFrom.Date, DateTo.Date);
        ShowProfitCharges = true;
        RefreshLabels();
        Title = _locale.T("Reports_Title");
    }

    public PaginationHelper Pagination { get; }
    public DatePresetChipsModel DatePresets { get; }

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
    [ObservableProperty] private string _btnProfitCharges = string.Empty;

    [ObservableProperty] private int _selectedReportIndex;
    [ObservableProperty] private DateTimeOffset _dateFrom = new(DateTime.Today);
    [ObservableProperty] private DateTimeOffset _dateTo = new(DateTime.Today);

    // visible columns for each report — used in view
    [ObservableProperty] private bool _showSaleByProduct;
    [ObservableProperty] private bool _showSaleByCustomer;
    [ObservableProperty] private bool _showRefunds;
    [ObservableProperty] private bool _showDailySales;
    [ObservableProperty] private bool _showUnpaid;
    [ObservableProperty] private bool _showStockMovements;
    [ObservableProperty] private bool _showProfitCharges;

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
    [ObservableProperty] private string _lblProfitChargesTotalMargin = string.Empty;
    [ObservableProperty] private string _lblProfitChargesTotalAvoirsClient = string.Empty;
    [ObservableProperty] private string _lblProfitChargesTotalPurchases = string.Empty;
    [ObservableProperty] private string _lblProfitChargesTotalAvoirsFournisseur = string.Empty;
    [ObservableProperty] private string _lblProfitChargesTotalCharges = string.Empty;
    [ObservableProperty] private string _lblProfitChargesNetResult = string.Empty;
    [ObservableProperty] private bool _isNetPositive = true;
    [ObservableProperty] private string _lblProfitChargesMarginLabel = string.Empty;
    [ObservableProperty] private string _lblProfitChargesAvoirsClientLabel = string.Empty;
    [ObservableProperty] private string _lblProfitChargesPurchasesLabel = string.Empty;
    [ObservableProperty] private string _lblProfitChargesAvoirsFournisseurLabel = string.Empty;
    [ObservableProperty] private string _lblProfitChargesChargesLabel = string.Empty;
    [ObservableProperty] private string _lblProfitChargesNetLabel = string.Empty;
    [ObservableProperty] private string _colProfitType = string.Empty;
    [ObservableProperty] private string _colProfitRef = string.Empty;
    [ObservableProperty] private string _colProfitDate = string.Empty;
    [ObservableProperty] private string _colProfitHt = string.Empty;
    [ObservableProperty] private string _colProfitAmount = string.Empty;
    [ObservableProperty] private bool _showPagination;
    [ObservableProperty] private bool _isProfitFilterMarginActive;
    [ObservableProperty] private bool _isProfitFilterAvoirsClientActive;
    [ObservableProperty] private bool _isProfitFilterPurchasesActive;
    [ObservableProperty] private bool _isProfitFilterAvoirsFournisseurActive;
    [ObservableProperty] private bool _isProfitFilterChargesActive;
    [ObservableProperty] private bool _isProfitFilterAllActive = true;

    private List<ReportSaleByProductRow> _allSalesByProduct = [];
    private List<ReportSaleByCustomerRow> _allSalesByCustomer = [];
    private List<ReportRefundRow> _allRefunds = [];
    private List<ReportDailySaleRow> _allDailySales = [];
    private List<ReportUnpaidRow> _allUnpaidSales = [];
    private List<ReportStockMovementRow> _allStockMovements = [];
    private List<ReportProfitChargeRow> _allProfitCharges = [];
    private List<ReportProfitChargeRow> _filteredProfitCharges = [];
    private ReportProfitChargeKind? _profitFilterKind;

    public ObservableCollection<ReportSaleByProductRow> SalesByProduct { get; } = [];
    public ObservableCollection<ReportSaleByCustomerRow> SalesByCustomer { get; } = [];
    public ObservableCollection<ReportRefundRow> Refunds { get; } = [];
    public ObservableCollection<ReportDailySaleRow> DailySales { get; } = [];
    public ObservableCollection<ReportUnpaidRow> UnpaidSales { get; } = [];
    public ObservableCollection<ReportStockMovementRow> StockMovements { get; } = [];
    public ObservableCollection<ReportProfitChargeRow> ProfitCharges { get; } = [];

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
        BtnProfitCharges = _locale.T("Reports_BtnProfitCharges");
        EmptyMessage = _locale.T("Reports_Empty");
        LblSaleByCustomerLabelHt = _locale.T("Reports_LblTotalHt");
        LblSaleByCustomerLabelTtc = _locale.T("Reports_LblTotalTtc");
        LblSaleByCustomerLabelProfit = _locale.T("Reports_LblTotalProfit");
        LblStockValHtLabel = _locale.T("Reports_LblStockValHt");
        LblStockValTtcLabel = _locale.T("Reports_LblStockValTtc");
        LblProfitChargesMarginLabel = _locale.T("Reports_LblTotalSalesMargin");
        LblProfitChargesAvoirsClientLabel = _locale.T("Reports_LblTotalAvoirsClient");
        LblProfitChargesPurchasesLabel = _locale.T("Reports_LblTotalPurchases");
        LblProfitChargesAvoirsFournisseurLabel = _locale.T("Reports_LblTotalAvoirsFournisseur");
        LblProfitChargesChargesLabel = _locale.T("Reports_LblTotalCharges");
        LblProfitChargesNetLabel = _locale.T("Reports_LblNetResult");
        ColProfitType = _locale.T("Reports_ColType");
        ColProfitRef = _locale.T("Reports_ColRefLibelle");
        ColProfitDate = _locale.T("DevisList_ColDate");
        ColProfitHt = _locale.T("Reports_LblTotalTtc");
        ColProfitAmount = _locale.T("Reports_ColMarginCharge");
    }

    partial void OnSelectedReportIndexChanged(int value)
    {
        ShowProfitCharges = value == 0;
        ShowSaleByProduct = value == 1;
        ShowSaleByCustomer = value == 2;
        ShowRefunds = value == 3;
        ShowDailySales = value == 4;
        ShowUnpaid = value == 5;
        ShowStockMovements = value == 6;
        ShowDateFilter = value != 5;
        LoadReportCommand.Execute(null);
    }

    partial void OnDateFromChanged(DateTimeOffset value) =>
        DatePresets.SyncSelection(value.Date, DateTo.Date);

    partial void OnDateToChanged(DateTimeOffset value) =>
        DatePresets.SyncSelection(DateFrom.Date, value.Date);

    [RelayCommand]
    private void GoProfitCharges()
    {
        if (SelectedReportIndex != 0)
            SelectedReportIndex = 0;
        else
            LoadReportCommand.Execute(null);
    }
    [RelayCommand] private void GoSaleByProduct() => SelectedReportIndex = 1;
    [RelayCommand] private void GoSaleByCustomer() => SelectedReportIndex = 2;
    [RelayCommand] private void GoRefunds() => SelectedReportIndex = 3;
    [RelayCommand] private void GoDailySales() => SelectedReportIndex = 4;
    [RelayCommand] private void GoUnpaid() => SelectedReportIndex = 5;
    [RelayCommand] private void GoStockMovements() => SelectedReportIndex = 6;

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
                    await LoadProfitChargesAsync(from, to, cancellationToken);
                    break;
                case 1:
                    await LoadSalesByProductAsync(from, to, cancellationToken);
                    break;
                case 2:
                    await LoadSalesByCustomerAsync(from, to, cancellationToken);
                    break;
                case 3:
                    await LoadRefundsAsync(from, to, cancellationToken);
                    break;
                case 4:
                    await LoadDailySalesAsync(from, to, cancellationToken);
                    break;
                case 5:
                    await LoadUnpaidAsync(cancellationToken);
                    break;
                case 6:
                    await LoadStockMovementsAsync(from, to, cancellationToken);
                    break;
            }
        }
        catch (Exception ex)
        {
            AppLog.Error("Échec du chargement du rapport", ex, "ReportsListViewModel.LoadReportAsync");
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

    private async Task LoadProfitChargesAsync(DateTime from, DateTime to, CancellationToken ct)
    {
        var result = await Task.Run(() => _reportService.GetProfitChargesAsync(from, to, ct), ct);
        _allProfitCharges = result.Rows;
        var dev = result.Devise;
        LblProfitChargesTotalMargin = $"+{result.TotalSalesMargin:N2} {dev}";
        LblProfitChargesTotalAvoirsClient = $"-{result.TotalAvoirsClient:N2} {dev}";
        LblProfitChargesTotalPurchases = $"-{result.TotalPurchases:N2} {dev}";
        LblProfitChargesTotalAvoirsFournisseur = $"+{result.TotalAvoirsFournisseur:N2} {dev}";
        LblProfitChargesTotalCharges = $"-{result.TotalCharges:N2} {dev}";
        var netSign = result.NetResult >= 0 ? "+" : "";
        LblProfitChargesNetResult = $"{netSign}{result.NetResult:N2} {dev}";
        IsNetPositive = result.NetResult >= 0;
        ApplyProfitFilter(_profitFilterKind);
    }

    [RelayCommand]
    private void FilterProfitMargin() => ToggleProfitFilter(ReportProfitChargeKind.SaleMargin);

    [RelayCommand]
    private void FilterProfitAvoirsClient() => ToggleProfitFilter(ReportProfitChargeKind.AvoirClient);

    [RelayCommand]
    private void FilterProfitPurchases() => ToggleProfitFilter(ReportProfitChargeKind.Purchase);

    [RelayCommand]
    private void FilterProfitAvoirsFournisseur() => ToggleProfitFilter(ReportProfitChargeKind.AvoirFournisseur);

    [RelayCommand]
    private void FilterProfitCharges() => ToggleProfitFilter(ReportProfitChargeKind.Charge);

    [RelayCommand]
    private void FilterProfitAll() => ToggleProfitFilter(null);

    private void ToggleProfitFilter(ReportProfitChargeKind? kind)
    {
        if (_profitFilterKind == kind)
            kind = null; // click again clears filter
        ApplyProfitFilter(kind);
    }

    private void ApplyProfitFilter(ReportProfitChargeKind? kind)
    {
        _profitFilterKind = kind;
        IsProfitFilterMarginActive = kind == ReportProfitChargeKind.SaleMargin;
        IsProfitFilterAvoirsClientActive = kind == ReportProfitChargeKind.AvoirClient;
        IsProfitFilterPurchasesActive = kind == ReportProfitChargeKind.Purchase;
        IsProfitFilterAvoirsFournisseurActive = kind == ReportProfitChargeKind.AvoirFournisseur;
        IsProfitFilterChargesActive = kind == ReportProfitChargeKind.Charge;
        IsProfitFilterAllActive = kind == null;

        _filteredProfitCharges = kind == null
            ? _allProfitCharges
            : _allProfitCharges.Where(r => r.Kind == kind).ToList();

        FinishPagedLoad(_filteredProfitCharges.Count);
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
                ApplyPage(ProfitCharges, _filteredProfitCharges);
                break;
            case 1:
                ApplyPage(SalesByProduct, _allSalesByProduct);
                break;
            case 2:
                ApplyPage(SalesByCustomer, _allSalesByCustomer);
                break;
            case 3:
                ApplyPage(Refunds, _allRefunds);
                break;
            case 4:
                ApplyPage(DailySales, _allDailySales);
                break;
            case 5:
                ApplyPage(UnpaidSales, _allUnpaidSales);
                break;
            case 6:
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
