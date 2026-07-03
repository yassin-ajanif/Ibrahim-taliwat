using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionCommerciale.Modules.Auth.Services;
using GestionCommerciale.Modules.Facturation.Models;
using GestionCommerciale.Modules.Stock.Services;
using GestionCommerciale.Shared.Database;
using GestionCommerciale.Shared.Helpers;
using GestionCommerciale.Shared.Models.Pdf;
using GestionCommerciale.Shared.Services;
using GestionCommerciale.Shared.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GestionCommerciale.Modules.Facturation.ViewModels;

public partial class AvoirListViewModel : BaseViewModel
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly WorkspaceNavigator _workspace;
    private readonly IServiceProvider _sp;
    private readonly IDialogService _dialog;
    private readonly IPdfService _pdf;
    private readonly ILocaleService _locale;
    private readonly ICurrentUserSession _session;
    private readonly IStockMovementService _stock;

    public AvoirListViewModel(
        IDbContextFactory<AppDbContext> dbFactory,
        WorkspaceNavigator workspaceNavigator,
        IServiceProvider sp,
        IDialogService dialog,
        IPdfService pdf,
        ILocaleService locale,
        ICurrentUserSession session,
        IStockMovementService stock)
    {
        _dbFactory = dbFactory;
        _workspace = workspaceNavigator;
        _sp = sp;
        _dialog = dialog;
        _pdf = pdf;
        _locale = locale;
        _session = session;
        _stock = stock;
        _locale.CultureApplied += (_, _) =>
        {
            RefreshListToolbar();
            _ = LoadPageAsync(CancellationToken.None, true);
        };
        RefreshListToolbar();
        Title = _locale.T("AvoirList_Title");
        Pagination = new PaginationHelper(() => _ = LoadPageAsync(CancellationToken.None));
    }

    [ObservableProperty] private string _btnNew = string.Empty;
    [ObservableProperty] private string _btnPdf = string.Empty;
    [ObservableProperty] private string _btnFilterDate = string.Empty;
    [ObservableProperty] private string _searchText = string.Empty;
    private DateTime? _dateFrom;
    private DateTime? _dateTo;
    [ObservableProperty] private string _wmSearch = string.Empty;
    [ObservableProperty] private string _colNumero = string.Empty;
    [ObservableProperty] private string _colClient = string.Empty;
    [ObservableProperty] private string _colDate = string.Empty;
    [ObservableProperty] private string _colFacture = string.Empty;
    [ObservableProperty] private string _colMotif = string.Empty;
    [ObservableProperty] private string _colHt = string.Empty;
    [ObservableProperty] private string _colTtc = string.Empty;
    [ObservableProperty] private string _menuDeleteAvoir = string.Empty;

    public PaginationHelper Pagination { get; }

    private void RefreshListToolbar()
    {
        BtnNew = _locale.T("Btn_NewAvoir");
        BtnPdf = _locale.T("Btn_Pdf");
        UpdateBtnFilterDateText();
        WmSearch = _locale.T("Wm_SearchAvoirList");
        // Reuse existing document/list header keys so the UI shows real labels (not missing key strings).
        ColNumero = _locale.T("DevisList_ColRef");
        ColClient = _locale.T("Lbl_Client");
        ColDate = _locale.T("DevisList_ColDate");
        ColFacture = _locale.T("DocList_ColFacture");
        ColMotif = _locale.T("Lbl_Motif");
        ColHt = _locale.T("DevisList_ColHt");
        ColTtc = _locale.T("DevisList_ColTtc");
        MenuDeleteAvoir = _locale.T("Avoir_MenuDelete");
    }

    partial void OnSearchTextChanged(string value) => _ = LoadPageAsync(CancellationToken.None, true);

    public ObservableCollection<AvoirListRow> Items { get; } = [];
    [ObservableProperty] private AvoirListRow? _selected;

    [RelayCommand]
    private Task LoadAsync(CancellationToken cancellationToken) => LoadPageAsync(cancellationToken, true);

    private async Task LoadPageAsync(CancellationToken cancellationToken, bool resetPage = false)
    {
        if (!_session.CanAccessAvoir)
        {
            Items.Clear();
            Pagination.TotalCount = 0;
            return;
        }

        IsBusy = true;
        try
        {
            if (resetPage)
                Pagination.CurrentPage = 1;

            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var cfg = await db.AppSettings.AsNoTracking().FirstAsync(cancellationToken);
            var devise = string.IsNullOrWhiteSpace(cfg.Devise) ? "MAD" : cfg.Devise.Trim();

            var joined = from a in db.Avoirs.AsNoTracking().Include(a => a.Lignes)
                         join t in db.Tiers.AsNoTracking() on a.ClientId equals t.Id into tj
                         from t in tj.DefaultIfEmpty()
                         join f in db.Factures.AsNoTracking() on a.FactureId equals f.Id into fj
                         from f in fj.DefaultIfEmpty()
                         select new { a, nom = t != null ? t.Nom : string.Empty, factNum = f != null ? f.Numero : string.Empty };

            var joinedQ = joined.AsQueryable();
            if (_dateFrom.HasValue)
                joinedQ = joinedQ.Where(x => x.a.Date >= _dateFrom.Value);
            if (_dateTo.HasValue)
                joinedQ = joinedQ.Where(x => x.a.Date <= _dateTo.Value);

            var search = SearchText?.Trim();
            if (!string.IsNullOrEmpty(search))
            {
                joinedQ = joinedQ.Where(x =>
                    EF.Functions.Like(x.a.Numero, $"%{search}%")
                    || EF.Functions.Like(x.nom, $"%{search}%")
                    || EF.Functions.Like(x.factNum, $"%{search}%")
                    || EF.Functions.Like(x.a.Motif ?? string.Empty, $"%{search}%"));
            }

            var total = await joinedQ.CountAsync(cancellationToken);
            var rows = await joinedQ
                .OrderByDescending(x => x.a.Date)
                .Skip(Pagination.Skip)
                .Take(Pagination.PageSize)
                .Select(x => new { x.a, x.nom, x.factNum })
                .ToListAsync(cancellationToken);

            var selId = Selected?.Avoir.Id;
            Items.Clear();
            foreach (var r in rows)
                Items.Add(AvoirListRow.Create(r.a, r.nom, r.factNum, devise, _locale));
            Pagination.TotalCount = total;
            if (selId is { } id)
                Selected = Items.FirstOrDefault(i => i.Avoir.Id == id);
        }
        finally
        {
            IsBusy = false;
        }
    }

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
    private void NewAvoir()
    {
        if (!_session.CanAccessAvoir) return;
        var vm = _sp.GetRequiredService<AvoirEditViewModel>();
        vm.Load(null);
        _workspace.Open(vm);
    }

    [RelayCommand]
    private void OpenSelected()
    {
        if (Selected == null || !_session.CanAccessAvoir) return;
        var vm = _sp.GetRequiredService<AvoirEditViewModel>();
        vm.LoadExisting(Selected.Avoir.Id);
        _workspace.Open(vm);
    }

    [RelayCommand]
    private async Task ExportPdfAsync(CancellationToken cancellationToken)
    {
        if (Selected == null || !_session.CanAccessAvoir) return;
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var a = await db.Avoirs.Include(x => x.Lignes).FirstAsync(x => x.Id == Selected.Avoir.Id, cancellationToken);
            var client = await db.Tiers.AsNoTracking().FirstAsync(t => t.Id == a.ClientId, cancellationToken);
            var bytes = await _pdf.BuildAvoirPdfAsync(a, DocumentPartyPdfInfo.FromTiers(client), cancellationToken);
            var ok = await _dialog.SavePickedFileBytesAsync(_locale.T("Export_PdfPicker"), $"{a.Numero}.pdf", new[] { "*.pdf" }, bytes, cancellationToken);
            if (ok)
                await _dialog.ShowInfoAsync(_locale.T("Export_Pdf"), _locale.T("Export_Done"), cancellationToken);
        }
        catch (Exception ex)
        {
            await _dialog.ShowErrorAsync(_locale.T("Export_Pdf"), ex.Message, cancellationToken);
        }
    }

    [RelayCommand]
    private async Task DeleteAvoirAsync(AvoirListRow? row, CancellationToken cancellationToken)
    {
        if (row == null) return;
        var item = row.Avoir;

        if (!await _dialog.ConfirmAsync(_locale.T("Avoir_Title"), _locale.Tf("Avoir_ConfirmDelete", item.Numero), cancellationToken))
            return;

        IsBusy = true;
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var entity = await db.Avoirs.Include(a => a.Lignes).FirstAsync(a => a.Id == item.Id, cancellationToken);
            await _stock.SyncAvoirStockAsync(db, item.Id, entity.Numero, false, [], null, cancellationToken);
            db.Avoirs.Remove(entity);
            await db.SaveChangesAsync(cancellationToken);

            if (Selected?.Avoir.Id == item.Id)
                Selected = null;
            await LoadPageAsync(cancellationToken);
            await _dialog.ShowInfoAsync(_locale.T("Avoir_Title"), _locale.T("Avoir_Deleted"), cancellationToken);
        }
        catch (Exception ex)
        {
            await _dialog.ShowErrorAsync(_locale.T("Avoir_Title"), ex.Message, cancellationToken);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
