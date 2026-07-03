using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionCommerciale.Modules.AvoirFournisseur.Models;
using GestionCommerciale.Modules.Stock.Services;
using GestionCommerciale.Modules.Auth.Services;
using GestionCommerciale.Shared.Database;
using GestionCommerciale.Shared.Helpers;
using GestionCommerciale.Shared.Services;
using GestionCommerciale.Shared.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GestionCommerciale.Modules.AvoirFournisseur.ViewModels;

public partial class AvoirFournisseurListViewModel : BaseViewModel
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly IDialogService _dialog;
    private readonly WorkspaceNavigator _workspace;
    private readonly IServiceProvider _sp;
    private readonly IAppSettingsService _settings;
    private readonly ILocaleService _locale;
    private readonly IStockMovementService _stock;

    public AvoirFournisseurListViewModel(
        IDbContextFactory<AppDbContext> dbFactory,
        IDialogService dialog,
        WorkspaceNavigator workspaceNavigator,
        IServiceProvider sp,
        IAppSettingsService settings,
        ILocaleService locale,
        IStockMovementService stock)
    {
        _dbFactory = dbFactory;
        _dialog = dialog;
        _workspace = workspaceNavigator;
        _sp = sp;
        _settings = settings;
        _locale = locale;
        _stock = stock;
        Title = _locale.T("Avf_Title");
        RefreshUi();
        _locale.CultureApplied += (_, _) => RefreshUi();
        Pagination = new PaginationHelper(() => _ = LoadPageAsync(CancellationToken.None));
    }

    [ObservableProperty] private string _btnNew = string.Empty;
    [ObservableProperty] private string _btnPdf = string.Empty;
    [ObservableProperty] private string _btnFilterDate = string.Empty;
    [ObservableProperty] private string _colNumero = string.Empty;
    [ObservableProperty] private string _colFournisseur = string.Empty;
    [ObservableProperty] private string _colDate = string.Empty;
    [ObservableProperty] private string _colHt = string.Empty;
    [ObservableProperty] private string _colTtc = string.Empty;
    [ObservableProperty] private string _colMotif = string.Empty;

    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private AvoirFournisseurListRow? _selected;

    private DateTime? _dateFrom, _dateTo;

    public ObservableCollection<AvoirFournisseurListRow> Rows { get; } = [];
    public PaginationHelper Pagination { get; }

    private void RefreshUi()
    {
        BtnNew = _locale.T("Btn_New");
        BtnPdf = _locale.T("Btn_Pdf");
        BtnFilterDate = _locale.T("Btn_FilterDate");
        ColNumero = _locale.T("DevisList_ColRef");
        ColFournisseur = _locale.T("Avf_ColFournisseur");
        ColDate = _locale.T("DevisList_ColDate");
        ColHt = _locale.T("DevisList_ColHt");
        ColTtc = _locale.T("DevisList_ColTtc");
        ColMotif = _locale.T("Lbl_Motif");
    }

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
            var q = db.Set<Models.AvoirFournisseur>().AsNoTracking().Include(d => d.Lignes).AsQueryable();
            if (_dateFrom.HasValue) q = q.Where(d => d.Date >= _dateFrom.Value);
            if (_dateTo.HasValue) q = q.Where(d => d.Date <= _dateTo.Value);

            var search = SearchText?.Trim();
            if (!string.IsNullOrEmpty(search))
                q = q.Where(a => EF.Functions.Like(a.Numero, $"%{search}%")
                    || db.Tiers.AsNoTracking().Any(t => t.Id == a.FournisseurId && EF.Functions.Like(t.Nom, $"%{search}%")));

            var total = await q.CountAsync(ct);
            var docs = await q.OrderByDescending(d => d.Date)
                .Skip(Pagination.Skip).Take(Pagination.PageSize)
                .ToListAsync(ct);
            var fourIds = docs.Select(d => d.FournisseurId).Distinct().ToList();
            var fours = await db.Tiers.AsNoTracking()
                .Where(t => fourIds.Contains(t.Id))
                .ToDictionaryAsync(t => t.Id, t => t.Nom, ct);

            var selId = Selected?.Doc.Id;
            Rows.Clear();
            foreach (var d in docs)
                Rows.Add(AvoirFournisseurListRow.Create(d, fours.GetValueOrDefault(d.FournisseurId, "?"), devise, _locale));
            Pagination.TotalCount = total;
            if (selId is { } id)
                Selected = Rows.FirstOrDefault(x => x.Doc.Id == id);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private Task LoadAsync(CancellationToken ct) => LoadPageAsync(ct, true);

    [RelayCommand]
    private void New()
    {
        var vm = _sp.GetRequiredService<AvoirFournisseurEditViewModel>();
        vm.Load(null);
        _workspace.Open(vm);
    }

    [RelayCommand]
    private void OpenSelected()
    {
        if (Selected == null) return;
        var vm = _sp.GetRequiredService<AvoirFournisseurEditViewModel>();
        vm.Load(Selected.Doc.Id);
        _workspace.Open(vm);
    }

    [RelayCommand]
    private async Task DeleteAsync(CancellationToken cancellationToken)
    {
        if (Selected == null) return;
        if (!await _dialog.ConfirmAsync(_locale.T("Avf_Title"),
                _locale.Tf("Avf_ConfirmDelete", Selected.Doc.Numero), cancellationToken))
            return;

        IsBusy = true;
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var entity = await db.Set<Models.AvoirFournisseur>()
                .Include(d => d.Lignes)
                .FirstAsync(d => d.Id == Selected.Doc.Id, cancellationToken);
            await _stock.SyncAvoirFournisseurStockAsync(db, entity.Id, entity.Numero, false, [], null, cancellationToken);
            db.Remove(entity);
            await db.SaveChangesAsync(cancellationToken);
            await LoadAsync(cancellationToken);
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

    private void UpdateBtnFilterDateText()
    {
        if (_dateFrom == null || _dateTo == null)
            BtnFilterDate = _locale.T("Btn_FilterDate");
        else
            BtnFilterDate = $"{_dateFrom:dd/MM/yy} — {_dateTo:dd/MM/yy}";
    }
}
