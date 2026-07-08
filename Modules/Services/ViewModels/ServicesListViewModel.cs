using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionCommerciale.Modules.Services;
using GestionCommerciale.Modules.Services.Models;
using GestionCommerciale.Shared.Database;
using GestionCommerciale.Shared.Helpers;
using GestionCommerciale.Shared.Services;
using GestionCommerciale.Shared.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GestionCommerciale.Modules.Services.ViewModels;

public partial class ServicesListViewModel : BaseViewModel
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly IDialogService _dialog;
    private readonly WorkspaceNavigator _workspace;
    private readonly IServiceProvider _sp;
    private readonly IAppSettingsService _settings;
    private readonly ILocaleService _locale;

    public ServicesListViewModel(
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

        Details = _sp.GetRequiredService<ServiceEditViewModel>();
        Details.ShowBackButton = false;
        Details.EmbeddedRefreshAction = () => _ = LoadPageAsync(CancellationToken.None, false);
        Details.Load(null);
    }

    public ObservableCollection<ServicesListRow> Rows { get; } = [];
    public PaginationHelper Pagination { get; }

    [ObservableProperty] private string _btnNew = string.Empty;
    [ObservableProperty] private string _colReference = string.Empty;
    [ObservableProperty] private string _colDesignation = string.Empty;
    [ObservableProperty] private string _colUnite = string.Empty;
    [ObservableProperty] private string _colPrix = string.Empty;
    [ObservableProperty] private string _colTva = string.Empty;
    [ObservableProperty] private string _colActif = string.Empty;
    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private ServicesListRow? _selected;

    public ServiceEditViewModel Details { get; }

    private void RefreshUi()
    {
        Title = _locale.T("ServicesList_Title");
        BtnNew = _locale.T("Btn_New");
        ColReference = _locale.T("Lbl_ColRef");
        ColDesignation = _locale.T("Lbl_ColDesignation");
        ColUnite = _locale.T("Lbl_Unite");
        ColPrix = _locale.T("Lbl_PrixVenteHt");
        ColTva = _locale.T("Lbl_TvaPctField");
        ColActif = _locale.T("Lbl_Actif");
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
            var q = db.Services.AsNoTracking().AsQueryable();

            var search = SearchText?.Trim();
            if (!string.IsNullOrEmpty(search))
            {
                q = q.Where(s =>
                    EF.Functions.Like(s.Reference, $"%{search}%")
                    || EF.Functions.Like(s.Designation, $"%{search}%")
                    || EF.Functions.Like(s.Note, $"%{search}%"));
            }

            var total = await q.CountAsync(ct);
            var items = await q.SelectForListWithoutImageData()
                .OrderBy(s => s.Designation)
                .ThenBy(s => s.Reference)
                .Skip(Pagination.Skip)
                .Take(Pagination.PageSize)
                .ToListAsync(ct);

            var selId = Selected?.Service.Id;
            Rows.Clear();
            foreach (var s in items)
                Rows.Add(ServicesListRow.Create(s, devise));

            Pagination.TotalCount = total;
            if (selId is { } id)
                Selected = Rows.FirstOrDefault(x => x.Service.Id == id);
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
        Selected = null;
        Details.Load(null);
    }

    [RelayCommand]
    private void OpenSelected()
    {
        if (Selected == null) return;
        Details.Load(Selected.Service.Id);
    }

    partial void OnSelectedChanged(ServicesListRow? value)
    {
        Details.Load(value?.Service.Id);
    }

    [RelayCommand]
    private async Task DeleteAsync(CancellationToken cancellationToken)
    {
        if (Selected == null) return;
        if (!await _dialog.ConfirmAsync(
                _locale.T("Service_Title"),
                _locale.Tf("Service_ConfirmDelete", Selected.Service.Designation),
                cancellationToken))
            return;

        IsBusy = true;
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var usedOnBl = await db.BonLivraisonLignes.AnyAsync(l => l.ServiceId == Selected.Service.Id, cancellationToken);
            var usedOnFacture = await db.FactureLignes.AnyAsync(l => l.ServiceId == Selected.Service.Id, cancellationToken);
            if (usedOnBl || usedOnFacture)
            {
                await _dialog.ShowErrorAsync(_locale.T("Service_Title"), _locale.T("Service_ErrInUse"), cancellationToken);
                return;
            }

            var entity = await db.Services.FirstAsync(s => s.Id == Selected.Service.Id, cancellationToken);
            db.Services.Remove(entity);
            await db.SaveChangesAsync(cancellationToken);
            await LoadAsync(cancellationToken);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
