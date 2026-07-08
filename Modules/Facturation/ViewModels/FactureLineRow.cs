using CommunityToolkit.Mvvm.ComponentModel;
using GestionCommerciale.Modules.Services.Models;
using GestionCommerciale.Modules.Stock.Models;
using GestionCommerciale.Shared.Helpers;

namespace GestionCommerciale.Modules.Facturation.ViewModels;

public partial class FactureLineRow : ObservableObject
{
    [ObservableProperty] private int? _bonLivraisonId;
    [ObservableProperty] private int? _produitId;
    [ObservableProperty] private int? _serviceId;
    [ObservableProperty] private string _reference = string.Empty;
    [ObservableProperty] private string _designation = string.Empty;
    [ObservableProperty] private string _conditionnement = string.Empty;
    [ObservableProperty] private decimal _quantite = 1;
    [ObservableProperty] private decimal _prixUnitaireHt;
    [ObservableProperty] private decimal _remise;
    [ObservableProperty] private decimal _tauxTva;

    public bool IsService => ServiceId is > 0;

    public decimal MontantHt => DocumentTotalsHelper.LigneHT(Quantite, PrixUnitaireHt, Remise);

    public decimal MontantTtc => MontantHt * (1 + TauxTva / 100m);

    partial void OnQuantiteChanged(decimal value) => NotifyMontants();
    partial void OnPrixUnitaireHtChanged(decimal value) => NotifyMontants();
    partial void OnRemiseChanged(decimal value) => NotifyMontants();
    partial void OnTauxTvaChanged(decimal value) => NotifyMontants();

    public void ApplyCatalogProduct(Produit p)
    {
        ProduitId = p.Id;
        ServiceId = null;
        Reference = p.Reference;
        Designation = p.Designation;
        Conditionnement = p.Unite;
        PrixUnitaireHt = p.PrixVenteHT;
        TauxTva = p.TauxTVA;
        NotifyMontants();
    }

    public void ApplyCatalogService(Service s)
    {
        ServiceId = s.Id;
        ProduitId = null;
        Reference = s.Reference;
        Designation = s.Designation;
        Conditionnement = s.Unite;
        PrixUnitaireHt = s.PrixVenteHT;
        TauxTva = s.TauxTVA;
        NotifyMontants();
    }

    public void ApplyCatalogItem(DocumentCatalogItem item)
    {
        if (item.Kind == DocumentCatalogKind.Service)
        {
            ServiceId = item.Id;
            ProduitId = null;
        }
        else
        {
            ProduitId = item.Id;
            ServiceId = null;
        }

        Reference = item.Reference;
        Designation = item.Designation;
        Conditionnement = item.Unite;
        PrixUnitaireHt = item.PrixVenteHT;
        TauxTva = item.TauxTVA;
        NotifyMontants();
    }

    private void NotifyMontants()
    {
        OnPropertyChanged(nameof(MontantHt));
        OnPropertyChanged(nameof(MontantTtc));
    }
}
