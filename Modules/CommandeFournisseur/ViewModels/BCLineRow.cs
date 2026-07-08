using CommunityToolkit.Mvvm.ComponentModel;
using GestionCommerciale.Modules.Stock.Models;
using GestionCommerciale.Shared.Helpers;

namespace GestionCommerciale.Modules.CommandeFournisseur.ViewModels;

public partial class BCLineRow : ObservableObject
{
    [ObservableProperty] private int? _produitId;
    [ObservableProperty] private int? _serviceId;
    [ObservableProperty] private string _reference = string.Empty;
    [ObservableProperty] private string _designation = string.Empty;
    [ObservableProperty] private string _conditionnement = string.Empty;
    [ObservableProperty] private decimal _quantiteCommandee;
    [ObservableProperty] private decimal _prixUnitaireHt;
    [ObservableProperty] private decimal _remise;
    [ObservableProperty] private decimal _tauxTva;

    public bool IsService => ServiceId is > 0;

    public decimal MontantHt => DocumentTotalsHelper.LigneHT(QuantiteCommandee, PrixUnitaireHt, Remise);

    public decimal MontantTtc => MontantHt * (1 + TauxTva / 100m);

    partial void OnQuantiteCommandeeChanged(decimal value) => NotifyMontants();
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
        PrixUnitaireHt = p.PrixAchatHT;
        TauxTva = p.TauxTVA;
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
        PrixUnitaireHt = item.PrixAchatHT;
        TauxTva = item.TauxTVA;
        NotifyMontants();
    }

    private void NotifyMontants()
    {
        OnPropertyChanged(nameof(MontantHt));
        OnPropertyChanged(nameof(MontantTtc));
    }
}
