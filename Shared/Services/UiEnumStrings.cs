using GestionCommerciale.Modules.Facturation.Models;

namespace GestionCommerciale.Shared.Services;

public static class UiEnumStrings
{
    public static string FormatModePaiement(ILocaleService locale, ModePaiement m) =>
        locale.T(m switch
        {
            ModePaiement.Credit => "ModePaiement_Credit",
            ModePaiement.Cheque => "ModePaiement_Cheque",
            ModePaiement.Especes => "ModePaiement_Especes",
            ModePaiement.TPE => "ModePaiement_TPE",
            ModePaiement.Virement => "ModePaiement_Virement",
            ModePaiement.Effet => "ModePaiement_Effet",
            _ => "ModePaiement_Especes"
        });
}
