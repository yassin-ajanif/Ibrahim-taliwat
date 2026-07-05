using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionCommerciale.Modules.Charges.Models;
using GestionCommerciale.Shared.Database;
using GestionCommerciale.Shared.Helpers;
using GestionCommerciale.Shared.Services;
using GestionCommerciale.Shared.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GestionCommerciale.Modules.Charges.ViewModels;

public partial class ChargesListViewModel : BaseViewModel
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly IDialogService _dialog;
    private readonly WorkspaceNavigator _workspace;
    private readonly IServiceProvider _sp;
    private readonly IAppSettingsService _settings;
    private readonly ILocaleService _locale;

    private DateTime? _dateFrom;
    private DateTime? _dateTo;

    public ChargesListViewModel(
        IDbContextFactory<AppDbContext> dbFactory,
        IDialogService dialog,
        WorkspaceNavigator workspaceNavigator,
        IServiceProvider sp,
        IAppSettingsService settings,
        ILocaleService locale)
    {
        _dbFactory = dbFactory;
        _dialog = dialog;
        _workspace = workspaceNavigator;
        _sp = sp;
        _settings = settings;
        _locale = locale;
        _locale.CultureApplied += (_, _) => RefreshUi();
        RefreshUi();
        Pagination = new PaginationHelper(() => _ = LoadPageAsync(CancellationToken.None));
    }

    public ObservableCollection<ChargesListRow> Rows { get; } = [];
    public PaginationHelper Pagination { get; }

    [ObservableProperty] private string _btnNew = string.Empty;
    [ObservableProperty] private string _btnFilterDate = string.Empty;
    [ObservableProperty] private string _colType = string.Empty;
    [ObservableProperty] private string _colDate = string.Empty;
    [ObservableProperty] private string _colLibelle = string.Empty;
    [ObservableProperty] private string _colBeneficiaire = string.Empty;
    [ObservableProperty] private string _colTtc = string.Empty;
    [ObservableProperty] private string _colNote = string.Empty;
    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private ChargesListRow? _selected;

    private void RefreshUi()
    {
        Title = _locale.T("ChargesList_Title");
        BtnNew = _locale.T("Btn_New");
        BtnFilterDate = _locale.T("Btn_FilterDate");
        ColType = _locale.T("Charge_ColType");
        ColDate = _locale.T("DevisList_ColDate");
        ColLibelle = _locale.T("Charge_LblLibelle");
        ColBeneficiaire = _locale.T("Charge_ColBeneficiaire");
        ColTtc = _locale.T("DevisList_ColTtc");
        ColNote = _locale.T("DevisList_ColNote");
        UpdateBtnFilterDateText();
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
            var q = db.Charges.AsNoTracking()
                .Include(c => c.TypeCharge)
                .AsQueryable();

            if (_dateFrom.HasValue)
                q = q.Where(c => c.Date >= _dateFrom.Value);
            if (_dateTo.HasValue)
                q = q.Where(c => c.Date <= _dateTo.Value);

            var search = SearchText?.Trim();
            if (!string.IsNullOrEmpty(search))
            {
                q = q.Where(c =>
                    EF.Functions.Like(c.Libelle, $"%{search}%")
                    || EF.Functions.Like(c.BeneficiaireLibre, $"%{search}%")
                    || EF.Functions.Like(c.Note, $"%{search}%")
                    || (c.TypeCharge != null && EF.Functions.Like(c.TypeCharge.Nom, $"%{search}%"))
                    || db.Tiers.AsNoTracking().Any(t =>
                        t.Id == c.FournisseurId && EF.Functions.Like(t.Nom, $"%{search}%")));
            }

            var total = await q.CountAsync(ct);
            var charges = await q.OrderByDescending(c => c.Date)
                .ThenByDescending(c => c.Id)
                .Skip(Pagination.Skip)
                .Take(Pagination.PageSize)
                .ToListAsync(ct);

            var fourIds = charges.Where(c => c.FournisseurId.HasValue).Select(c => c.FournisseurId!.Value).Distinct().ToList();
            var fours = await db.Tiers.AsNoTracking()
                .Where(t => fourIds.Contains(t.Id))
                .ToDictionaryAsync(t => t.Id, t => t.Nom, ct);

            var selId = Selected?.Charge.Id;
            Rows.Clear();
            foreach (var c in charges)
            {
                var fourNom = c.FournisseurId is { } fid ? fours.GetValueOrDefault(fid) : null;
                Rows.Add(ChargesListRow.Create(c, c.TypeCharge?.Nom ?? "?", fourNom, devise, _locale));
            }

            Pagination.TotalCount = total;
            if (selId is { } id)
                Selected = Rows.FirstOrDefault(x => x.Charge.Id == id);
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
        var vm = _sp.GetRequiredService<ChargeEditViewModel>();
        vm.Load(null);
        _workspace.Open(vm);
    }

    [RelayCommand]
    private void OpenSelected()
    {
        if (Selected == null) return;
        var vm = _sp.GetRequiredService<ChargeEditViewModel>();
        vm.Load(Selected.Charge.Id);
        _workspace.Open(vm);
    }

    [RelayCommand]
    private async Task DeleteAsync(CancellationToken cancellationToken)
    {
        if (Selected == null) return;
        if (!await _dialog.ConfirmAsync(
                _locale.T("Charge_Title"),
                _locale.Tf("Charge_ConfirmDelete", Selected.Charge.Libelle),
                cancellationToken))
            return;

        IsBusy = true;
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var entity = await db.Charges.FirstAsync(c => c.Id == Selected.Charge.Id, cancellationToken);
            db.Charges.Remove(entity);
            await db.SaveChangesAsync(cancellationToken);
            await LoadAsync(cancellationToken);
        }
        finally
        {
            IsBusy = false;
        }
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
