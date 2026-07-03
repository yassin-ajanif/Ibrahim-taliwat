using System.Collections.ObjectModel;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionCommerciale.Modules.Facturation.Models;
using GestionCommerciale.Modules.Pos.Models;
using GestionCommerciale.Modules.Stock.Services;
using GestionCommerciale.Modules.Pos.Services;
using GestionCommerciale.Modules.Stock.Models;
using GestionCommerciale.Shared.Database;
using GestionCommerciale.Shared.Helpers;
using GestionCommerciale.Shared.Services;
using GestionCommerciale.Shared.ViewModels;
using Microsoft.EntityFrameworkCore;
using TiersEntity = GestionCommerciale.Modules.Tiers.Models.Tiers;

namespace GestionCommerciale.Modules.Pos.ViewModels;

public partial class PosViewModel : BaseViewModel
{
    private readonly IPosService _posService;
    private readonly ILocaleService _locale;
    private readonly IDialogService _dialog;
    private readonly IAppSettingsService _settings;
    private readonly WorkspaceNavigator _workspace;
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly IDocumentNumberService _numbers;
    private readonly IStockMovementService _stock;

    public PosViewModel(
        IPosService posService,
        ILocaleService locale,
        IDialogService dialog,
        IAppSettingsService settings,
        WorkspaceNavigator workspaceNavigator,
        IDbContextFactory<AppDbContext> dbFactory,
        IDocumentNumberService numbers,
        IStockMovementService stock)
    {
        _posService = posService;
        _locale = locale;
        _dialog = dialog;
        _settings = settings;
        _workspace = workspaceNavigator;
        _dbFactory = dbFactory;
        _numbers = numbers;
        _stock = stock;
        Title = _locale.T("Nav_Pos");
        _locale.CultureApplied += (_, _) =>
        {
            Title = _locale.T("Nav_Pos");
            OnPropertyChanged(nameof(SearchWatermark));
            OnPropertyChanged(nameof(CartTitle));
            OnPropertyChanged(nameof(TotalLabel));
            OnPropertyChanged(nameof(BtnClearCart));
            OnPropertyChanged(nameof(BtnCheckout));
            OnPropertyChanged(nameof(WmClientSearch));
            OnPropertyChanged(nameof(BtnDiscounts));
            OnPropertyChanged(nameof(LabelRemiseGlobale));
            OnPropertyChanged(nameof(LabelRemiseGlobaleMontant));
        };
        _ = LoadClientsAsync();
        _ = LoadSettingsAsync();
    }

    [ObservableProperty] private bool _showKeyboard;
    [ObservableProperty] private bool _showDiscounts;

    private async Task LoadSettingsAsync()
    {
        var cfg = await _settings.GetAsync();
        ShowKeyboard = cfg.EnableVirtualKeyboard;
    }

    public ObservableCollection<ProductSearchRow> SearchResults { get; } = [];
    public ObservableCollection<CartLineRow> Cart { get; } = [];
    public ObservableCollection<TiersEntity> Clients { get; } = [];

    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private ProductSearchRow? _selectedProduct;
    [ObservableProperty] private TiersEntity? _selectedClient;

    public AutoCompleteFilterPredicate<object?> PartyAutocompleteFilter => PartyAutoComplete.ItemFilter;
    public string WmClientSearch => _locale.T("Wm_SearchClient");
    public decimal TotalPaiements => PaymentSplits.Sum(p => p.Montant);

    public ObservableCollection<PaymentSplitRow> PaymentSplits { get; } = [];

    private async Task LoadClientsAsync()
    {
        var clients = await _posService.GetActiveClientsAsync();
        Clients.Clear();
        foreach (var c in clients)
            Clients.Add(c);
    }

    public bool HasItems => Cart.Count > 0;

    public string SearchWatermark => _locale.T("Wm_SearchProducts");
    public string CartTitle => _locale.T("Nav_Pos");
    public string TotalLabel => "Total TTC";
    public string BtnClearCart => "Vider";
    public string BtnRefund => "Rembourser";
    public string BtnCheckout => "Encaisser";
    public string BtnAddPaymentSplit => "Ajouter mode";
    public bool CanRemovePaymentSplit => PaymentSplits.Count > 1;
    public string LabelMontantRecu => "Montant reçu";
    public string LabelResteARendre => "Reste à rendre";

    [ObservableProperty] private decimal _montantRecu;
    [ObservableProperty] private decimal _remiseGlobale;
    [ObservableProperty] private decimal _remiseGlobaleMontant;

