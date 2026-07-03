using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionCommerciale.Modules.Auth.Services;
using GestionCommerciale.Modules.Stock;
using GestionCommerciale.Modules.Stock.Models;
using GestionCommerciale.Modules.Stock.Services;
using GestionCommerciale.Shared.Database;
using GestionCommerciale.Shared.Helpers;
using GestionCommerciale.Shared.Services;
using GestionCommerciale.Shared.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace GestionCommerciale.Modules.Stock.ViewModels;

public partial class ProduitsViewModel : BaseViewModel
{
    private const long MaxImageFileBytes = 25 * 1024 * 1024;

    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly IDialogService _dialog;
    private readonly ICurrentUserSession _session;
    private readonly ILocaleService _locale;
    private readonly IProductImportExportService _importExport;

    private CancellationTokenSource? _imageLoadCts;
    private byte[]? _pendingImageReplacement;
    private bool _clearImageOnSave;

    public PaginationHelper Pagination { get; }

    private Bitmap? _ficheImagePreview;

    public ProduitsViewModel(IDbContextFactory<AppDbContext> dbFactory, IDialogService dialog, ICurrentUserSession session, ILocaleService locale, IProductImportExportService importExport)
    {
        _dbFactory = dbFactory;
        _dialog = dialog;
        _session = session;
        _locale = locale;
        _importExport = importExport;
        _locale.CultureApplied += (_, _) => RefreshProduitsUi();
        Pagination = new PaginationHelper(() => _ = LoadProduitsAsync(CancellationToken.None));
        RefreshProduitsUi();
    }

    [ObservableProperty] private string _btnNewProduct = string.Empty;
    [ObservableProperty] private string _btnExportCsv = string.Empty;
    [ObservableProperty] private string _btnImportCsv = string.Empty;
    [ObservableProperty] private string _helpList = string.Empty;
    [ObservableProperty] private string _wmSearch = string.Empty;
    [ObservableProperty] private string _colRef = string.Empty;
    [ObservableProperty] private string _colDesignation = string.Empty;
    [ObservableProperty] private string _colStk = string.Empty;
    [ObservableProperty] private string _colMin = string.Empty;
    [ObservableProperty] private string _lblFicheTitle = string.Empty;
    [ObservableProperty] private string _lblDraftHint = string.Empty;
    [ObservableProperty] private string _lblReference = string.Empty;
    [ObservableProperty] private string _lblDesignation = string.Empty;
    [ObservableProperty] private string _lblBarcode = string.Empty;
    [ObservableProperty] private string _lblUnite = string.Empty;
    [ObservableProperty] private string _lblStockActuel = string.Empty;
    [ObservableProperty] private string _lblPrixAchat = string.Empty;
    [ObservableProperty] private string _lblPrixVente = string.Empty;
    [ObservableProperty] private string _lblPrixAchatTtc = string.Empty;
    [ObservableProperty] private string _lblPrixVenteTtc = string.Empty;
    [ObservableProperty] private string _lblTva = string.Empty;
    [ObservableProperty] private string _lblStockMin = string.Empty;
    [ObservableProperty] private string _lblPhoto = string.Empty;
    [ObservableProperty] private string _btnChooseImage = string.Empty;
    [ObservableProperty] private string _btnRemovePhoto = string.Empty;
    [ObservableProperty] private string _chkProductActive = string.Empty;
    [ObservableProperty] private string _btnSaveSheet = string.Empty;
    [ObservableProperty] private string _btnDeleteProduct = string.Empty;
    [ObservableProperty] private string _wmRefExample = string.Empty;
    [ObservableProperty] private string _wmLibelle = string.Empty;
    [ObservableProperty] private string _wmBarcodeExample = string.Empty;

