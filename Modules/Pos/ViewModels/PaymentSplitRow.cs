using System;
using CommunityToolkit.Mvvm.ComponentModel;
using GestionCommerciale.Modules.Facturation.Models;

namespace GestionCommerciale.Modules.Pos.ViewModels;

public partial class PaymentSplitRow : ObservableObject
{
    [ObservableProperty] private ModePaiement _mode = ModePaiement.Especes;
    [ObservableProperty] private decimal _montant;

    public Array ModesPaiement => Enum.GetValues(typeof(ModePaiement));
}
