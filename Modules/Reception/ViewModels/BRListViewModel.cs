using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionCommerciale.Modules.FactureFournisseur.Services;
using GestionCommerciale.Modules.FactureFournisseur.ViewModels;
using GestionCommerciale.Modules.Reception.Models;
using GestionCommerciale.Modules.Stock.Services;
using GestionCommerciale.Shared.Helpers;
using GestionCommerciale.Shared.Database;
using GestionCommerciale.Shared.Models.Pdf;
using GestionCommerciale.Shared.Services;
using GestionCommerciale.Shared.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GestionCommerciale.Modules.Reception.ViewModels;

public partial class BRListViewModel : BaseViewModel
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly WorkspaceNavigator _workspace;
    private readonly IServiceProvider _sp;
    private readonly IDialogService _dialog;
    private readonly IPdfService _pdf;
    private readonly ILocaleService _locale;
    private readonly IStockMovementService _stock;
    private readonly IAppSettingsService _settings;
    private readonly IFactureFournisseurBrLinkService _brLinkService;

    public BRListViewModel(
        IDbContextFactory<AppDbContext> dbFactory,
        WorkspaceNavigator workspaceNavigator,
        IServiceProvider sp,
        IDialogService dialog,
        IPdfService pdf,
        ILocaleService locale,
        IStockMovementService stock,
        IAppSettingsService settings,
        IFactureFournisseurBrLinkService brLinkService)
    {
        _dbFactory = dbFactory;
        _workspace = workspaceNavigator;
        _sp = sp;
        _dialog = dialog;
        _pdf = pdf;
        _locale = locale;
        _stock = stock;
        _settings = settings;
        _brLinkService = brLinkService;
        _locale.CultureApplied += (_, _) => RefreshListToolbar();
        RefreshListToolbar();
        Title = _locale.T("BRList_Title");
        Pagination = new PaginationHelper(() => _ = LoadPageAsync(CancellationToken.None));
    }

    [ObservableProperty] private string _btnNew = string.Empty;
    [ObservableProperty] private string _btnPdf = string.Empty;
    [ObservableProperty] private string _btnFilterDate = string.Empty;
    [ObservableProperty] private string _btnFacturerSelection = string.Empty;
    [ObservableProperty] private string _menuDeleteBr = string.Empty;
    private DateTime? _dateFrom;
    private DateTime? _dateTo;
    [ObservableProperty] private string _colHeaderRef = string.Empty;
    [ObservableProperty] private string _colHeaderParty = string.Empty;
    [ObservableProperty] private string _colHeaderDate = string.Empty;
    [ObservableProperty] private string _colHeaderHt = string.Empty;
    [ObservableProperty] private string _colHeaderTtc = string.Empty;
    [ObservableProperty] private string _colHeaderNote = string.Empty;
    [ObservableProperty] private string _colHeaderInvoiced = string.Empty;
    [ObservableProperty] private string _searchWatermark = string.Empty;

    public PaginationHelper Pagination { get; }

    private void RefreshListToolbar()
    {
        BtnNew = _locale.T("Btn_New");
        BtnPdf = _locale.T("Btn_Pdf");
        BtnFilterDate = _locale.T("Btn_FilterDate");
        BtnFacturerSelection = _locale.T("BR_FacturerSelection");
        UpdateBtnFilterDateText();
        MenuDeleteBr = _locale.T("BR_MenuDelete");
        ColHeaderRef = _locale.T("DevisList_ColRef");
        ColHeaderParty = _locale.T("Lbl_Supplier");
        ColHeaderDate = _locale.T("DevisList_ColDate");
        ColHeaderHt = _locale.T("DevisList_ColHt");
        ColHeaderTtc = _locale.T("DevisList_ColTtc");
        ColHeaderNote = _locale.T("DevisList_ColNote");
        ColHeaderInvoiced = _locale.T("BL_ColInvoiced");
        SearchWatermark = _locale.T("DocList_SearchPlaceholderSupplier");
    }

    public ObservableCollection<BRListRow> Items { get; } = [];
    [ObservableProperty] private BRListRow? _selected;
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
            var q = db.BonsReception.AsNoTracking().Include(b => b.Lignes).AsQueryable();
            if (_dateFrom.HasValue)
                q = q.Where(b => b.Date >= _dateFrom.Value);
            if (_dateTo.HasValue)
                q = q.Where(b => b.Date <= _dateTo.Value);

            var search = SearchText?.Trim();
            if (!string.IsNullOrEmpty(search))
                q = q.Where(b => EF.Functions.Like(b.Numero, $"%{search}%")
                    || db.Tiers.AsNoTracking().Any(t => t.Id == b.FournisseurId && EF.Functions.Like(t.Nom, $"%{search}%")));

            var total = await q.CountAsync(ct);
            var list = await q.OrderByDescending(b => b.Date)
                .Skip(Pagination.Skip).Take(Pagination.PageSize)
                .ToListAsync(ct);
            var ids = list.Select(b => b.FournisseurId).Distinct().ToList();
            var noms = await db.Tiers.AsNoTracking()
                .Where(t => ids.Contains(t.Id))
                .ToDictionaryAsync(t => t.Id, t => t.Nom, ct);
            var brIds = list.Select(b => b.Id).ToList();
            var invoicedNums = await db.BonsReception.AsNoTracking()
                .Where(b => brIds.Contains(b.Id) && b.FactureFournisseurId != null)
                .Join(db.FacturesFournisseurs.AsNoTracking(),
                    b => b.FactureFournisseurId,
                    f => f.Id,
                    (b, f) => new { b.Id, f.Numero })
                .ToDictionaryAsync(x => x.Id, x => x.Numero, ct);
            var selId = Selected?.Br.Id;
            Items.Clear();
            foreach (var b in list)
            {
                var row = BRListRow.Create(b, noms.GetValueOrDefault(b.FournisseurId) ?? string.Empty, devise, _locale);
                if (invoicedNums.TryGetValue(b.Id, out var factNum))
                    row.InvoicedLabel = factNum;
                Items.Add(row);
            }
            Pagination.TotalCount = total;
            if (selId is { } id)
                Selected = Items.FirstOrDefault(x => x.Br.Id == id);
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
    private void NewBr()
    {
        var vm = _sp.GetRequiredService<BREditViewModel>();
        vm.Load(null);
        _workspace.Open(vm);
    }

    [RelayCommand]
    private void OpenSelected()
    {
        if (Selected == null) return;
        var vm = _sp.GetRequiredService<BREditViewModel>();
        vm.Load(Selected.Br.Id);
        _workspace.Open(vm);
    }

    [RelayCommand]
    private async Task DeleteBrAsync(BRListRow? row, CancellationToken cancellationToken)
    {
        if (row == null) return;
        var item = row.Br;

        if (!await _dialog.ConfirmAsync(_locale.T("BR_DlgShort"), _locale.Tf("BR_ConfirmDelete", item.Numero), cancellationToken))
            return;

        IsBusy = true;
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var entity = await db.BonsReception.Include(b => b.Lignes).FirstAsync(b => b.Id == item.Id, cancellationToken);
            await _stock.SyncBonReceptionStockAsync(db, entity.Id, entity.Numero, [], null, cancellationToken);
            db.BonsReception.Remove(entity);
            await db.SaveChangesAsync(cancellationToken);
            if (Selected?.Br.Id == item.Id)
                Selected = null;
            Items.Remove(row);
            await _dialog.ShowInfoAsync(_locale.T("BR_DlgShort"), _locale.T("BR_Deleted"), cancellationToken);
        }
        catch (Exception ex)
        {
            await _dialog.ShowErrorAsync(_locale.T("BR_DlgShort"), ex.Message, cancellationToken);
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
            var b = await db.BonsReception.Include(x => x.Lignes).FirstAsync(x => x.Id == Selected.Br.Id, cancellationToken);
            var f = await db.Tiers.AsNoTracking().FirstAsync(t => t.Id == b.FournisseurId, cancellationToken);
            var bytes = await _pdf.BuildBonReceptionPdfAsync(b, DocumentPartyPdfInfo.FromTiers(f), cancellationToken);
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
            await _dialog.ShowErrorAsync(_locale.T("BR_DlgShort"), _locale.T("BL_ErrNoSelection"), cancellationToken);
            return;
        }

        var fournisseurIds = selected.Select(r => r.Br.FournisseurId).Distinct().ToList();
        if (fournisseurIds.Count > 1)
        {
            await _dialog.ShowErrorAsync(_locale.T("BR_DlgShort"), _locale.T("BR_ErrDifferentSuppliers"), cancellationToken);
            return;
        }

        var brIds = selected.Select(r => r.Br.Id).ToList();
        var errors = await _brLinkService.ValidateBrsForFactureFournisseurAsync(fournisseurIds[0], brIds, cancellationToken);
        if (errors.Count > 0)
        {
            await _dialog.ShowErrorAsync(_locale.T("BR_DlgShort"), string.Join("\n", errors), cancellationToken);
            return;
        }

        var vm = _sp.GetRequiredService<FactureFournisseurEditViewModel>();
        await vm.LoadFromBrsAsync(brIds, cancellationToken);
        _workspace.Open(vm);
    }
}