    private void RefreshProduitsUi()
    {
        Title = _locale.T("Nav_Produits");
        BtnNewProduct = _locale.T("Btn_NewProduct");
        BtnExportCsv = _locale.T("Btn_ExportCsv");
        BtnImportCsv = _locale.T("Btn_ImportCsv");
        HelpList = _locale.T("Lbl_ProductListHelp");
        WmSearch = _locale.T("Wm_SearchProducts");
        ColRef = _locale.T("Lbl_ColRef");
        ColDesignation = _locale.T("Lbl_ColDesignation");
        ColStk = _locale.T("Lbl_ColStk");
        ColMin = _locale.T("Lbl_ColMin");
        LblFicheTitle = _locale.T("Lbl_ProductSheet");
        LblDraftHint = _locale.T("Lbl_DraftHint");
        LblReference = _locale.T("Lbl_ReferenceField");
        LblDesignation = _locale.T("Lbl_DesignationField");
        LblBarcode = _locale.T("Lbl_BarcodeField");
        LblUnite = _locale.T("Lbl_Unite");
        LblStockActuel = _locale.T("Lbl_StockActuelRo");
        LblPrixAchat = _locale.T("Lbl_PrixAchatHt");
        LblPrixVente = _locale.T("Lbl_PrixVenteHt");
        LblPrixAchatTtc = _locale.T("Lbl_PrixAchatTtc");
        LblPrixVenteTtc = _locale.T("Lbl_PrixVenteTtc");
        LblTva = _locale.T("Lbl_TvaPctField");
        LblStockMin = _locale.T("Lbl_StockMinField");
        LblPhoto = _locale.T("Lbl_ProductPhoto");
        BtnChooseImage = _locale.T("Btn_ChooseImageDots");
        BtnRemovePhoto = _locale.T("Btn_RemovePhoto");
        ChkProductActive = _locale.T("Lbl_ProductActive");
        BtnSaveSheet = _locale.T("Btn_RecordSheet");
        BtnDeleteProduct = _locale.T("Btn_DeleteProduct");
        WmRefExample = _locale.T("Wm_RefExample");
        WmLibelle = _locale.T("Wm_Libelle");
        WmBarcodeExample = _locale.T("Wm_BarcodeExample");
    }

    partial void OnProductSearchChanged(string value)
    {
        Pagination.CurrentPage = 1;
        _ = LoadProduitsAsync(CancellationToken.None);
    }

    partial void OnIsNewDraftChanged(bool value)
    {
        OnPropertyChanged(nameof(FicheEditable));
        OnPropertyChanged(nameof(CanDeleteSelectedProduit));
    }

    public ObservableCollection<Produit> Produits { get; } = [];

    [ObservableProperty] private Produit? _selectedProduit;

    /// <summary>True while creating a product: nothing is written to the DB until <see cref="SaveFicheCommand"/>.</summary>
    [ObservableProperty] private bool _isNewDraft;

    [ObservableProperty] private string _productSearch = string.Empty;

    public bool FicheEditable => SelectedProduit != null || IsNewDraft;

    public bool CanDeleteSelectedProduit => SelectedProduit != null && !IsNewDraft;

    [ObservableProperty] private string _ficheReference = string.Empty;
    [ObservableProperty] private string _ficheCodeBarre = string.Empty;
    [ObservableProperty] private string _ficheDesignation = string.Empty;
    [ObservableProperty] private string _ficheUnite = "U";
    [ObservableProperty] private decimal _fichePrixAchatHt;
    [ObservableProperty] private decimal _fichePrixVenteHt;
    [ObservableProperty] private decimal _ficheTauxTva = 20;
    [ObservableProperty] private decimal _fichePrixAchatTtc;
    [ObservableProperty] private decimal _fichePrixVenteTtc;

    private bool _syncingTtc;

    partial void OnFichePrixAchatHtChanged(decimal value)
    {
        if (!_syncingTtc)
        {
            _syncingTtc = true;
            FichePrixAchatTtc = value * (1 + FicheTauxTva / 100m);
            _syncingTtc = false;
        }
    }

    partial void OnFichePrixVenteHtChanged(decimal value)
    {
        if (!_syncingTtc)
        {
            _syncingTtc = true;
            FichePrixVenteTtc = value * (1 + FicheTauxTva / 100m);
            _syncingTtc = false;
        }
    }

    partial void OnFichePrixAchatTtcChanged(decimal value)
    {
        if (!_syncingTtc && FicheTauxTva > 0)
        {
            _syncingTtc = true;
            FichePrixAchatHt = value / (1 + FicheTauxTva / 100m);
            _syncingTtc = false;
        }
    }

