using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionCommerciale.Modules.Auth.Services;
using GestionCommerciale.Modules.CommandeClient.Models;
using GestionCommerciale.Modules.Livraison.ViewModels;
using GestionCommerciale.Shared.Database;
using GestionCommerciale.Shared.Helpers;
using GestionCommerciale.Shared.Models.Pdf;
using GestionCommerciale.Shared.Services;
using GestionCommerciale.Shared.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GestionCommerciale.Modules.CommandeClient.ViewModels;

public partial class BCVEditViewModel : BaseViewModel
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly IDocumentNumberService _numbers;
    private readonly IDialogService _dialog;
    private readonly WorkspaceNavigator _workspace;
    private readonly IServiceProvider _sp;
    private readonly ICurrentUserSession _session;
    private readonly ILocaleService _locale;
    private readonly IUiPreferencesService _uiPreferences;
    private readonly IPdfService _pdf;
    private readonly IPdfPrintService _pdfPrint;
    private readonly IAppSettingsService _settings;
    private readonly AddLineCatalogSearchCoordinator _addLineSearch;

    public BCVEditViewModel(
        IDbContextFactory<AppDbContext> dbFactory,
        IDocumentNumberService numbers,
        IDialogService dialog,
        WorkspaceNavigator workspaceNavigator,
        IServiceProvider sp,
        ICurrentUserSession session,
        ILocaleService locale,
        IUiPreferencesService uiPreferences,
        IPdfService pdf,
        IPdfPrintService pdfPrint,
        IAppSettingsService settings,
        ICatalogSearchService catalogSearch)
    {
        _dbFactory = dbFactory;
        _numbers = numbers;
        _dialog = dialog;
        _workspace = workspaceNavigator;
        _sp = sp;
        _session = session;
        _locale = locale;
        _uiPreferences = uiPreferences;
        _pdf = pdf;
        _pdfPrint = pdfPrint;
        _settings = settings;
        _addLineSearch = new AddLineCatalogSearchCoordinator(catalogSearch);
        _locale.CultureApplied += (_, _) => RefreshBccUi();
        LineGridColumns.PropertyChanged += OnLineGridColumnsPropertyChanged;
        _uiPreferences.LoadDocumentLineColumns("bon_commande_client", LineGridColumns);
        Lignes.CollectionChanged += LignesOnCollectionChanged;
        Title = _locale.T("BCC_Title");
        RefreshBccUi();
    }

    [ObservableProperty] private string _btnPdf = string.Empty;
    [ObservableProperty] private string _btnPrint = string.Empty;
    [ObservableProperty] private string _btnBack = string.Empty;
    [ObservableProperty] private string _btnSave = string.Empty;
    [ObservableProperty] private string _btnToBl = string.Empty;
    [ObservableProperty] private string _menuDeleteBcc = string.Empty;
    [ObservableProperty] private string _lblClient = string.Empty;
    [ObservableProperty] private string _wmClientSearch = string.Empty;
    [ObservableProperty] private string _lblDateBc = string.Empty;
    [ObservableProperty] private string _btnAddLine = string.Empty;
    [ObservableProperty] private string _btnRemoveLine = string.Empty;
    [ObservableProperty] private string _lblAddProduct = string.Empty;
    [ObservableProperty] private string _wmAddProduct = string.Empty;
    [ObservableProperty] private string _lblTotals = string.Empty;
    [ObservableProperty] private string _lblDocLineColumnsHint = string.Empty;
    [ObservableProperty] private string _lblDocColRef = string.Empty;
    [ObservableProperty] private string _lblDocColDesignation = string.Empty;
    [ObservableProperty] private string _lblDocColQte = string.Empty;
    [ObservableProperty] private string _lblDocColCond = string.Empty;
    [ObservableProperty] private string _wmDocLineUnite = string.Empty;
    [ObservableProperty] private string _lblDocColPuHt = string.Empty;
    [ObservableProperty] private string _lblDocColRemise = string.Empty;
    [ObservableProperty] private string _lblDocColTva = string.Empty;
    [ObservableProperty] private string _lblDocColMontantHt = string.Empty;
    [ObservableProperty] private string _lblDocColMontantTtc = string.Empty;

    public DocumentLineGridColumnState LineGridColumns { get; } = new(supportsLineRemise: true);

    public AutoCompleteFilterPredicate<object?> PartyAutocompleteFilter => PartyAutoComplete.ItemFilter;

    public ObservableCollection<DocumentCatalogItem> AddLineSearchResults => _addLineSearch.Results;

    [ObservableProperty] private decimal _totalHt;
    [ObservableProperty] private decimal _totalTva;
    [ObservableProperty] private decimal _totalTtc;
    [ObservableProperty] private string _totalHtLabel = "HT 0,00";
    [ObservableProperty] private string _totalTvaLabel = "TVA 0,00";
    [ObservableProperty] private string _totalTtcLabel = "TTC 0,00";
    [ObservableProperty] private string _devise = string.Empty;
    [ObservableProperty] private string _addLineSearchText = string.Empty;
    [ObservableProperty] private object? _addLineCatalogPick;
    private bool _suppressAddLinePick;

    private void OnLineGridColumnsPropertyChanged(object? sender, PropertyChangedEventArgs e) =>
        _uiPreferences.SaveDocumentLineColumns("bon_commande_client", LineGridColumns);

    private void LignesOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
            foreach (BCVLineRow row in e.NewItems)
                row.PropertyChanged += LineOnPropertyChanged;
        if (e.OldItems != null)
            foreach (BCVLineRow row in e.OldItems)
                row.PropertyChanged -= LineOnPropertyChanged;
        RefreshTotals();
    }

    private void LineOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(BCVLineRow.ProduitId) or nameof(BCVLineRow.ServiceId)
            && sender is BCVLineRow row && (row.ProduitId is > 0 || row.ServiceId is > 0))
            ConsolidateDuplicateCatalogLines();
        RefreshTotals();
    }

    partial void OnAddLineSearchTextChanged(string value) => _addLineSearch.QueueSearch(value);

    private void RefreshBccUi()
    {
        BtnPdf = _locale.T("Btn_Pdf");
        BtnPrint = _locale.T("Btn_Print");
        BtnBack = _locale.T("Btn_Back");
        BtnSave = _locale.T("Btn_Save");
        BtnToBl = _locale.T("Btn_ToBL");
        MenuDeleteBcc = _locale.T("BCC_MenuDelete");
        LblClient = _locale.T("Lbl_Client");
        WmClientSearch = _locale.T("Wm_SearchClient");
        LblDateBc = _locale.T("Lbl_DateBC");
        BtnAddLine = _locale.T("Btn_AddLine");
        BtnRemoveLine = _locale.T("Btn_RemoveLine");
        LblAddProduct = _locale.T("Devis_LblAddProduct");
        WmAddProduct = _locale.T("Wm_SearchCatalog");
        LblTotals = _locale.T("Lbl_Totals");
        LblDocLineColumnsHint = _locale.T("DocLine_ColumnsHint");
        LblDocColRef = _locale.T("DocLine_ColRef");
        LblDocColDesignation = _locale.T("DocLine_ColDesignation");
        LblDocColQte = _locale.T("DocLine_ColQte");
        LblDocColCond = _locale.T("DocLine_ColCond");
        WmDocLineUnite = _locale.T("DocLine_WmUnite");
        LblDocColPuHt = _locale.T("DocLine_ColPuHt");
        LblDocColRemise = _locale.T("DocLine_ColRemise");
        LblDocColTva = _locale.T("DocLine_ColTva");
        LblDocColMontantHt = _locale.T("DocLine_ColMontantHt");
        LblDocColMontantTtc = _locale.T("DocLine_ColMontantTtc");
        UpdateTotalLabels(TotalHt, TotalTva, TotalTtc);
    }

    public ObservableCollection<GestionCommerciale.Modules.Tiers.Models.Tiers> Clients { get; } = [];
    public ObservableCollection<BCVLineRow> Lignes { get; } = [];

    [ObservableProperty] private int? _bccId;
    [ObservableProperty] private int _clientId;
    [ObservableProperty] private GestionCommerciale.Modules.Tiers.Models.Tiers? _selectedClient;
    [ObservableProperty] private string _numero = string.Empty;
    [ObservableProperty] private DateTimeOffset _date = new(DateTime.Today);
    [ObservableProperty] private string _note = string.Empty;
    [ObservableProperty] private BCVLineRow? _selectedLine;

    public bool CanEdit => true;

    partial void OnBccIdChanged(int? value) => RemoveBccCommand.NotifyCanExecuteChanged();

    private bool CanRemoveBcc() => BccId != null;

    [RelayCommand(CanExecute = nameof(CanRemoveBcc))]
    private async Task RemoveBccAsync(CancellationToken cancellationToken)
    {
        if (BccId is not { } id) return;

        if (!await _dialog.ConfirmAsync(_locale.T("BCC_Title"), _locale.Tf("BCC_ConfirmDelete", Numero), cancellationToken))
            return;

        IsBusy = true;
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var blockedMsg = await BonCommandeClientDeleteReferencedMessage.BuildIfBlockedAsync(db, id, _locale, cancellationToken);
            if (blockedMsg != null)
            {
                await _dialog.ShowErrorAsync(_locale.T("BCC_Title"), blockedMsg, cancellationToken);
                return;
            }

            var entity = await db.BonsCommandeClient.Include(b => b.Lignes).FirstAsync(b => b.Id == id, cancellationToken);
            db.BonsCommandeClient.Remove(entity);
            await db.SaveChangesAsync(cancellationToken);
            await _dialog.ShowInfoAsync(_locale.T("BCC_Title"), _locale.T("BCC_Deleted"), cancellationToken);
            Back();
        }
        catch (Exception ex)
        {
            AppLog.Error("Échec de la suppression du bon de commande client", ex, "BCVEditViewModel.RemoveBccAsync");
            await _dialog.ShowErrorAsync(_locale.T("BCC_Title"), ex.Message, cancellationToken);
        }
        finally
        {
            IsBusy = false;
        }
    }

    partial void OnAddLineCatalogPickChanged(object? value)
    {
        if (_suppressAddLinePick || !CanEdit) return;
        if (value is not DocumentCatalogItem item) return;
        _suppressAddLinePick = true;
        const decimal addQty = 1;
        var existing = item.Kind == DocumentCatalogKind.Service
            ? Lignes.FirstOrDefault(l => l.ServiceId == item.Id && item.Id != 0)
            : Lignes.FirstOrDefault(l => l.ProduitId == item.Id && item.Id != 0);
        if (existing != null)
        {
            existing.QuantiteCommandee += addQty;
            SelectedLine = existing;
        }
        else
        {
            var row = new BCVLineRow();
            row.ApplyCatalogItem(item);
            row.QuantiteCommandee = addQty;
            Lignes.Add(row);
            SelectedLine = row;
        }

        AddLineCatalogPick = null;
        AddLineSearchText = string.Empty;
        _suppressAddLinePick = false;
        _addLineSearch.Clear();
        RefreshTotals();
    }

    private void ConsolidateDuplicateCatalogLines()
    {
        foreach (var g in Lignes.Where(l => l.ProduitId is > 0).GroupBy(l => l.ProduitId).ToList())
        {
            if (g.Count() < 2) continue;
            MergeDuplicateGroup(g);
        }

        foreach (var g in Lignes.Where(l => l.ServiceId is > 0).GroupBy(l => l.ServiceId).ToList())
        {
            if (g.Count() < 2) continue;
            MergeDuplicateGroup(g);
        }
    }

    private void MergeDuplicateGroup(IEnumerable<BCVLineRow> group)
    {
        var ordered = group.OrderBy(l => Lignes.IndexOf(l)).ToList();
        var keep = ordered[0];
        var extraQty = ordered.Skip(1).Sum(l => l.QuantiteCommandee);
        foreach (var line in ordered.Skip(1))
        {
            if (ReferenceEquals(SelectedLine, line))
                SelectedLine = keep;
            line.PropertyChanged -= LineOnPropertyChanged;
            Lignes.Remove(line);
        }

        keep.QuantiteCommandee += extraQty;
    }

    private void RefreshTotals()
    {
        var ht = Lignes.Sum(l => l.MontantHt);
        var tva = LineGridColumns.ShowTva ? Lignes.Sum(l => l.MontantHt * (l.TauxTva / 100m)) : 0m;
        var ttc = ht + tva;
        TotalHt = ht;
        TotalTva = tva;
        TotalTtc = ttc;
        UpdateTotalLabels(ht, tva, ttc);
    }

    private void UpdateTotalLabels(decimal ht, decimal tva, decimal ttc)
    {
        TotalHtLabel = _locale.Tf("Doc_FmtHt", ht, Devise).TrimEnd();
        TotalTvaLabel = _locale.Tf("Doc_FmtTva", tva, Devise).TrimEnd();
        TotalTtcLabel = _locale.Tf("Doc_FmtTtc", ttc, Devise).TrimEnd();
    }

    partial void OnDeviseChanged(string value) => RefreshTotals();

    partial void OnSelectedClientChanged(GestionCommerciale.Modules.Tiers.Models.Tiers? value)
    {
        var id = value?.Id ?? 0;
        if (ClientId == id) return;
        ClientId = id;
    }

    partial void OnClientIdChanged(int value)
    {
        if (SelectedClient?.Id == value) return;
        SelectedClient = Clients.FirstOrDefault(c => c.Id == value);
    }

    public async Task LoadAsync(int? id, CancellationToken cancellationToken = default)
    {
        BccId = id;
        Lignes.Clear();
        SelectedLine = null;
        ResetAddProductSearch();
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        var clients = await db.Tiers.AsNoTracking()
            .Where(t => t.Actif && (t.Type == GestionCommerciale.Modules.Tiers.Models.TypeTiers.Client || t.Type == GestionCommerciale.Modules.Tiers.Models.TypeTiers.LesDeux))
            .OrderBy(t => t.Nom).ToListAsync(cancellationToken);
        Clients.Clear();
        foreach (var c in clients) Clients.Add(c);

        var cfg = await _settings.GetAsync(cancellationToken);
        Devise = CurrencyHelper.FromSettings(cfg);

        if (id == null)
        {
            Numero = "(brouillon)";
            ClientId = Clients.FirstOrDefault()?.Id ?? 0;
            Title = _locale.T("BCC_NewTitle");
            RefreshTotals();
            return;
        }

        var b = await db.BonsCommandeClient.Include(x => x.Lignes).FirstAsync(x => x.Id == id, cancellationToken);
        Numero = b.Numero;
        ClientId = b.ClientId;
        Date = new DateTimeOffset(b.Date);
        Note = b.Note;
        foreach (var l in b.Lignes)
        {
            Lignes.Add(new BCVLineRow
            {
                ProduitId = l.ProduitId,
                ServiceId = l.ServiceId,
                Designation = l.Designation,
                Conditionnement = l.Conditionnement,
                QuantiteCommandee = l.QuantiteCommandee,
                PrixUnitaireHt = l.PrixUnitaireHT,
                Remise = l.Remise,
                TauxTva = l.TauxTVA
            });
        }

        Title = _locale.Tf("BCC_TitleNum", Numero);
        RefreshTotals();
        ResetAddProductSearch();
    }

    private void ResetAddProductSearch()
    {
        _suppressAddLinePick = true;
        AddLineCatalogPick = null;
        AddLineSearchText = string.Empty;
        _suppressAddLinePick = false;
        _addLineSearch.Clear();
    }

    public void Load(int? id) => _ = LoadAsync(id, CancellationToken.None);

    [RelayCommand]
    private void RemoveLine(BCVLineRow? row)
    {
        if (!CanEdit || row == null) return;
        Lignes.Remove(row);
    }

    [RelayCommand]
    private void RemoveSelectedLine()
    {
        if (SelectedLine == null) return;
        RemoveLine(SelectedLine);
        SelectedLine = null;
    }

    [RelayCommand]
    private async Task SaveAsync(CancellationToken cancellationToken)
    {
        if (ClientId == 0 || !Lignes.Any())
        {
            await _dialog.ShowErrorAsync(_locale.T("BCC_Title"), _locale.T("BCC_ErrClientLines"), cancellationToken);
            return;
        }

        if (DocumentTotalsHelper.IsEffectivelyZeroTotal(TotalTtc))
        {
            await _dialog.ShowErrorAsync(_locale.T("BCC_Title"), _locale.T("Doc_ErrZeroTtc"), cancellationToken);
            return;
        }

        IsBusy = true;
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            BonCommandeClient entity;
            if (BccId == null)
            {
                var num = await _numbers.NextBCClientAsync(cancellationToken);
                entity = new BonCommandeClient
                {
                    Numero = num,
                    ClientId = ClientId,
                    Date = Date.DateTime,
                    Note = Note,
                    CreatedByUserId = _session.UserId
                };
                foreach (var l in Lignes)
                {
                    entity.Lignes.Add(new BonCommandeClientLigne
                    {
                        ProduitId = l.IsService ? null : l.ProduitId,
                        ServiceId = l.IsService ? l.ServiceId : null,
                        Designation = l.Designation,
                        Conditionnement = l.Conditionnement,
                        QuantiteCommandee = l.QuantiteCommandee,
                        PrixUnitaireHT = l.PrixUnitaireHt,
                        Remise = l.Remise,
                        TauxTVA = l.TauxTva
                    });
                }

                db.BonsCommandeClient.Add(entity);
                await db.SaveChangesAsync(cancellationToken);
                BccId = entity.Id;
            }
            else
            {
                entity = await db.BonsCommandeClient.Include(b => b.Lignes).FirstAsync(b => b.Id == BccId, cancellationToken);
                entity.ClientId = ClientId;
                entity.Date = Date.DateTime;
                entity.Note = Note;
                db.BonCommandeClientLignes.RemoveRange(entity.Lignes);
                foreach (var l in Lignes)
                {
                    entity.Lignes.Add(new BonCommandeClientLigne
                    {
                        ProduitId = l.IsService ? null : l.ProduitId,
                        ServiceId = l.IsService ? l.ServiceId : null,
                        Designation = l.Designation,
                        Conditionnement = l.Conditionnement,
                        QuantiteCommandee = l.QuantiteCommandee,
                        PrixUnitaireHT = l.PrixUnitaireHt,
                        Remise = l.Remise,
                        TauxTVA = l.TauxTva
                    });
                }

                await db.SaveChangesAsync(cancellationToken);
            }

            Numero = entity.Numero;
            await _dialog.ShowInfoAsync(_locale.T("BCC_Title"), _locale.T("BCC_Saved"), cancellationToken);
            await LoadAsync(BccId, cancellationToken);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ToBlAsync(CancellationToken cancellationToken)
    {
        if (BccId is not { } id)
        {
            await _dialog.ShowErrorAsync(_locale.T("BCC_Title"), _locale.T("BCC_ToBlNeedSave"), cancellationToken);
            return;
        }

        var vm = _sp.GetRequiredService<BLEditViewModel>();
        await vm.LoadNewFromBonCommandeClientAsync(id, cancellationToken);
        _workspace.Open(vm);
    }

    [RelayCommand]
    private void Back()
    {
        var list = _sp.GetRequiredService<BCVListViewModel>();
        _workspace.Open(list);
        list.LoadCommand.Execute(null);
    }

    [RelayCommand]
    private async Task ExportPdfAsync(CancellationToken cancellationToken)
    {
        if (BccId is not { }) return;
        try
        {
            IsBusy = true;
            var bytes = await BuildBccPdfBytesAsync(cancellationToken);
            if (bytes == null) return;
            var ok = await _dialog.SavePickedFileBytesAsync(_locale.T("Export_PdfPicker"), $"{Numero}.pdf", new[] { "*.pdf" }, bytes, cancellationToken);
            if (ok)
                await _dialog.ShowInfoAsync(_locale.T("Export_Pdf"), _locale.T("Export_Done"), cancellationToken);
        }
        catch (Exception ex)
        {
            AppLog.Error("Échec de l'export PDF du bon de commande client", ex, "BCVEditViewModel.ExportPdfAsync");
            await _dialog.ShowErrorAsync(_locale.T("Export_Pdf"), ex.Message, cancellationToken);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task PrintAsync(CancellationToken cancellationToken)
    {
        if (BccId is not { }) return;
        try
        {
            IsBusy = true;
            var bytes = await BuildBccPdfBytesAsync(cancellationToken);
            if (bytes == null) return;
            await _pdfPrint.PrintPdfAsync(bytes, Numero, cancellationToken);
        }
        catch (Exception ex)
        {
            AppLog.Error("Échec de l'impression du bon de commande client", ex, "BCVEditViewModel.PrintAsync");
            await _dialog.ShowErrorAsync(_locale.T("Btn_Print"), ex.Message, cancellationToken);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task<byte[]?> BuildBccPdfBytesAsync(CancellationToken cancellationToken)
    {
        if (BccId is not { } id) return null;
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        var b = await db.BonsCommandeClient.Include(x => x.Lignes).FirstAsync(x => x.Id == id, cancellationToken);
        var client = await db.Tiers.AsNoTracking().FirstAsync(t => t.Id == b.ClientId, cancellationToken);
        return await _pdf.BuildBonCommandeClientPdfAsync(b, DocumentPartyPdfInfo.FromTiers(client), cancellationToken);
    }
}