    public decimal TotalHtBrut => Cart.Sum(l => l.MontantHt);
    public decimal TotalTtcBrut => Cart.Sum(l => l.MontantTtc);
    public decimal TotalHt => AjusterRemiseGlobale(TotalHtBrut);
    public decimal TotalTtc => AjusterRemiseGlobale(TotalTtcBrut);
    public decimal ResteARendre => MontantRecu >= TotalTtc ? MontantRecu - TotalTtc : 0;
    public string LabelRemiseGlobale => _locale.T("Pos_LabelRemisePct");
    public string LabelRemiseGlobaleMontant => _locale.T("Pos_LabelRemiseMontant");
    public string BtnDiscounts => ShowDiscounts ? _locale.T("Pos_HideDiscounts") : _locale.T("Pos_ShowDiscounts");

    private decimal AjusterRemiseGlobale(decimal montant)
    {
        if (RemiseGlobale > 0)
            montant *= 1 - RemiseGlobale / 100m;
        if (RemiseGlobaleMontant > 0)
            montant -= RemiseGlobaleMontant;
        return Math.Max(montant, 0);
    }

    private void SyncPaymentSplits()
    {
        if (PaymentSplits.Count == 0)
        {
            PaymentSplits.Add(new PaymentSplitRow { Mode = ModePaiement.Especes, Montant = TotalTtc });
        }
        else if (PaymentSplits.Count == 1)
        {
            PaymentSplits[0].Montant = TotalTtc;
        }

        var allocated = PaymentSplits.Sum(p => p.Montant);
        if (PaymentSplits.Count > 1 && allocated != TotalTtc)
        {
            var diff = TotalTtc - allocated;
            PaymentSplits[^1].Montant += diff;
        }

        OnPropertyChanged(nameof(TotalPaiements));
        OnPropertyChanged(nameof(CanRemovePaymentSplit));
    }

    [RelayCommand]
    private void ToggleDiscounts()
    {
        ShowDiscounts = !ShowDiscounts;
    }

    partial void OnShowDiscountsChanged(bool value)
    {
        if (!value)
            ClearDiscounts();
        OnPropertyChanged(nameof(BtnDiscounts));
    }

    private void ClearDiscounts()
    {
        RemiseGlobale = 0;
        RemiseGlobaleMontant = 0;
        foreach (var line in Cart)
        {
            line.RemisePct = 0;
            line.RemiseMontant = 0;
        }
        NotifyTotals();
    }

    [RelayCommand]
    private void AddPaymentSplit()
    {
        PaymentSplits.Add(new PaymentSplitRow { Mode = ModePaiement.TPE, Montant = 0 });
        SyncPaymentSplits();
    }

    [RelayCommand]
    private void RemovePaymentSplit(PaymentSplitRow? row)
    {
        if (row is null || PaymentSplits.Count <= 1) return;
        PaymentSplits.Remove(row);
        SyncPaymentSplits();
    }

    [RelayCommand]
    private async Task SearchProducts()
    {
        var list = await _posService.SearchProductsAsync(SearchText);
        SearchResults.Clear();
        foreach (var p in list)
            SearchResults.Add(new ProductSearchRow(p));
    }

    [RelayCommand]
    private void AddProduct(ProductSearchRow? row)
    {
        if (row?.Product is not { } produit) return;

        var existing = Cart.FirstOrDefault(l => l.ProduitId == produit.Id);
        if (existing is not null)
        {
            existing.Quantite++;
            NotifyTotals();
            return;
        }

        var line = new CartLineRow
        {
            ProduitId = produit.Id,
            Reference = produit.Reference,
            Designation = produit.Designation,
            PrixUnitaireHt = produit.PrixVenteHT,
            TauxTva = produit.TauxTVA,
            Quantite = 1
        };
        line.PropertyChanged += OnCartLinePropertyChanged;
        Cart.Add(line);
        NotifyTotals();
    }

