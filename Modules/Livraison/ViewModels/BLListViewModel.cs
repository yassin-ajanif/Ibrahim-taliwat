using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionCommerciale.Modules.Facturation.Services;
using GestionCommerciale.Modules.Facturation.ViewModels;
using GestionCommerciale.Modules.Livraison.Models;
using GestionCommerciale.Modules.Stock.Services;
using GestionCommerciale.Shared.Helpers;
using GestionCommerciale.Shared.Database;
using GestionCommerciale.Shared.Models.Pdf;
using GestionCommerciale.Shared.Services;
using GestionCommerciale.Shared.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GestionCommerciale.Modules.Livraison.ViewModels;

public partial class BLListViewModel : BaseViewModel
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly WorkspaceNavigator _workspace;
    private readonly IServiceProvider _sp;
    private readonly IDialogService _dialog;
    private readonly IPdfService _pdf;
    private readonly ILocaleService _locale;
    private readonly IStockMovementService _stock;
    private readonly IAppSettingsService _settings;
    private readonly IFactureBlLinkService _blLinkService;

    public BLListViewModel(
        IDbContextFactory<AppDbContext> dbFactory,
        WorkspaceNavigator workspaceNavigator,
        IServiceProvider sp,
        IDialogService dialog,
        IPdfService pdf,
        ILocaleService locale,
        IStockMovementService stock,
        IAppSettingsService settings,
        IFactureBlLinkService blLinkService)
    {
        _dbFactory = dbFactory;
        _workspace = workspaceNavigator;
        _sp = sp;
        _dialog = dialog;
        _pdf = pdf;
        _locale = locale;
        _stock = stock;
        _settings = settings;
        _blLinkService = blLinkService;
        _locale.CultureApplied += (_, _) => RefreshListToolbar();
        RefreshListToolbar();
        Title = _locale.T("BLList_Title");
        Pagination = new PaginationHelper(() => _ = LoadPageAsync(CancellationToken.None));
    }

    [ObservableProperty] private string _btnNew = string.Empty;
    [ObservableProperty] private string _btnPdf = string.Empty;
    [ObservableProperty] private string _btnFilterDate = string.Empty;
    [ObservableProperty] private string _menuDeleteBl = string.Empty;
    private DateTime? _dateFrom;
    private DateTime? _dateTo;
    [ObservableProperty] private string _colHeaderRef = string.Empty;
    [ObservableProperty] private string _colHeaderParty = string.Empty;
    [ObservableProperty] private string _colHeaderDate = string.Empty;
    [ObservableProperty] private string _colHeaderTtc = string.Empty;
    [ObservableProperty] private string _colHeaderNote = string.Empty;
    [ObservableProperty] private string _colHeaderInvoiced = string.Empty;
    [ObservableProperty] private string _btnFacturerSelection = string.Empty;
    [ObservableProperty] private string _searchWatermark = string.Empty;

    public PaginationHelper Pagination { get; }

    private void RefreshListToolbar()
    {
        BtnNew = _locale.T("Btn_New");
        BtnPdf = _locale.T("Btn_Pdf");
        UpdateBtnFilterDateText();
        MenuDeleteBl = _locale.T("BL_MenuDelete");
        ColHeaderRef = _locale.T("DevisList_ColRef");
        ColHeaderParty = _locale.T("Lbl_Client");
        ColHeaderDate = _locale.T("DevisList_ColDate");
        ColHeaderTtc = _locale.T("DevisList_ColTtc");
        ColHeaderNote = _locale.T("DevisList_ColNote");
        ColHeaderInvoiced = _locale.T("BL_ColInvoiced");
        BtnFacturerSelection = _locale.T("BL_FacturerSelection");
        SearchWatermark = _locale.T("DocList_SearchPlaceholderClient");
    }

    public ObservableCollection<BLListRow> Items { get; } = [];
    [ObservableProperty] private BLListRow? _selected;
    [ObservableProperty] private string _searchText = string.Empty;

    partial void OnSearchTextChanged(string value) => _ = LoadPageAsync(CancellationToken.None, true);

    private async Task LoadPageAsync(CancellationToken ct, bool resetPage = false)
    {
        IsBusy = true;
        try
        {
            if (resetPage)
                Pagination.CurrentPage = 1;

            var cfg = await _settings.GetAsync(ct);
            var devise = string.IsNullOrWhiteSpace(cfg.Devise) ? "MAD" : cfg.Devise.Trim();
            await using var db = await _dbFactory.CreateDbContextAsync(ct);
            var q = db.BonsLivraison.AsNoTracking().Include(b => b.Lignes).AsQueryable();
            if (_dateFrom.HasValue)
                q = q.Where(b => b.Date >= _dateFrom.Value);
            if (_dateTo.HasValue)
                q = q.Where(b => b.Date <= _dateTo.Value);

            var search = SearchText?.Trim();
            if (!string.IsNullOrEmpty(search))
                q = q.Where(bl => EF.Functions.Like(bl.Numero, $"%{search}%")
                    || db.Tiers.AsNoTracking().Any(t => t.Id == bl.ClientId && EF.Functions.Like(t.Nom, $"%{search}%")));

            var total = await q.CountAsync(ct);
            var list = await q.OrderByDescending(b => b.Date)
                .Skip(Pagination.Skip).Take(Pagination.PageSize)
                .ToListAsync(ct);
            var ids = list.Select(b => b.ClientId).Distinct().ToList();
            var noms = await db.Tiers.AsNoTracking()
                .Where(t => ids.Contains(t.Id))
                .ToDictionaryAsync(t => t.Id, t => t.Nom, ct);
            var invoicedNums = await db.BonsLivraison.AsNoTracking()
                .Where(b => list.Select(x => x.Id).Contains(b.Id) && b.FactureId != null)
                .Include(b => b.Facture)
                .ToDictionaryAsync(b => b.Id, b => b.Facture!.Numero, ct);
            var selId = Selected?.Bl.Id;
            Items.Clear();
            foreach (var b in list)
            {
                var row = BLListRow.Create(b, noms.GetValueOrDefault(b.ClientId) ?? string.Empty, devise, _locale);
                if (invoicedNums.TryGetValue(b.Id, out var factNum))
                    row.InvoicedLabel = factNum;
                Items.Add(row);
            }
            Pagination.TotalCount = total;
            if (selId is { } id)
                Selected = Items.FirstOrDefault(x => x.Bl.Id == id);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private Task LoadAsync(CancellationToken ct) => LoadPageAsync(ct, true);

    private void UpdateBtnFilterDateText()
    {
        if (_dateFrom.HasValue && _dateTo.HasValue)
            BtnFilterDate = $"{_dateFrom:dd/MM/yy} — {_dateTo:dd/MM/yy}";
        else
            BtnFilterDate = _locale.T("Btn_FilterDate");
    }

    [RelayCommand]
    private async Task FilterDateAsync(CancellationToken cancellationToken)
    {
        var range = await _dialog.PickDateRangeAsync(_locale.T("Btn_FilterDate"), cancellationToken);
        if (range == null) return;
        if (range.Value.from == DateTime.MinValue && range.Value.to == DateTime.MinValue)
        {
            _dateFrom = null;
            _dateTo = null;
        }
        else
        {
            _dateFrom = range.Value.from;
            _dateTo = range.Value.to;
        }
        UpdateBtnFilterDateText();
        await LoadAsync(cancellationToken);
    }

    [RelayCommand]
    private void NewBl()
    {
        var vm = _sp.GetRequiredService<BLEditViewModel>();
        vm.Load(null);
        _workspace.Open(vm);
    }

    [RelayCommand]
    private void OpenSelected()
    {
        if (Selected == null) return;
        var vm = _sp.GetRequiredService<BLEditViewModel>();
        vm.Load(Selected.Bl.Id);
        _workspace.Open(vm);
    }

    [RelayCommand]
    private async Task DeleteBlAsync(BLListRow? row, CancellationToken cancellationToken)
    {
        if (row == null) return;
        var item = row.Bl;

        if (!await _dialog.ConfirmAsync(_locale.T("BL_DlgShort"), _locale.Tf("BL_ConfirmDelete", item.Numero), cancellationToken))
            return;

        IsBusy = true;
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var blockedMsg = await BonLivraisonDeleteReferencedMessage.BuildIfBlockedAsync(db, item.Id, _locale, cancellationToken);
            if (blockedMsg != null)
            {
                await _dialog.ShowErrorAsync(_locale.T("BL_DlgShort"), blockedMsg, cancellationToken);
                return;
            }

            var entity = await db.BonsLivraison.Include(b => b.Lignes).FirstAsync(b => b.Id == item.Id, cancellationToken);
            await _stock.ResyncBonLivraisonStockAsync(db, entity.Id, entity.Numero, Enumerable.Empty<(int ProduitId, decimal QuantiteLivree)>(), null, cancellationToken);
            db.BonsLivraison.Remove(entity);
            await db.SaveChangesAsync(cancellationToken);
            if (Selected?.Bl.Id == item.Id)
                Selected = null;
            Items.Remove(row);
            await _dialog.ShowInfoAsync(_locale.T("BL_DlgShort"), _locale.T("BL_Deleted"), cancellationToken);
        }
        catch (Exception ex)
        {
            await _dialog.ShowErrorAsync(_locale.T("BL_DlgShort"), ex.Message, cancellationToken);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ExportPdfAsync(CancellationToken cancellationToken)
    {
        if (Selected == null) return;
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var b = await db.BonsLivraison.Include(x => x.Lignes).FirstAsync(x => x.Id == Selected.Bl.Id, cancellationToken);
            var client = await db.Tiers.AsNoTracking().FirstAsync(t => t.Id == b.ClientId, cancellationToken);
            var bytes = await _pdf.BuildBonLivraisonPdfAsync(b, DocumentPartyPdfInfo.FromTiers(client), cancellationToken);
            var ok = await _dialog.SavePickedFileBytesAsync(_locale.T("Export_PdfPicker"), $"{b.Numero}.pdf", new[] { "*.pdf" }, bytes, cancellationToken);
            if (ok)
                await _dialog.ShowInfoAsync(_locale.T("Export_Pdf"), _locale.T("Export_Done"), cancellationToken);
        }
        catch (Exception ex)
        {
            await _dialog.ShowErrorAsync(_locale.T("Export_Pdf"), ex.Message, cancellationToken);
        }
    }

    [RelayCommand]
    private async Task FacturerSelectionAsync(CancellationToken cancellationToken)
    {
        var selected = Items.Where(r => r.IsSelected && r.CanInvoice).ToList();
        if (selected.Count == 0)
        {
            await _dialog.ShowErrorAsync(_locale.T("BL_DlgShort"), _locale.T("BL_ErrNoSelection"), cancellationToken);
            return;
        }

        var clientIds = selected.Select(r => r.Bl.ClientId).Distinct().ToList();
        if (clientIds.Count > 1)
        {
            await _dialog.ShowErrorAsync(_locale.T("BL_DlgShort"), _locale.T("BL_ErrDifferentClients"), cancellationToken);
            return;
        }

        var blIds = selected.Select(r => r.Bl.Id).ToList();
        var errors = await _blLinkService.ValidateBlsForFactureAsync(clientIds[0], blIds, cancellationToken);
        if (errors.Count > 0)
        {
            await _dialog.ShowErrorAsync(_locale.T("BL_DlgShort"), string.Join("\n", errors), cancellationToken);
            return;
        }

        var vm = _sp.GetRequiredService<FactureEditViewModel>();
        await vm.LoadFromBlsAsync(blIds, cancellationToken);
        _workspace.Open(vm);
    }
}