    partial void OnFichePrixVenteTtcChanged(decimal value)
    {
        if (!_syncingTtc && FicheTauxTva > 0)
        {
            _syncingTtc = true;
            FichePrixVenteHt = value / (1 + FicheTauxTva / 100m);
            _syncingTtc = false;
        }
    }

    partial void OnFicheTauxTvaChanged(decimal value)
    {
        if (!_syncingTtc)
        {
            _syncingTtc = true;
            FichePrixAchatTtc = FichePrixAchatHt * (1 + value / 100m);
            FichePrixVenteTtc = FichePrixVenteHt * (1 + value / 100m);
            _syncingTtc = false;
        }
    }

    [ObservableProperty] private decimal _ficheStockMinimum;
    [ObservableProperty] private decimal _ficheStockActuel;
    [ObservableProperty] private bool _ficheActif = true;

    [ObservableProperty] private bool _ficheHasImage;
    [ObservableProperty] private bool _canRemoveFicheImage;

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

    [RelayCommand]
    private void NewProduit()
    {
        _imageLoadCts?.Cancel();
        _imageLoadCts?.Dispose();
        _imageLoadCts = new CancellationTokenSource();
        _pendingImageReplacement = null;
        _clearImageOnSave = false;
        FicheImagePreview = null;
        FicheHasImage = false;
        CanRemoveFicheImage = false;

        ApplyNewDraftDefaults();
        IsNewDraft = true;
        if (SelectedProduit != null)
            SelectedProduit = null;
        else
            OnPropertyChanged(nameof(FicheEditable));
    }

    private static string SuggestDraftReference() =>
        "P-" + Guid.NewGuid().ToString("N")[..10].ToUpperInvariant();

    private void ApplyNewDraftDefaults()
    {
        FicheReference = SuggestDraftReference();
        FicheCodeBarre = string.Empty;
        FicheDesignation = _locale.T("Prod_DraftDesignation");
        FicheUnite = "U";
        FichePrixAchatHt = 0;
        FichePrixVenteHt = 0;
        FicheTauxTva = 20;
        FicheStockMinimum = 0;
        FicheStockActuel = 0;
        FicheActif = true;
    }

