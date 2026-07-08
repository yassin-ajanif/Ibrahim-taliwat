using System.Collections.ObjectModel;
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

public partial class StockMainViewModel : BaseViewModel
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly IStockMovementService _stock;
    private readonly IDialogService _dialog;
    private readonly ICurrentUserSession _session;
    private readonly ILocaleService _locale;

    private int _currentProduitId;

    public StockMainViewModel(
        IDbContextFactory<AppDbContext> dbFactory,
        IStockMovementService stock,
        IDialogService dialog,
        ICurrentUserSession session,
        ILocaleService locale)
    {
        _dbFactory = dbFactory;
        _stock = stock;
        _dialog = dialog;
        _session = session;
        _locale = locale;
        _locale.CultureApplied += (_, _) => RefreshStockUi();
        RefreshStockUi();
        Pagination = new PaginationHelper(() => _ = LoadProduitsAsync(CancellationToken.None));
        MouvementPagination = new PaginationHelper(() => _ = LoadMouvementsAsync(_currentProduitId, CancellationToken.None));
    }

    public PaginationHelper Pagination { get; }
    public PaginationHelper MouvementPagination { get; }

    [ObservableProperty] private string _lblCatalog = string.Empty;
    [ObservableProperty] private string _helpStock = string.Empty;
    [ObservableProperty] private string _wmSearch = string.Empty;
    [ObservableProperty] private string _colRef = string.Empty;
    [ObservableProperty] private string _colDesignation = string.Empty;
    [ObservableProperty] private string _colStock = string.Empty;
    [ObservableProperty] private string _colMinDot = string.Empty;
    [ObservableProperty] private string _lblAdjustHistory = string.Empty;
    [ObservableProperty] private string _lblAdjustManual = string.Empty;
    [ObservableProperty] private string _lblVariation = string.Empty;
    [ObservableProperty] private string _lblMotifTrace = string.Empty;
    [ObservableProperty] private string _wmAdjustNote = string.Empty;
    [ObservableProperty] private string _btnApply = string.Empty;
    [ObservableProperty] private string _lblMovements = string.Empty;
    [ObservableProperty] private string _colDate = string.Empty;
    [ObservableProperty] private string _colStockCurrent = string.Empty;
    [ObservableProperty] private string _colBeforeQty = string.Empty;
    [ObservableProperty] private string _colQty = string.Empty;
    [ObservableProperty] private string _colDetail = string.Empty;
    [ObservableProperty] private string _wmMovementClientSearch = string.Empty;

    public bool IsMovementClientSearchEnabled => SelectedProduit != null;

    private void RefreshStockUi()
    {
        Title = _locale.T("Stock_Title");
        LblCatalog = _locale.T("Lbl_Catalog");
        HelpStock = _locale.T("Lbl_StockMainHelp");
        WmSearch = _locale.T("Wm_SearchProducts");
        ColRef = _locale.T("Lbl_ColRef");
        ColDesignation = _locale.T("Lbl_ColDesignation");
        ColStock = _locale.T("Lbl_ColStock");
        ColMinDot = _locale.T("Lbl_ColMinDot");
        LblAdjustHistory = _locale.T("Lbl_AdjustHistory");
        LblAdjustManual = _locale.T("Lbl_AdjustDelta");
        LblVariation = _locale.T("Lbl_Variation");
        LblMotifTrace = _locale.T("Lbl_MotifTrace");
        WmAdjustNote = _locale.T("Wm_AdjustNote");
        BtnApply = _locale.T("Btn_Apply");
        LblMovements = _locale.T("Lbl_MovementsForProduct");
        ColDate = _locale.T("Lbl_ColDate");
        ColStockCurrent = _locale.T("Lbl_ColStockCurrent");
        ColBeforeQty = _locale.T("Lbl_ColBeforeQty");
        ColQty = _locale.T("Lbl_ColQty");
        ColDetail = _locale.T("Lbl_ColDetail");
        WmMovementClientSearch = _locale.T("Wm_SearchMovementClient");
        if (SelectedProduit != null)
            _ = LoadMouvementsAsync(SelectedProduit.Id, CancellationToken.None);
    }

    partial void OnProductSearchChanged(string value)
    {
        Pagination.CurrentPage = 1;
        _ = LoadProduitsAsync(CancellationToken.None);
    }

    public ObservableCollection<Produit> Produits { get; } = [];
    public ObservableCollection<MouvementStock> Mouvements { get; } = [];

    [ObservableProperty] private Produit? _selectedProduit;

    [ObservableProperty] private string _productSearch = string.Empty;

    [ObservableProperty] private string _movementClientSearch = string.Empty;

    [ObservableProperty] private decimal _ajustementDelta;
    [ObservableProperty] private string _ajustementNote = string.Empty;

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
                .Skip(Pagination.Skip).Take(Pagination.PageSize)
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
        Mouvements.Clear();
        MovementClientSearch = string.Empty;
        OnPropertyChanged(nameof(IsMovementClientSearchEnabled));
        if (value == null) return;
        _currentProduitId = value.Id;
        MouvementPagination.CurrentPage = 1;
        _ = LoadMouvementsAsync(value.Id, CancellationToken.None);
    }

    partial void OnMovementClientSearchChanged(string value)
    {
        if (SelectedProduit == null) return;
        MouvementPagination.CurrentPage = 1;
        _ = LoadMouvementsAsync(SelectedProduit.Id, CancellationToken.None);
    }

    private async Task LoadMouvementsAsync(int produitId, CancellationToken cancellationToken)
    {
        if (produitId == 0) return;
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        var q = db.MouvementsStock.AsNoTracking()
            .Where(m => m.ProduitId == produitId)
            .WherePartyNameMatches(db, MovementClientSearch);
        var total = await q.CountAsync(cancellationToken);
        var list = await q
            .OrderByDescending(m => m.CreatedAt)
            .Skip(MouvementPagination.Skip)
            .Take(MouvementPagination.PageSize)
            .ToListAsync(cancellationToken);
        await EnrichMovementDetailsAsync(db, list, _locale.T("Lbl_PrixHt"), cancellationToken);
        Mouvements.Clear();
        foreach (var m in list) Mouvements.Add(m);
        MouvementPagination.TotalCount = total;
    }

    private static async Task EnrichMovementDetailsAsync(
        AppDbContext db,
        IReadOnlyList<MouvementStock> movements,
        string prixHtLabel,
        CancellationToken cancellationToken)
    {
        if (movements.Count == 0) return;

        var blIds = movements
            .Where(m => m.OrigineType == StockMovementService.OrigineTypeBonLivraison && m.OrigineId.HasValue)
            .Select(m => m.OrigineId!.Value)
            .Distinct()
            .ToList();
        var brIds = movements
            .Where(m => m.OrigineType == StockMovementService.OrigineTypeBonReception && m.OrigineId.HasValue)
            .Select(m => m.OrigineId!.Value)
            .Distinct()
            .ToList();
        var avoirIds = movements
            .Where(m => m.OrigineType == StockMovementService.OrigineTypeAvoir && m.OrigineId.HasValue)
            .Select(m => m.OrigineId!.Value)
            .Distinct()
            .ToList();
        var avoirFournisseurIds = movements
            .Where(m => m.OrigineType == StockMovementService.OrigineTypeAvoirFournisseur && m.OrigineId.HasValue)
            .Select(m => m.OrigineId!.Value)
            .Distinct()
            .ToList();

        var blParties = blIds.Count == 0
            ? []
            : await db.BonsLivraison.AsNoTracking()
                .Where(b => blIds.Contains(b.Id))
                .Select(b => new { b.Id, b.ClientId })
                .ToListAsync(cancellationToken);

        var brParties = brIds.Count == 0
            ? []
            : await db.BonsReception.AsNoTracking()
                .Where(b => brIds.Contains(b.Id))
                .Select(b => new { b.Id, b.FournisseurId })
                .ToListAsync(cancellationToken);

        var avoirParties = avoirIds.Count == 0
            ? []
            : await db.Avoirs.AsNoTracking()
                .Where(a => avoirIds.Contains(a.Id))
                .Select(a => new { a.Id, a.ClientId })
                .ToListAsync(cancellationToken);

        var avoirFournisseurParties = avoirFournisseurIds.Count == 0
            ? []
            : await db.AvoirsFournisseurs.AsNoTracking()
                .Where(a => avoirFournisseurIds.Contains(a.Id))
                .Select(a => new { a.Id, a.FournisseurId })
                .ToListAsync(cancellationToken);

        var tierIds = blParties.Select(x => x.ClientId)
            .Concat(brParties.Select(x => x.FournisseurId))
            .Concat(avoirParties.Select(x => x.ClientId))
            .Concat(avoirFournisseurParties.Select(x => x.FournisseurId))
            .Distinct()
            .ToList();

        var tierNames = tierIds.Count == 0
            ? new Dictionary<int, string>()
            : await db.Tiers.AsNoTracking()
                .Where(t => tierIds.Contains(t.Id))
                .ToDictionaryAsync(t => t.Id, t => t.Nom, cancellationToken);

        var blMap = blParties.ToDictionary(x => x.Id, x => tierNames.GetValueOrDefault(x.ClientId, string.Empty));
        var brMap = brParties.ToDictionary(x => x.Id, x => tierNames.GetValueOrDefault(x.FournisseurId, string.Empty));
        var avoirMap = avoirParties.ToDictionary(x => x.Id, x => tierNames.GetValueOrDefault(x.ClientId, string.Empty));
        var avoirFournisseurMap = avoirFournisseurParties.ToDictionary(x => x.Id, x => tierNames.GetValueOrDefault(x.FournisseurId, string.Empty));

        var blPriceMap = blIds.Count == 0
            ? new Dictionary<(int, int), decimal>()
            : (await db.BonLivraisonLignes.AsNoTracking()
                .Where(l => blIds.Contains(l.BLId) && l.ProduitId != null)
                .Select(l => new { l.BLId, ProduitId = l.ProduitId!.Value, l.PrixUnitaireHT })
                .ToListAsync(cancellationToken))
                .GroupBy(l => (l.BLId, l.ProduitId))
                .ToDictionary(g => g.Key, g => g.Last().PrixUnitaireHT);

        var brPriceMap = brIds.Count == 0
            ? new Dictionary<(int, int), decimal>()
            : (await db.BonReceptionLignes.AsNoTracking()
                .Where(l => brIds.Contains(l.BRId) && l.ProduitId != null)
                .Select(l => new { l.BRId, ProduitId = l.ProduitId!.Value, l.PrixUnitaireHT })
                .ToListAsync(cancellationToken))
                .GroupBy(l => (l.BRId, l.ProduitId))
                .ToDictionary(g => g.Key, g => g.Last().PrixUnitaireHT);

        var avoirPriceMap = avoirIds.Count == 0
            ? new Dictionary<(int, int), decimal>()
            : (await db.AvoirLignes.AsNoTracking()
                .Where(l => avoirIds.Contains(l.AvoirId) && l.ProduitId != null)
                .Select(l => new { l.AvoirId, ProduitId = l.ProduitId!.Value, l.PrixUnitaireHT })
                .ToListAsync(cancellationToken))
                .GroupBy(l => (l.AvoirId, l.ProduitId))
                .ToDictionary(g => g.Key, g => g.Last().PrixUnitaireHT);

        var avoirFournisseurPriceMap = avoirFournisseurIds.Count == 0
            ? new Dictionary<(int, int), decimal>()
            : (await db.AvoirFournisseurLignes.AsNoTracking()
                .Where(l => avoirFournisseurIds.Contains(l.AvoirFournisseurId) && l.ProduitId != null)
                .Select(l => new { l.AvoirFournisseurId, ProduitId = l.ProduitId!.Value, l.PrixUnitaireHT })
                .ToListAsync(cancellationToken))
                .GroupBy(l => (l.AvoirFournisseurId, l.ProduitId))
                .ToDictionary(g => g.Key, g => g.Last().PrixUnitaireHT);

        foreach (var m in movements)
        {
            m.PartyName = m.OrigineType switch
            {
                StockMovementService.OrigineTypeBonLivraison when m.OrigineId is int blId => blMap.GetValueOrDefault(blId, string.Empty),
                StockMovementService.OrigineTypeBonReception when m.OrigineId is int brId => brMap.GetValueOrDefault(brId, string.Empty),
                StockMovementService.OrigineTypeAvoir when m.OrigineId is int avoirId => avoirMap.GetValueOrDefault(avoirId, string.Empty),
                StockMovementService.OrigineTypeAvoirFournisseur when m.OrigineId is int avfId => avoirFournisseurMap.GetValueOrDefault(avfId, string.Empty),
                _ => string.Empty
            };
            m.PartyIsSupplier = m.OrigineType is StockMovementService.OrigineTypeBonReception
                or StockMovementService.OrigineTypeAvoirFournisseur;

            decimal? price = null;
            if (m.OrigineId is int docId)
            {
                price = m.OrigineType switch
                {
                    StockMovementService.OrigineTypeBonLivraison when blPriceMap.TryGetValue((docId, m.ProduitId), out var blP) => blP,
                    StockMovementService.OrigineTypeBonReception when brPriceMap.TryGetValue((docId, m.ProduitId), out var brP) => brP,
                    StockMovementService.OrigineTypeAvoir when avoirPriceMap.TryGetValue((docId, m.ProduitId), out var avP) => avP,
                    StockMovementService.OrigineTypeAvoirFournisseur when avoirFournisseurPriceMap.TryGetValue((docId, m.ProduitId), out var avfP) => avfP,
                    _ => null
                };
            }
            m.UnitPriceDetail = price is decimal p
                ? $"{prixHtLabel} : {p.ToString("N2", System.Globalization.CultureInfo.CurrentCulture)}"
                : string.Empty;
        }
    }

    [RelayCommand]
    private async Task AjustementAsync(CancellationToken cancellationToken)
    {
        if (SelectedProduit == null) return;
        if (AjustementDelta == 0)
        {
            await _dialog.ShowErrorAsync(_locale.T("Stock_Title"), _locale.T("Stock_ErrVariation"), cancellationToken);
            return;
        }

        var id = SelectedProduit.Id;
        var libInventaire = _locale.T("Stock_DefaultMotif");
        var motif = AjustementNote.Trim();
        var detailNote = string.IsNullOrEmpty(motif)
            ? libInventaire
            : $"{libInventaire} — {motif}";
        IsBusy = true;
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            await using var trx = await db.Database.BeginTransactionAsync(cancellationToken);
            await _stock.ApplyMovementAsync(
                db,
                id,
                TypeMouvement.Ajustement,
                AjustementDelta,
                libInventaire,
                null,
                detailNote,
                _session.UserId,
                cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            await trx.CommitAsync(cancellationToken);
            AjustementDelta = 0;
            AjustementNote = string.Empty;
            await LoadProduitsAsync(cancellationToken);
            if (SelectedProduit != null)
                await LoadMouvementsAsync(SelectedProduit.Id, cancellationToken);
        }
        catch (Exception ex)
        {
            await _dialog.ShowErrorAsync(_locale.T("Stock_Title"), ex.Message, cancellationToken);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
