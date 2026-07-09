using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionCommerciale.Modules.Stock.Services;
using GestionCommerciale.Modules.Services.Models;
using GestionCommerciale.Shared.Database;
using GestionCommerciale.Shared.Services;
using GestionCommerciale.Shared.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Avalonia.Media.Imaging;
using System.IO;

namespace GestionCommerciale.Modules.Services.ViewModels;

public partial class ServiceEditViewModel : BaseViewModel
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly IDialogService _dialog;
    private readonly WorkspaceNavigator _workspace;
    private readonly IServiceProvider _sp;
    private readonly ILocaleService _locale;
    private const long MaxImageFileBytes = 25 * 1024 * 1024;
    private byte[]? _pendingImageReplacement;
    private bool _clearImageOnSave;

    private Bitmap? _ficheImagePreview;

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
    [ObservableProperty] private bool _showBackButton = true;
    [ObservableProperty] private string _lblPhoto = string.Empty;
    [ObservableProperty] private string _btnChooseImage = string.Empty;
    [ObservableProperty] private string _btnRemovePhoto = string.Empty;
    [ObservableProperty] private string _lblReference = string.Empty;
    [ObservableProperty] private string _lblDesignation = string.Empty;
    [ObservableProperty] private string _lblUnite = string.Empty;
    [ObservableProperty] private string _lblPrixVente = string.Empty;
    [ObservableProperty] private string _lblCout = string.Empty;
    [ObservableProperty] private string _lblTva = string.Empty;
    [ObservableProperty] private bool _canRemoveFicheImage;
    [ObservableProperty] private string _chkActif = string.Empty;
    [ObservableProperty] private string _lblNote = string.Empty;
    [ObservableProperty] private string _wmRefExample = string.Empty;
    [ObservableProperty] private string _wmLibelle = string.Empty;

    // Used when this view-model is embedded on the services list page.
    // In embedded mode, we refresh the list instead of navigating away.
    public Action? EmbeddedRefreshAction { get; set; }

    public bool FicheEditable => true;

    public Bitmap? FicheImagePreview
    {
        get => _ficheImagePreview;
        private set
        {
            if (!ReferenceEquals(_ficheImagePreview, value))
            {
                _ficheImagePreview?.Dispose();
                _ficheImagePreview = value;
                OnPropertyChanged();
            }
        }
    }

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
        LblPhoto = _locale.T("Lbl_ProductPhoto");
        BtnChooseImage = _locale.T("Btn_ChooseImageDots");
        BtnRemovePhoto = _locale.T("Btn_RemovePhoto");
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

    private static string SuggestDraftReference() =>
        "S-" + Guid.NewGuid().ToString("N")[..10].ToUpperInvariant();

    public void Load(int? id) => _ = LoadAsync(id, CancellationToken.None);

    private async Task LoadAsync(int? id, CancellationToken ct)
    {
        ServiceId = id;
        _pendingImageReplacement = null;
        _clearImageOnSave = false;
        FicheImagePreview = null;
        CanRemoveFicheImage = false;

        if (id == null)
        {
            Reference = SuggestDraftReference();
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
        SetFicheImagePreviewFromBytes(s.ImageData);
        UpdateTitle();
    }

    [RelayCommand]
    private void Back()
    {
        if (!ShowBackButton)
        {
            EmbeddedRefreshAction?.Invoke();
            return;
        }

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

            Service entity;
            if (ServiceId == null)
            {
                if (await db.Services.AnyAsync(s => s.Reference == refNorm, cancellationToken))
                {
                    await _dialog.ShowErrorAsync(_locale.T("Service_Title"), _locale.T("Service_ErrDuplicateRef"), cancellationToken);
                    return;
                }

                entity = new Service();
                db.Services.Add(entity);
            }
            else
            {
                var id = ServiceId.Value;
                if (await db.Services.AnyAsync(s => s.Reference == refNorm && s.Id != id, cancellationToken))
                {
                    await _dialog.ShowErrorAsync(_locale.T("Service_Title"), _locale.T("Service_ErrDuplicateRef"), cancellationToken);
                    return;
                }

                entity = await db.Services.FirstAsync(s => s.Id == id, cancellationToken);
            }

            entity.Reference = refNorm;
            entity.Designation = Designation.Trim();
            entity.Unite = string.IsNullOrWhiteSpace(Unite) ? "U" : Unite.Trim();
            entity.PrixVenteHT = PrixVenteHt;
            entity.CoutHT = CoutHt;
            entity.TauxTVA = TauxTva;
            entity.Actif = Actif;
            entity.Note = Note.Trim();

            // Image update logic:
            // - if user cleared photo => store null
            // - if user picked new photo => store bytes
            // - otherwise => keep existing bytes (update case)
            if (_clearImageOnSave)
                entity.ImageData = null;
            else if (_pendingImageReplacement != null)
                entity.ImageData = _pendingImageReplacement;

            await db.SaveChangesAsync(cancellationToken);
            ServiceId = entity.Id;
            _pendingImageReplacement = null;
            _clearImageOnSave = false;
            SetFicheImagePreviewFromBytes(entity.ImageData);
            await _dialog.ShowInfoAsync(_locale.T("Service_Title"), _locale.T("Service_Saved"), cancellationToken);
            Back();
        }
        catch (Exception ex)
        {
            await _dialog.ShowInfoAsync(_locale.T("Service_Title"), ex.Message, cancellationToken);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void SetFicheImagePreviewFromBytes(byte[]? bytes)
    {
        FicheImagePreview = null;
        CanRemoveFicheImage = false;

        if (bytes == null || bytes.Length == 0)
            return;

        try
        {
            using var ms = new MemoryStream(bytes);
            FicheImagePreview = new Bitmap(ms);
            CanRemoveFicheImage = true;
        }
        catch
        {
            // ignore broken image; keep preview empty
            FicheImagePreview = null;
            CanRemoveFicheImage = false;
        }
    }

    [RelayCommand]
    private async Task PickImageAsync(CancellationToken cancellationToken)
    {
        if (!FicheEditable)
            return;

        var path = await _dialog.PickOpenFileAsync(
            _locale.T("Prod_PickImage"),
            ["*.png", "*.jpg", "*.jpeg", "*.webp", "*.bmp"],
            cancellationToken);
        if (path == null)
            return;

        try
        {
            var info = new FileInfo(path);
            if (info.Length > MaxImageFileBytes)
            {
                await _dialog.ShowErrorAsync(_locale.T("Service_Title"), _locale.T("Prod_ErrFileSize"), cancellationToken);
                return;
            }
        }
        catch
        {
            // continue; compressor may fail if unreadable
        }

        byte[] jpeg;
        try
        {
            jpeg = await Task.Run(() => ProductImageCompressor.CompressFileToJpeg(path), cancellationToken);
        }
        catch (Exception ex)
        {
            await _dialog.ShowErrorAsync(_locale.T("Service_Title"), _locale.T("Prod_ErrImagePrefix") + ex.Message, cancellationToken);
            return;
        }

        if (jpeg.Length == 0)
        {
            await _dialog.ShowErrorAsync(_locale.T("Service_Title"), _locale.T("Prod_ErrImageEmpty"), cancellationToken);
            return;
        }

        _pendingImageReplacement = jpeg;
        _clearImageOnSave = false;
        SetFicheImagePreviewFromBytes(jpeg);
    }

    [RelayCommand]
    private void ClearFicheImage()
    {
        if (!FicheEditable)
            return;

        _pendingImageReplacement = null;
        _clearImageOnSave = true;
        FicheImagePreview = null;
        CanRemoveFicheImage = false;
    }
}