    private void OnCartLinePropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CartLineRow.MontantTtc))
            NotifyTotals();
    }

    [RelayCommand]
    private void RemoveProduct(CartLineRow? line)
    {
        if (line is null) return;
        line.PropertyChanged -= OnCartLinePropertyChanged;
        Cart.Remove(line);
        NotifyTotals();
    }

    [RelayCommand]
    private void IncreaseQty(CartLineRow? line)
    {
        if (line is null) return;
        line.Quantite++;
    }

    [RelayCommand]
    private void DecreaseQty(CartLineRow? line)
    {
        if (line is null) return;
        if (line.Quantite <= 1)
        {
            line.PropertyChanged -= OnCartLinePropertyChanged;
            Cart.Remove(line);
        }
        else
        {
            line.Quantite--;
        }
        NotifyTotals();
    }

    [RelayCommand]
    private void ClearCart()
    {
        foreach (var line in Cart)
            line.PropertyChanged -= OnCartLinePropertyChanged;
        Cart.Clear();
        MontantRecu = 0;
        ShowDiscounts = false;
        NotifyTotals();
    }

    [RelayCommand]
    private async Task Checkout()
    {
        if (!HasItems) return;

        var requiresNamedClient = PaymentSplits.Any(p => p.Montant > 0 && (p.Mode == ModePaiement.Credit || p.Mode == ModePaiement.Cheque));
        if (requiresNamedClient && SelectedClient is null)
        {
            await _dialog.ShowErrorAsync("POS", "Veuillez sélectionner un client pour les paiements par Crédit ou Chèque.");
            return;
        }

        var clientId = SelectedClient?.Id ?? await _posService.GetDefaultClientIdAsync();

        var cartData = Cart.Select(l => new CartLineData
        {
            ProduitId = l.ProduitId,
            Designation = l.Designation,
            Quantite = l.Quantite,
            PrixUnitaireHt = l.PrixUnitaireHt,
            TauxTva = l.TauxTva,
            Remise = l.EffectiveRemisePct
        }).ToList();

        var totalPaiements = PaymentSplits.Sum(p => p.Montant);
        if (totalPaiements > TotalTtc)
        {
            await _dialog.ShowErrorAsync("POS", "Le total des paiements ne peut pas dépasser le montant total TTC.");
            return;
        }

        var payments = PaymentSplits.Where(p => p.Montant > 0).Select(p => (p.Mode, p.Montant)).ToList();
        var facture = await _posService.CheckoutAsync(clientId, cartData, payments, RemiseGlobale);

        Cart.Clear();
        SelectedClient = null;
        PaymentSplits.Clear();
        MontantRecu = 0;
        ShowDiscounts = false;
        NotifyTotals();

        await _dialog.ShowInfoAsync("POS", $"Facture #{facture.Id} créée avec succès.", autoCloseMs: 1000);
    }

    [RelayCommand]
    private async Task RefundAsync(CancellationToken cancellationToken)
    {
        if (!HasItems) return;

        var confirm = await _dialog.ConfirmAsync(
            _locale.T("Avoir_Title"),
            $"Rembourser {Cart.Count} article(s) pour un total de {TotalTtc:N2} DH ? Cette action retournera les produits au stock.",
            cancellationToken);
        if (!confirm) return;

        IsBusy = true;
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            await using var trx = await db.Database.BeginTransactionAsync(cancellationToken);

            var clientId = SelectedClient?.Id ?? await _posService.GetDefaultClientIdAsync(cancellationToken);
            var num = await _numbers.NextAvoirAsync(cancellationToken);
            var avoir = new Avoir
            {
                Numero = num,
                ClientId = clientId,
                FactureId = null,
                Date = DateTime.Today,
                Motif = "Remboursement POS",
                RetourMarchandise = true
            };
            foreach (var l in Cart)
            {
                avoir.Lignes.Add(new AvoirLigne
                {
                    ProduitId = l.ProduitId,
                    Designation = l.Designation,
                    Quantite = l.Quantite,
                    PrixUnitaireHT = l.PrixUnitaireHt,
                    TauxTVA = l.TauxTva
                });
            }

            db.Avoirs.Add(avoir);
            await db.SaveChangesAsync(cancellationToken);
            await _stock.SyncAvoirStockAsync(
                db,
                avoir.Id,
                avoir.Numero,
                true,
                avoir.Lignes.Select(l => (l.ProduitId, l.Quantite)),
                null,
                cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            await trx.CommitAsync(cancellationToken);

            Cart.Clear();
            MontantRecu = 0;
            ShowDiscounts = false;
            NotifyTotals();

            await _dialog.ShowInfoAsync(
                _locale.T("Avoir_Title"),
                $"Avoir créé : {num}",
                cancellationToken,
                autoCloseMs: 1000);
        }
        finally
        {
            IsBusy = false;
        }
    }

    partial void OnMontantRecuChanged(decimal value)
    {
        OnPropertyChanged(nameof(ResteARendre));
    }

    partial void OnRemiseGlobaleChanged(decimal value)
    {
        OnPropertyChanged(nameof(TotalHt));
        OnPropertyChanged(nameof(TotalTtc));
        OnPropertyChanged(nameof(ResteARendre));
        SyncPaymentSplits();
    }

    partial void OnRemiseGlobaleMontantChanged(decimal value)
    {
        OnPropertyChanged(nameof(TotalHt));
        OnPropertyChanged(nameof(TotalTtc));
        OnPropertyChanged(nameof(ResteARendre));
        SyncPaymentSplits();
    }

    private void NotifyTotals()
    {
        OnPropertyChanged(nameof(HasItems));
        OnPropertyChanged(nameof(TotalHtBrut));
        OnPropertyChanged(nameof(TotalTtcBrut));
        OnPropertyChanged(nameof(TotalHt));
        OnPropertyChanged(nameof(TotalTtc));
        OnPropertyChanged(nameof(ResteARendre));
        SyncPaymentSplits();
    }
}