    [RelayCommand]
    private async Task LoadProduitsAsync(CancellationToken cancellationToken)
    {
        var prevId = SelectedProduit?.Id;
        IsBusy = true;
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var q = db.Produits.AsNoTracking()
                .WhereSearchMatches(ProductSearch)
                .SelectForListWithoutImageData();
            var total = await q.CountAsync(cancellationToken);
            var list = await q
                .OrderBy(p => p.Reference)
                .Skip(Pagination.Skip)
                .Take(Pagination.PageSize)
                .ToListAsync(cancellationToken);
            Produits.Clear();
            foreach (var p in list) Produits.Add(p);
            Pagination.TotalCount = total;
            if (prevId.HasValue)
                SelectedProduit = Produits.FirstOrDefault(p => p.Id == prevId.Value);
        }
        finally
        {
            IsBusy = false;
        }
    }

    partial void OnSelectedProduitChanged(Produit? value)
    {
        _imageLoadCts?.Cancel();
        _imageLoadCts?.Dispose();
        _imageLoadCts = new CancellationTokenSource();
        var token = _imageLoadCts.Token;

        _pendingImageReplacement = null;
        _clearImageOnSave = false;

        if (value != null)
        {
            IsNewDraft = false;
            SyncFicheFromProduit(value);
            FicheImagePreview = null;
            FicheHasImage = false;
            CanRemoveFicheImage = false;
            _ = LoadFicheImagePreviewAsync(value.Id, token);
        }
        else
        {
            FicheImagePreview = null;
            FicheHasImage = false;
            CanRemoveFicheImage = false;
            if (!IsNewDraft)
                SyncFicheFromProduit(null);
        }

        OnPropertyChanged(nameof(FicheEditable));
        OnPropertyChanged(nameof(CanDeleteSelectedProduit));
    }

    private async Task<string?> GetProduitDeleteBlockReasonAsync(AppDbContext db, int produitId, CancellationToken cancellationToken)
    {
        if (await db.MouvementsStock.AsNoTracking().AnyAsync(m => m.ProduitId == produitId, cancellationToken))
            return _locale.T("Prod_BlockMvt");
        if (await db.DevisLignes.AsNoTracking().AnyAsync(l => l.ProduitId == produitId, cancellationToken))
            return _locale.T("Prod_BlockDevis");
        if (await db.BonLivraisonLignes.AsNoTracking().AnyAsync(l => l.ProduitId == produitId, cancellationToken))
            return _locale.T("Prod_BlockBL");
        if (await db.BonCommandeLignes.AsNoTracking().AnyAsync(l => l.ProduitId == produitId, cancellationToken))
            return _locale.T("Prod_BlockBC");
        if (await db.BonReceptionLignes.AsNoTracking().AnyAsync(l => l.ProduitId == produitId, cancellationToken))
            return _locale.T("Prod_BlockBR");
        if (await db.FactureLignes.AsNoTracking().AnyAsync(l => l.ProduitId == produitId, cancellationToken))
            return _locale.T("Prod_BlockFact");
        if (await db.AvoirLignes.AsNoTracking().AnyAsync(l => l.ProduitId == produitId, cancellationToken))
            return _locale.T("Prod_BlockAvoir");
        return null;
    }

    [RelayCommand]
    private async Task DeleteSelectedProduitAsync(CancellationToken cancellationToken)
    {
        if (SelectedProduit == null || IsNewDraft)
            return;

        var id = SelectedProduit.Id;
        var label = $"{SelectedProduit.Reference} — {SelectedProduit.Designation}";
        if (!await _dialog.ConfirmAsync(_locale.T("Prod_DeleteTitle"),
                _locale.Tf("Prod_DeleteConfirm", label), cancellationToken))
            return;

        IsBusy = true;
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var block = await GetProduitDeleteBlockReasonAsync(db, id, cancellationToken);
            if (block != null)
            {
                await _dialog.ShowErrorAsync(_locale.T("Prod_ErrDeleteTitle"), block, cancellationToken);
                return;
            }

            var entity = await db.Produits.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
            if (entity == null)
            {
                await _dialog.ShowErrorAsync(_locale.T("Nav_Produits"), _locale.T("Prod_NotFound"), cancellationToken);
                return;
            }

            db.Produits.Remove(entity);
            await db.SaveChangesAsync(cancellationToken);

            _imageLoadCts?.Cancel();
            SelectedProduit = null;
            IsNewDraft = false;
            FicheImagePreview = null;
            await LoadProduitsAsync(cancellationToken);
            await _dialog.ShowInfoAsync(_locale.T("Nav_Produits"), _locale.T("Prod_Deleted"), cancellationToken);
        }
        catch (Exception ex)
        {
            await _dialog.ShowErrorAsync(_locale.T("Nav_Produits"), ex.Message, cancellationToken);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadFicheImagePreviewAsync(int produitId, CancellationToken cancellationToken)
    {
        byte[]? bytes = null;
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            bytes = await db.Produits.AsNoTracking()
                .Where(p => p.Id == produitId)
                .Select(p => p.ImageData)
                .FirstOrDefaultAsync(cancellationToken);
        }
        catch
        {
            // ignore load errors; preview stays empty
        }

        if (cancellationToken.IsCancellationRequested)
            return;

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (cancellationToken.IsCancellationRequested)
                return;
            if (SelectedProduit?.Id != produitId)
                return;
            SetFicheImagePreviewFromBytes(bytes);
        }, DispatcherPriority.Normal, cancellationToken);
    }

    private void SetFicheImagePreviewFromBytes(byte[]? bytes)
    {
        FicheImagePreview = null;
        if (bytes == null || bytes.Length == 0)
        {
            FicheHasImage = false;
            CanRemoveFicheImage = false;
            return;
        }

        try
        {
            using var ms = new MemoryStream(bytes);
            FicheImagePreview = new Bitmap(ms);
            FicheHasImage = true;
            CanRemoveFicheImage = true;
        }
        catch
        {
            FicheHasImage = false;
            CanRemoveFicheImage = false;
        }
    }

    private void SyncFicheFromProduit(Produit? p)
    {
        if (p == null)
        {
            FicheReference = string.Empty;
            FicheCodeBarre = string.Empty;
            FicheDesignation = string.Empty;
            FicheUnite = "U";
            FichePrixAchatHt = 0;
            FichePrixVenteHt = 0;
            FicheTauxTva = 20;
            FicheStockMinimum = 0;
            FicheStockActuel = 0;
            FicheActif = true;
            return;
        }

        FicheReference = p.Reference;
        FicheCodeBarre = p.CodeBarre ?? string.Empty;
        FicheDesignation = p.Designation;
        FicheUnite = string.IsNullOrWhiteSpace(p.Unite) ? "U" : p.Unite;
        FichePrixAchatHt = p.PrixAchatHT;
        FichePrixVenteHt = p.PrixVenteHT;
        FicheTauxTva = p.TauxTVA;
        FicheStockMinimum = p.StockMinimum;
        FicheStockActuel = p.StockActuel;
        FicheActif = p.Actif;
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
                await _dialog.ShowErrorAsync(_locale.T("Nav_Produits"), _locale.T("Prod_ErrFileSize"), cancellationToken);
                return;
            }
        }
        catch
        {
            // continue; compressor will fail if unreadable
        }

        byte[] jpeg;
        try
        {
            jpeg = await Task.Run(() => ProductImageCompressor.CompressFileToJpeg(path), cancellationToken);
        }
        catch (Exception ex)
        {
            await _dialog.ShowErrorAsync(_locale.T("Nav_Produits"), _locale.T("Prod_ErrImagePrefix") + ex.Message, cancellationToken);
            return;
        }

        if (jpeg.Length == 0)
        {
            await _dialog.ShowErrorAsync(_locale.T("Nav_Produits"), _locale.T("Prod_ErrImageEmpty"), cancellationToken);
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
        FicheHasImage = false;
        CanRemoveFicheImage = false;
    }

    [RelayCommand]
    private async Task SaveFicheAsync(CancellationToken cancellationToken)
    {
        if (!FicheEditable)
            return;
        if (string.IsNullOrWhiteSpace(FicheReference) || string.IsNullOrWhiteSpace(FicheDesignation))
        {
            await _dialog.ShowErrorAsync(_locale.T("Nav_Produits"), _locale.T("Prod_ErrRefDesig"), cancellationToken);
            return;
        }

        IsBusy = true;
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var refTrim = FicheReference.Trim();
            var codeTrim = string.IsNullOrWhiteSpace(FicheCodeBarre) ? null : FicheCodeBarre.Trim();

            if (IsNewDraft)
            {
                if (await db.Produits.AsNoTracking().AnyAsync(p => p.Reference == refTrim, cancellationToken))
                {
                    await _dialog.ShowErrorAsync(_locale.T("Nav_Produits"), _locale.T("Prod_ErrDupRef"), cancellationToken);
                    return;
                }

                if (codeTrim != null &&
                    await db.Produits.AsNoTracking().AnyAsync(p => p.CodeBarre != null && p.CodeBarre == codeTrim, cancellationToken))
                {
                    await _dialog.ShowErrorAsync(_locale.T("Nav_Produits"), _locale.T("Prod_ErrDupBarcode"), cancellationToken);
                    return;
                }

                var entity = new Produit
                {
                    Reference = refTrim,
                    CodeBarre = codeTrim,
                    Designation = FicheDesignation.Trim(),
                    Unite = string.IsNullOrWhiteSpace(FicheUnite) ? "U" : FicheUnite.Trim(),
                    PrixAchatHT = FichePrixAchatHt,
                    PrixVenteHT = FichePrixVenteHt,
                    TauxTVA = FicheTauxTva,
                    StockActuel = 0,
                    StockMinimum = FicheStockMinimum,
                    Actif = FicheActif,
                    CreatedByUserId = _session.UserId,
                    ImageData = _clearImageOnSave ? null : _pendingImageReplacement,
                };
                db.Produits.Add(entity);
                await db.SaveChangesAsync(cancellationToken);
                var newId = entity.Id;
                _pendingImageReplacement = null;
                _clearImageOnSave = false;
                IsNewDraft = false;

                await _dialog.ShowInfoAsync(_locale.T("Nav_Produits"), _locale.T("Prod_Created"), cancellationToken);
                await LoadProduitsAsync(cancellationToken);
                SelectedProduit = Produits.FirstOrDefault(p => p.Id == newId);
                return;
            }

            if (SelectedProduit == null)
                return;

            var id = SelectedProduit.Id;
            var entityUpdate = await db.Produits.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
            if (entityUpdate == null)
            {
                await _dialog.ShowErrorAsync(_locale.T("Nav_Produits"), _locale.T("Prod_NotFound"), cancellationToken);
                return;
            }

            var dupRef = await db.Produits.AsNoTracking()
                .AnyAsync(p => p.Reference == refTrim && p.Id != id, cancellationToken);
            if (dupRef)
            {
                await _dialog.ShowErrorAsync(_locale.T("Nav_Produits"), _locale.T("Prod_ErrDupRef"), cancellationToken);
                return;
            }

            if (codeTrim != null)
            {
                var dupCodeUpdate = await db.Produits.AsNoTracking()
                    .AnyAsync(p => p.Id != id && p.CodeBarre != null && p.CodeBarre == codeTrim, cancellationToken);
                if (dupCodeUpdate)
                {
                    await _dialog.ShowErrorAsync(_locale.T("Nav_Produits"), _locale.T("Prod_ErrDupBarcode"), cancellationToken);
                    return;
                }
            }

            entityUpdate.Reference = refTrim;
            entityUpdate.CodeBarre = codeTrim;
            entityUpdate.Designation = FicheDesignation.Trim();
            entityUpdate.Unite = string.IsNullOrWhiteSpace(FicheUnite) ? "U" : FicheUnite.Trim();
            entityUpdate.PrixAchatHT = FichePrixAchatHt;
            entityUpdate.PrixVenteHT = FichePrixVenteHt;
            entityUpdate.TauxTVA = FicheTauxTva;
            entityUpdate.StockMinimum = FicheStockMinimum;
            entityUpdate.Actif = FicheActif;

            if (_clearImageOnSave)
                entityUpdate.ImageData = null;
            else if (_pendingImageReplacement != null)
                entityUpdate.ImageData = _pendingImageReplacement;

            await db.SaveChangesAsync(cancellationToken);
            _pendingImageReplacement = null;
            _clearImageOnSave = false;

            await _dialog.ShowInfoAsync(_locale.T("Nav_Produits"), _locale.T("Prod_Saved"), cancellationToken);
            await LoadProduitsAsync(cancellationToken);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ExportCsvAsync(CancellationToken cancellationToken)
    {
        IsBusy = true;
        try
        {
            var data = await _importExport.ExportCsvAsync(cancellationToken);
            var saved = await _dialog.SavePickedFileBytesAsync(
                _locale.T("Export_CsvPicker"),
                "produits.csv",
                new[] { "*.csv" },
                data,
                cancellationToken);
            if (saved)
                await _dialog.ShowInfoAsync(_locale.T("Nav_Produits"), _locale.T("Export_Done"), cancellationToken);
        }
        catch (Exception ex)
        {
            await _dialog.ShowErrorAsync(_locale.T("Nav_Produits"), ex.Message, cancellationToken);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ImportCsvAsync(CancellationToken cancellationToken)
    {
        var path = await _dialog.PickOpenFileAsync(
            _locale.T("Export_CsvPicker"),
            new[] { "*.csv" },
            cancellationToken);
        if (string.IsNullOrWhiteSpace(path)) return;

        IsBusy = true;
        try
        {
            var data = await System.IO.File.ReadAllBytesAsync(path, cancellationToken);
            var (imported, updated, errors) = await _importExport.ImportCsvAsync(data, cancellationToken);
            var msg = string.Format(
                System.Globalization.CultureInfo.CurrentUICulture,
                "{0} créés, {1} mis à jour, {2} erreurs.",
                imported, updated, errors);
            await _dialog.ShowInfoAsync(_locale.T("Nav_Produits"), msg, cancellationToken);
            await LoadProduitsAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            await _dialog.ShowErrorAsync(_locale.T("Nav_Produits"), ex.Message, cancellationToken);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
