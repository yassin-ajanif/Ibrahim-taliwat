using CommunityToolkit.Mvvm.ComponentModel;

namespace GestionCommerciale.Modules.Devis.ViewModels;

public partial class DevisConditionRow : ObservableObject
{
    [ObservableProperty] private string _titre = string.Empty;
    [ObservableProperty] private string _valeur = string.Empty;
}
