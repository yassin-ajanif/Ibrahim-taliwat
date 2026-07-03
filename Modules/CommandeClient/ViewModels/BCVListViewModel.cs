using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionCommerciale.Modules.CommandeClient.Models;
using GestionCommerciale.Modules.Tiers.Models;
using GestionCommerciale.Shared.Database;
using GestionCommerciale.Shared.Helpers;
using GestionCommerciale.Shared.Models.Pdf;
using GestionCommerciale.Shared.Services;
using GestionCommerciale.Shared.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GestionCommerciale.Modules.CommandeClient.ViewModels;

public partial class BCVListViewModel : BaseViewModel
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly WorkspaceNavigator _workspace;
    private readonly IServiceProvider _sp;
    private readonly IDialogService _dialog;
    private readonly IPdfService _pdf;
    private readonly ILocaleService _locale;
    private readonly IAppSettingsService _settings;

    public BCVListViewModel(
        IDbContextFactory<AppDbContext> dbFactory,
        WorkspaceNavigator workspaceNavigator,
        IServiceProvider sp,
        IDialogService dialog,
        IPdfService pdf,
        ILocaleService locale,
        IAppSettingsService settings)
    {
        _dbFactory = dbFactory;
        _workspace = workspaceNavigator;
        _sp = sp;
        _dialog = dialog;
        _pdf = pdf;
        _locale = locale;
        _settings = settings;
        _locale.CultureApplied += (_, _) => RefreshListToolbar();
        RefreshListToolbar();
        Title = _locale.T("BCCList_Title");
        Pagination = new PaginationHelper(() => _ = LoadPageAsync(CancellationToken.None));
    }

    [ObservableProperty] private string _btnNew = string.Empty;
    [ObservableProperty] private string _btnPdf = string.Empty;
    [ObservableProperty] private string _btnFilterDate = string.Empty;
    [ObservableProperty] private string _menuDeleteBcc = string.Empty;
    private DateTime? _dateFrom;
    private DateTime? _dateTo;
    [ObservableProperty] private string _colHeaderRef = string.Empty;
    [ObservableProperty] private string _colHeaderParty = string.Empty;
    [ObservableProperty] private string _colHeaderDate = string.Empty;
    [ObservableProperty] private string _colHeaderTtc = string.Empty;
    [ObservableProperty] private string _colHeaderNote = string.Empty;
    [ObservableProperty] private string _searchWatermark = string.Empty;

    public PaginationHelper Pagination { get; }

    private void RefreshListToolbar()
    {
        BtnNew = _locale.T("Btn_New");
        UpdateBtnFilterDateText();
        MenuDeleteBcc = _locale.T("BCC_MenuDelete");
        ColHeaderRef = _locale.T("DevisList_ColRef");
        ColHeaderParty = _locale.T("Lbl_Client");
        ColHeaderDate = _locale.T("DevisList_ColDate");
        ColHeaderTtc = _locale.T("DevisList_ColTtc");
        ColHeaderNote = _locale.T("DevisList_ColNote");
        SearchWatermark = _locale.T("DocList_SearchPlaceholderClient");
    }

    public ObservableCollection<BCVListRow> Items { get; } = [];
    [ObservableProperty] private BCVListRow? _selected;
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
            var q = db.BonsCommandeClient.AsNoTracking().Include(b => b.Lignes).AsQueryable();
            if (_dateFrom.HasValue)
                q = q.Where(b => b.Date >= _dateFrom.Value);
            if (_dateTo.HasValue)
                q = q.Where(b => b.Date <= _dateTo.Value);

            var search = SearchText?.Trim();
            if (!string.IsNullOrEmpty(search))
                q = q.Where(bcc => EF.Functions.Like(bcc.Numero, $"%{search}%")
                    || db.Tiers.AsNoTracking().Any(t => t.Id == bcc.ClientId && EF.Functions.Like(t.Nom, $"%{search}%")));

            var total = await q.CountAsync(ct);
            var list = await q.OrderByDescending(b => b.Date)
                .Skip(Pagination.Skip).Take(Pagination.PageSize)
                .ToListAsync(ct);
            var ids = list.Select(b => b.ClientId).Distinct().ToList();
            var noms = await db.Tiers.AsNoTracking()
                .Where(t => ids.Contains(t.Id))
                .ToDictionaryAsync(t => t.Id, t => t.Nom, ct);
            var selId = Selected?.Bcc.Id;
            Items.Clear();
            foreach (var b in list)
                Items.Add(BCVListRow.Create(b, noms.GetValueOrDefault(b.ClientId) ?? string.Empty, devise, _locale));
            Pagination.TotalCount = total;
            if (selId is { } id)
                Selected = Items.FirstOrDefault(x => x.Bcc.Id == id);
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
    private void NewBcc()
    {
        var vm = _sp.GetRequiredService<BCVEditViewModel>();
        vm.Load(null);
        _workspace.Open(vm);
    }

    [RelayCommand]
    private void OpenSelected()
    {
        if (Selected == null) return;
        var vm = _sp.GetRequiredService<BCVEditViewModel>();
        vm.Load(Selected.Bcc.Id);
        _workspace.Open(vm);
    }

    [RelayCommand]
    private async Task DeleteBccAsync(BCVListRow? row, CancellationToken cancellationToken)
    {
        if (row == null) return;
        var item = row.Bcc;

        if (!await _dialog.ConfirmAsync(_locale.T("BCC_Title"), _locale.Tf("BCC_ConfirmDelete", item.Numero), cancellationToken))
            return;

        IsBusy = true;
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var blockedMsg = await BonCommandeClientDeleteReferencedMessage.BuildIfBlockedAsync(db, item.Id, _locale, cancellationToken);
            if (blockedMsg != null)
            {
                await _dialog.ShowErrorAsync(_locale.T("BCC_Title"), blockedMsg, cancellationToken);
                return;
            }

            var entity = await db.BonsCommandeClient.Include(b => b.Lignes).FirstAsync(b => b.Id == item.Id, cancellationToken);
            db.BonsCommandeClient.Remove(entity);
            await db.SaveChangesAsync(cancellationToken);
            if (Selected?.Bcc.Id == item.Id)
                Selected = null;
            Items.Remove(row);
            await _dialog.ShowInfoAsync(_locale.T("BCC_Title"), _locale.T("BCC_Deleted"), cancellationToken);
        }
        catch (Exception ex)
        {
            await _dialog.ShowErrorAsync(_locale.T("BCC_Title"), ex.Message, cancellationToken);
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
            var b = await db.BonsCommandeClient.Include(x => x.Lignes).FirstAsync(x => x.Id == Selected.Bcc.Id, cancellationToken);
            var client = await db.Tiers.AsNoTracking().FirstAsync(t => t.Id == b.ClientId, cancellationToken);
            var bytes = await _pdf.BuildBonCommandeClientPdfAsync(b, DocumentPartyPdfInfo.FromTiers(client), cancellationToken);
            var ok = await _dialog.SavePickedFileBytesAsync(_locale.T("Export_PdfPicker"), $"{b.Numero}.pdf", new[] { "*.pdf" }, bytes, cancellationToken);
            if (ok)
                await _dialog.ShowInfoAsync(_locale.T("Export_Pdf"), _locale.T("Export_Done"), cancellationToken);
        }
        catch (Exception ex)
        {
            await _dialog.ShowErrorAsync(_locale.T("Export_Pdf"), ex.Message, cancellationToken);
        }
    }
}
