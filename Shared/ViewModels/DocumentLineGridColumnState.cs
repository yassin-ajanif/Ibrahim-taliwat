using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;

namespace GestionCommerciale.Shared.ViewModels;

/// <summary>Column visibility and widths for document line grids (devis, facture, BC, etc.).</summary>
public partial class DocumentLineGridColumnState : ObservableObject
{
    public DocumentLineGridColumnState(bool supportsLineRemise = true) =>
        SupportsLineRemise = supportsLineRemise;

    public bool SupportsLineRemise { get; }

    [ObservableProperty] private bool _showReference = true;
    [ObservableProperty] private bool _showDesignation = true;
    [ObservableProperty] private bool _showQuantite = true;
    [ObservableProperty] private bool _showConditionnement = true;
    [ObservableProperty] private bool _showPuHt = true;
    [ObservableProperty] private bool _showRemise = true;
    [ObservableProperty] private bool _showTva = true;
    [ObservableProperty] private bool _showMontantHt = true;
    [ObservableProperty] private bool _showMontantTtc = true;

    private bool EffectiveShowRemise => SupportsLineRemise && ShowRemise;

    public GridLength ColRef => ShowReference ? new GridLength(1.15, GridUnitType.Star) : new GridLength(0);
    public GridLength ColDesignation => ShowDesignation ? new GridLength(2.35, GridUnitType.Star) : new GridLength(0);
    public GridLength ColQte => ShowQuantite ? new GridLength(0.85, GridUnitType.Star) : new GridLength(0);
    public GridLength ColCond => ShowConditionnement ? new GridLength(0.75, GridUnitType.Star) : new GridLength(0);
    public GridLength ColPuHt => ShowPuHt ? new GridLength(0.95, GridUnitType.Star) : new GridLength(0);
    public GridLength ColRemise => EffectiveShowRemise ? new GridLength(0.65, GridUnitType.Star) : new GridLength(0);
    public GridLength ColTva => ShowTva ? new GridLength(0.65, GridUnitType.Star) : new GridLength(0);
    public GridLength ColMontantHt => ShowMontantHt ? new GridLength(0.95, GridUnitType.Star) : new GridLength(0);
    public GridLength ColMontantTtc => ShowMontantTtc ? new GridLength(0.95, GridUnitType.Star) : new GridLength(0);

    partial void OnShowReferenceChanged(bool value) => NotifyColWidths();
    partial void OnShowDesignationChanged(bool value) => NotifyColWidths();
    partial void OnShowQuantiteChanged(bool value) => NotifyColWidths();
    partial void OnShowConditionnementChanged(bool value) => NotifyColWidths();
    partial void OnShowPuHtChanged(bool value) => NotifyColWidths();
    partial void OnShowRemiseChanged(bool value) => NotifyColWidths();
    partial void OnShowTvaChanged(bool value) => NotifyColWidths();
    partial void OnShowMontantHtChanged(bool value) => NotifyColWidths();
    partial void OnShowMontantTtcChanged(bool value) => NotifyColWidths();

    private void NotifyColWidths()
    {
        OnPropertyChanged(nameof(ColRef));
        OnPropertyChanged(nameof(ColDesignation));
        OnPropertyChanged(nameof(ColQte));
        OnPropertyChanged(nameof(ColCond));
        OnPropertyChanged(nameof(ColPuHt));
        OnPropertyChanged(nameof(ColRemise));
        OnPropertyChanged(nameof(ColTva));
        OnPropertyChanged(nameof(ColMontantHt));
        OnPropertyChanged(nameof(ColMontantTtc));
    }
}
