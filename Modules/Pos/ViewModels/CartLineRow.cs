using CommunityToolkit.Mvvm.ComponentModel;

namespace GestionCommerciale.Modules.Pos.ViewModels;

public partial class CartLineRow : ObservableObject
{
    public int ProduitId { get; set; }
    public string Reference { get; set; } = string.Empty;
    public string Designation { get; set; } = string.Empty;
    public decimal PrixUnitaireHt { get; set; }
    public decimal TauxTva { get; set; }

    [ObservableProperty] private decimal _quantite = 1;
    [ObservableProperty] private decimal _remisePct;
    [ObservableProperty] private decimal _remiseMontant;

    public decimal PrixUnitaireTtc => PrixUnitaireHt * (1 + TauxTva / 100m);

    private decimal LigneTtc => Quantite * PrixUnitaireTtc * (1 - RemisePct / 100m) - RemiseMontant;

    public decimal MontantTtc => Math.Max(LigneTtc, 0);
    public decimal MontantHt => MontantTtc / (1 + TauxTva / 100m);

    public decimal EffectiveRemisePct
    {
        get
        {
            var rawTtc = Quantite * PrixUnitaireTtc;
            if (rawTtc == 0) return 0;
            return (rawTtc - MontantTtc) / rawTtc * 100m;
        }
    }

    partial void OnQuantiteChanged(decimal value) => NotifyChange();
    partial void OnRemisePctChanged(decimal value) => NotifyChange();
    partial void OnRemiseMontantChanged(decimal value) => NotifyChange();

    private void NotifyChange()
    {
        OnPropertyChanged(nameof(MontantHt));
        OnPropertyChanged(nameof(MontantTtc));
        OnPropertyChanged(nameof(EffectiveRemisePct));
    }
}
