using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionCommerciale.Modules.Services.Models;
using GestionCommerciale.Shared.Database;
using GestionCommerciale.Shared.Services;
using GestionCommerciale.Shared.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GestionCommerciale.Modules.Services.ViewModels;

public partial class ServiceEditViewModel : BaseViewModel
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly IDialogService _dialog;
    private readonly WorkspaceNavigator _workspace;
    private readonly IServiceProvider _sp;
    private readonly ILocaleService _locale;

    public ServiceEditViewModel(
        IDbContextFactory<AppDbContext> dbFactory,
        IDialogService dialog,
        WorkspaceNavigator workspaceNavigator,
        IServiceProvider sp,
        ILocaleService locale)
    {
        _dbFactory = dbFactory;
        _dialog = dialog;
        _workspace = workspaceNavigator;
        _sp = sp;
        _locale = locale;
        _locale.CultureApplied += (_, _) => RefreshUi();
        RefreshUi();
    }

    [ObservableProperty] private int? _serviceId;
    [ObservableProperty] private string _reference = string.Empty;
    [ObservableProperty] private string _designation = string.Empty;
    [ObservableProperty] private string _unite = "U";
    [ObservableProperty] private decimal _prixVenteHt;
    [ObservableProperty] private decimal _coutHt;
    [ObservableProperty] private decimal _tauxTva = 20;
    [ObservableProperty] private bool _actif = true;
    [ObservableProperty] private string _note = string.Empty;

    [ObservableProperty] private string _btnBack = string.Empty;
    [ObservableProperty] private string _btnSave = string.Empty;
    [ObservableProperty] private string _lblReference = string.Empty;
    [ObservableProperty] private string _lblDesignation = string.Empty;
    [ObservableProperty] private string _lblUnite = string.Empty;
    [ObservableProperty] private string _lblPrixVente = string.Empty;
    [ObservableProperty] private string _lblCout = string.Empty;
    [ObservableProperty] private string _lblTva = string.Empty;
    [ObservableProperty] private string _chkActif = string.Empty;
    [ObservableProperty] private string _lblNote = string.Empty;
    [ObservableProperty] private string _wmRefExample = string.Empty;
    [ObservableProperty] private string _wmLibelle = string.Empty;

    private void RefreshUi()
    {
        BtnBack = _locale.T("Btn_BackList");
        BtnSave = _locale.T("Btn_Save");
        LblReference = _locale.T("Lbl_ReferenceField");
        LblDesignation = _locale.T("Lbl_DesignationField");
        LblUnite = _locale.T("Lbl_Unite");
        LblPrixVente = _locale.T("Lbl_PrixVenteHt");
        LblCout = _locale.T("Service_LblCoutHt");
        LblTva = _locale.T("Lbl_TvaPctField");
        ChkActif = _locale.T("Lbl_ProductActive");
        LblNote = _locale.T("Lbl_Note");
        WmRefExample = _locale.T("Service_WmRefExample");
        WmLibelle = _locale.T("Wm_Libelle");
        UpdateTitle();
    }

    private void UpdateTitle()
    {
        Title = ServiceId == null
            ? _locale.T("Service_NewTitle")
            : _locale.Tf("Service_TitleEdit", Designation);
    }

    partial void OnDesignationChanged(string value) => UpdateTitle();

    public void Load(int? id) => _ = LoadAsync(id, CancellationToken.None);

    private async Task LoadAsync(int? id, CancellationToken ct)
    {
        ServiceId = id;
        if (id == null)
        {
            Reference = string.Empty;
            Designation = string.Empty;
            Unite = "U";
            PrixVenteHt = 0;
            CoutHt = 0;
            TauxTva = 20;
            Actif = true;
            Note = string.Empty;
            UpdateTitle();
            return;
        }

        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        var s = await db.Services.AsNoTracking().FirstAsync(x => x.Id == id, ct);
        Reference = s.Reference;
        Designation = s.Designation;
        Unite = s.Unite;
        PrixVenteHt = s.PrixVenteHT;
        CoutHt = s.CoutHT;
        TauxTva = s.TauxTVA;
        Actif = s.Actif;
        Note = s.Note;
        UpdateTitle();
    }

    [RelayCommand]
    private void Back()
    {
        var vm = _sp.GetRequiredService<ServicesListViewModel>();
        _workspace.Open(vm);
        vm.LoadCommand.Execute(null);
    }

    [RelayCommand]
    private async Task SaveAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(Reference) || string.IsNullOrWhiteSpace(Designation))
        {
            await _dialog.ShowErrorAsync(_locale.T("Service_Title"), _locale.T("Service_ErrRequired"), cancellationToken);
            return;
        }

        IsBusy = true;
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var refNorm = Reference.Trim();
            var dup = await db.Services.AnyAsync(
                s => s.Reference == refNorm && s.Id != (ServiceId ?? 0),
                cancellationToken);
            if (dup)
            {
                await _dialog.ShowErrorAsync(_locale.T("Service_Title"), _locale.T("Service_ErrDuplicateRef"), cancellationToken);
                return;
            }

            Service entity;
            if (ServiceId == null)
            {
                entity = new Service();
                db.Services.Add(entity);
            }
            else
            {
                entity = await db.Services.FirstAsync(s => s.Id == ServiceId, cancellationToken);
            }

            entity.Reference = refNorm;
            entity.Designation = Designation.Trim();
            entity.Unite = string.IsNullOrWhiteSpace(Unite) ? "U" : Unite.Trim();
            entity.PrixVenteHT = PrixVenteHt;
            entity.CoutHT = CoutHt;
            entity.TauxTVA = TauxTva;
            entity.Actif = Actif;
            entity.Note = Note.Trim();

            await db.SaveChangesAsync(cancellationToken);
            ServiceId = entity.Id;
            await _dialog.ShowInfoAsync(_locale.T("Service_Title"), _locale.T("Service_Saved"), cancellationToken);
            Back();
        }
        finally
        {
            IsBusy = false;
        }
    }
}
