using System.Collections.ObjectModel;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionCommerciale.Modules.Charges.Models;
using GestionCommerciale.Modules.Tiers.Models;
using GestionCommerciale.Shared.Database;
using GestionCommerciale.Shared.Helpers;
using GestionCommerciale.Shared.Services;
using GestionCommerciale.Shared.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TiersEntity = GestionCommerciale.Modules.Tiers.Models.Tiers;

namespace GestionCommerciale.Modules.Charges.ViewModels;

public partial class ChargeEditViewModel : BaseViewModel
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly IDialogService _dialog;
    private readonly WorkspaceNavigator _workspace;
    private readonly IServiceProvider _sp;
    private readonly ILocaleService _locale;

    public ChargeEditViewModel(
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

    public ObservableCollection<TiersEntity> Fournisseurs { get; } = [];
    public ObservableCollection<TypeCharge> TypesCharge { get; } = [];
    public ObservableCollection<TypeCharge> TypesChargeActive { get; } = [];

    public AutoCompleteFilterPredicate<object?> PartyAutocompleteFilter => PartyAutoComplete.ItemFilter;

    [ObservableProperty] private int? _chargeId;
    [ObservableProperty] private int _typeChargeId;
    [ObservableProperty] private TypeCharge? _selectedTypeCharge;
    [ObservableProperty] private DateTimeOffset _date = new(DateTime.Today);
    [ObservableProperty] private string _libelle = string.Empty;
    [ObservableProperty] private int? _fournisseurId;
    [ObservableProperty] private TiersEntity? _selectedFournisseur;
    [ObservableProperty] private string _beneficiaireLibre = string.Empty;
    [ObservableProperty] private decimal _montantTtc;
    [ObservableProperty] private string _note = string.Empty;

    [ObservableProperty] private string _newTypeNom = string.Empty;
    [ObservableProperty] private TypeCharge? _selectedTypeInPanel;

    [ObservableProperty] private string _btnBack = string.Empty;
    [ObservableProperty] private string _btnSave = string.Empty;
    [ObservableProperty] private string _lblType = string.Empty;
    [ObservableProperty] private string _lblDate = string.Empty;
    [ObservableProperty] private string _lblLibelle = string.Empty;
    [ObservableProperty] private string _lblSupplier = string.Empty;
    [ObservableProperty] private string _wmSupplierSearch = string.Empty;
    [ObservableProperty] private string _lblBeneficiaireLibre = string.Empty;
    [ObservableProperty] private string _wmBeneficiaireLibre = string.Empty;
    [ObservableProperty] private string _lblTtc = string.Empty;
    [ObservableProperty] private string _lblNote = string.Empty;
    [ObservableProperty] private string _lblTypesPanel = string.Empty;
    [ObservableProperty] private string _wmNewType = string.Empty;
    [ObservableProperty] private string _btnAddType = string.Empty;
    [ObservableProperty] private string _colTypeNom = string.Empty;
    [ObservableProperty] private string _colTypeActif = string.Empty;
    [ObservableProperty] private string _btnToggleTypeActif = string.Empty;
    [ObservableProperty] private string _menuDeleteType = string.Empty;

    private void RefreshUi()
    {
        BtnBack = _locale.T("Btn_BackList");
        BtnSave = _locale.T("Btn_Save");
        LblType = _locale.T("Charge_LblType");
        LblDate = _locale.T("Lbl_Date");
        LblLibelle = _locale.T("Charge_LblLibelle");
        LblSupplier = _locale.T("Lbl_Supplier");
        WmSupplierSearch = _locale.T("Wm_SearchSupplier");
        LblBeneficiaireLibre = _locale.T("Charge_LblBeneficiaireLibre");
        WmBeneficiaireLibre = _locale.T("Charge_WmBeneficiaireLibre");
        LblTtc = _locale.T("DevisList_ColTtc");
        LblNote = _locale.T("Lbl_Note");
        LblTypesPanel = _locale.T("Charge_LblTypesPanel");
        WmNewType = _locale.T("Charge_WmNewType");
        BtnAddType = _locale.T("Charge_BtnAddType");
        ColTypeNom = _locale.T("Charge_ColNom");
        ColTypeActif = _locale.T("Lbl_Actif");
        BtnToggleTypeActif = _locale.T("Btn_ToggleActif");
        MenuDeleteType = _locale.T("Btn_Delete");
        UpdateTitle();
    }

    partial void OnSelectedTypeChargeChanged(TypeCharge? value)
    {
        var id = value?.Id ?? 0;
        if (TypeChargeId == id) return;
        TypeChargeId = id;
    }

    partial void OnTypeChargeIdChanged(int value)
    {
        if (SelectedTypeCharge?.Id == value) return;
        SelectedTypeCharge = TypesChargeActive.FirstOrDefault(t => t.Id == value)
            ?? TypesCharge.FirstOrDefault(t => t.Id == value);
    }

    partial void OnSelectedFournisseurChanged(TiersEntity? value)
    {
        var id = value?.Id;
        if (FournisseurId == id) return;
        FournisseurId = id;
        if (value != null)
            BeneficiaireLibre = string.Empty;
    }

    partial void OnFournisseurIdChanged(int? value)
    {
        if (SelectedFournisseur?.Id == value) return;
        SelectedFournisseur = value is { } id
            ? Fournisseurs.FirstOrDefault(f => f.Id == id)
            : null;
    }

    public void Load(int? id)
    {
        ChargeId = id;
        UpdateTitle();
        _ = LoadAsync(id, CancellationToken.None);
    }

    private void UpdateTitle()
    {
        Title = ChargeId is null or 0
            ? _locale.T("Charge_NewTitle")
            : _locale.T("Charge_Title");
    }

    private async Task LoadAsync(int? id, CancellationToken cancellationToken)
    {
        IsBusy = true;
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

            var fournisseurs = await db.Tiers.AsNoTracking()
                .Where(t => t.Actif && (t.Type == TypeTiers.Fournisseur || t.Type == TypeTiers.LesDeux))
                .OrderBy(t => t.Nom)
                .ToListAsync(cancellationToken);

            Fournisseurs.Clear();
            foreach (var f in fournisseurs)
                Fournisseurs.Add(f);

            await ReloadTypesAsync(db, cancellationToken);

            if (id is null or 0)
            {
                Date = new DateTimeOffset(DateTime.Today);
                Libelle = string.Empty;
                FournisseurId = null;
                SelectedFournisseur = null;
                BeneficiaireLibre = string.Empty;
                MontantTtc = 0;
                Note = string.Empty;
                SelectedTypeCharge = TypesChargeActive.FirstOrDefault();
                return;
            }

            var entity = await db.Charges.AsNoTracking()
                .FirstAsync(c => c.Id == id, cancellationToken);

            Date = new DateTimeOffset(entity.Date);
            Libelle = entity.Libelle;
            FournisseurId = entity.FournisseurId;
            SelectedFournisseur = entity.FournisseurId is { } fid
                ? Fournisseurs.FirstOrDefault(f => f.Id == fid)
                : null;
            BeneficiaireLibre = entity.BeneficiaireLibre;
            MontantTtc = entity.MontantTtc;
            Note = entity.Note;
            SelectedTypeCharge = TypesCharge.FirstOrDefault(t => t.Id == entity.TypeChargeId);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ReloadTypesAsync(AppDbContext db, CancellationToken cancellationToken)
    {
        var types = await db.TypesCharge.AsNoTracking()
            .OrderBy(t => t.Nom)
            .ToListAsync(cancellationToken);

        TypesCharge.Clear();
        foreach (var t in types)
            TypesCharge.Add(t);

        RefreshActiveTypes();
    }

    private void RefreshActiveTypes()
    {
        TypesChargeActive.Clear();
        foreach (var t in TypesCharge.Where(x => x.Actif).OrderBy(x => x.Nom))
            TypesChargeActive.Add(t);

        if (SelectedTypeCharge != null && !SelectedTypeCharge.Actif)
            SelectedTypeCharge = TypesChargeActive.FirstOrDefault(t => t.Id == SelectedTypeCharge.Id);
    }

    [RelayCommand]
    private void Back()
    {
        var list = _sp.GetRequiredService<ChargesListViewModel>();
        _workspace.Open(list);
        list.LoadCommand.Execute(null);
    }

    [RelayCommand]
    private async Task SaveAsync(CancellationToken cancellationToken)
    {
        if (SelectedTypeCharge == null)
        {
            await _dialog.ShowErrorAsync(_locale.T("Charge_Title"), _locale.T("Charge_ErrType"), cancellationToken);
            return;
        }

        if (string.IsNullOrWhiteSpace(Libelle))
        {
            await _dialog.ShowErrorAsync(_locale.T("Charge_Title"), _locale.T("Charge_ErrLibelle"), cancellationToken);
            return;
        }

        if (SelectedFournisseur == null && string.IsNullOrWhiteSpace(BeneficiaireLibre))
        {
            await _dialog.ShowErrorAsync(_locale.T("Charge_Title"), _locale.T("Charge_ErrBeneficiaire"), cancellationToken);
            return;
        }

        if (MontantTtc < 0)
        {
            await _dialog.ShowErrorAsync(_locale.T("Charge_Title"), _locale.T("Charge_ErrMontant"), cancellationToken);
            return;
        }

        IsBusy = true;
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

            Charge entity;
            if (ChargeId is null or 0)
            {
                entity = new Charge();
                db.Charges.Add(entity);
            }
            else
            {
                entity = await db.Charges.FirstAsync(c => c.Id == ChargeId, cancellationToken);
            }

            entity.TypeChargeId = SelectedTypeCharge.Id;
            entity.Date = Date.Date;
            entity.Libelle = Libelle.Trim();
            entity.FournisseurId = SelectedFournisseur?.Id;
            entity.BeneficiaireLibre = SelectedFournisseur != null
                ? string.Empty
                : BeneficiaireLibre.Trim();
            entity.MontantTtc = MontantTtc;
            entity.Note = Note.Trim();

            await db.SaveChangesAsync(cancellationToken);
            ChargeId = entity.Id;
            UpdateTitle();
            await _dialog.ShowInfoAsync(_locale.T("Charge_Title"), _locale.T("Charge_Saved"), cancellationToken);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task AddTypeAsync(CancellationToken cancellationToken)
    {
        var nom = NewTypeNom.Trim();
        if (string.IsNullOrWhiteSpace(nom))
        {
            await _dialog.ShowErrorAsync(_locale.T("Charge_Title"), _locale.T("Charge_ErrTypeNom"), cancellationToken);
            return;
        }

        IsBusy = true;
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var exists = await db.TypesCharge.AnyAsync(t => t.Nom == nom, cancellationToken);
            if (exists)
            {
                await _dialog.ShowErrorAsync(_locale.T("Charge_Title"), _locale.T("Charge_ErrTypeExists"), cancellationToken);
                return;
            }

            var type = new TypeCharge { Nom = nom, Actif = true };
            db.TypesCharge.Add(type);
            await db.SaveChangesAsync(cancellationToken);

            NewTypeNom = string.Empty;
            await ReloadTypesAsync(db, cancellationToken);
            SelectedTypeCharge = TypesChargeActive.FirstOrDefault(t => t.Id == type.Id);
            SelectedTypeInPanel = TypesCharge.FirstOrDefault(t => t.Id == type.Id);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ToggleTypeActifAsync(TypeCharge? type, CancellationToken cancellationToken)
    {
        type ??= SelectedTypeInPanel;
        if (type == null) return;

        IsBusy = true;
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var entity = await db.TypesCharge.FirstAsync(t => t.Id == type.Id, cancellationToken);
            entity.Actif = !entity.Actif;
            await db.SaveChangesAsync(cancellationToken);

            await ReloadTypesAsync(db, cancellationToken);
            SelectedTypeInPanel = TypesCharge.FirstOrDefault(t => t.Id == type.Id);

            if (SelectedTypeCharge?.Id == type.Id && !entity.Actif)
                SelectedTypeCharge = TypesChargeActive.FirstOrDefault();
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task EditTypeAsync(CancellationToken cancellationToken)
    {
        var type = SelectedTypeInPanel;
        if (type == null) return;

        var newNom = await _dialog.ShowPromptAsync(
            _locale.T("Charge_EditTypeTitle"),
            _locale.T("Charge_EditTypePrompt"),
            type.Nom,
            cancellationToken);

        if (newNom == null) return;

        newNom = newNom.Trim();
        if (string.IsNullOrWhiteSpace(newNom))
        {
            await _dialog.ShowErrorAsync(_locale.T("Charge_Title"), _locale.T("Charge_ErrTypeNom"), cancellationToken);
            return;
        }

        if (string.Equals(newNom, type.Nom, StringComparison.Ordinal))
            return;

        IsBusy = true;
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var exists = await db.TypesCharge.AnyAsync(t => t.Nom == newNom && t.Id != type.Id, cancellationToken);
            if (exists)
            {
                await _dialog.ShowErrorAsync(_locale.T("Charge_Title"), _locale.T("Charge_ErrTypeExists"), cancellationToken);
                return;
            }

            var entity = await db.TypesCharge.FirstAsync(t => t.Id == type.Id, cancellationToken);
            entity.Nom = newNom;
            await db.SaveChangesAsync(cancellationToken);

            await ReloadTypesAsync(db, cancellationToken);
            SelectedTypeInPanel = TypesCharge.FirstOrDefault(t => t.Id == type.Id);
            if (SelectedTypeCharge?.Id == type.Id)
                SelectedTypeCharge = TypesChargeActive.FirstOrDefault(t => t.Id == type.Id)
                    ?? TypesCharge.FirstOrDefault(t => t.Id == type.Id);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task DeleteTypeAsync(TypeCharge? type, CancellationToken cancellationToken)
    {
        type ??= SelectedTypeInPanel;
        if (type == null) return;

        if (!await _dialog.ConfirmAsync(
                _locale.T("Charge_LblTypesPanel"),
                _locale.Tf("Charge_ConfirmDeleteType", type.Nom),
                cancellationToken))
            return;

        IsBusy = true;
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

            var inUse = await db.Charges.AnyAsync(c => c.TypeChargeId == type.Id, cancellationToken);
            if (inUse)
            {
                await _dialog.ShowErrorAsync(_locale.T("Charge_Title"), _locale.T("Charge_ErrTypeInUse"), cancellationToken);
                return;
            }

            var entity = await db.TypesCharge.FirstAsync(t => t.Id == type.Id, cancellationToken);
            db.TypesCharge.Remove(entity);
            await db.SaveChangesAsync(cancellationToken);

            if (SelectedTypeCharge?.Id == type.Id)
                SelectedTypeCharge = null;
            SelectedTypeInPanel = null;

            await ReloadTypesAsync(db, cancellationToken);
            SelectedTypeCharge ??= TypesChargeActive.FirstOrDefault();
        }
        finally
        {
            IsBusy = false;
        }
    }
}
