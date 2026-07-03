using GestionCommerciale.Modules.AvoirFournisseur.Models;
using GestionCommerciale.Modules.CommandeClient.Models;
using GestionCommerciale.Modules.CommandeFournisseur.Models;
using GestionCommerciale.Modules.Devis.Models;
using GestionCommerciale.Modules.Facturation.Models;
using GestionCommerciale.Modules.FactureFournisseur.Models;
using GestionCommerciale.Modules.Livraison.Models;
using GestionCommerciale.Modules.Reception.Models;

namespace GestionCommerciale.Shared.Helpers;

public static class DocumentTotalsHelper
{
    public const decimal ZeroTotalTolerance = 0.005m;
    public const decimal PaiementTtcTolerance = 0.02m;

    public static bool IsEffectivelyZeroTotal(decimal amount) =>
        Math.Abs(amount) <= ZeroTotalTolerance;

    public static bool PaymentsExceedTtc(decimal ttc, decimal totalPayments) =>
        totalPayments > ttc + PaiementTtcTolerance;

    public static void EnsurePaymentsNotOverTtc(decimal ttc, decimal totalPayments)
    {
        if (PaymentsExceedTtc(ttc, totalPayments))
        {
            throw new InvalidOperationException(
                $"La somme des paiements ({totalPayments:N2} TTC) ne peut pas dépasser le total de la facture ({ttc:N2} TTC).");
        }
    }

    public static decimal LigneHT(decimal qte, decimal puHt, decimal remisePct) =>
        qte * puHt * (1 - remisePct / 100m);

    public static (decimal ht, decimal tva, decimal ttc) DevisTotals(IEnumerable<DevisLigne> lignes, decimal remiseGlobalePct)
    {
        decimal ht = 0, tva = 0;
        foreach (var l in lignes)
        {
            var lht = LigneHT(l.Quantite, l.PrixUnitaireHT, l.Remise);
            ht += lht;
            tva += lht * (l.TauxTVA / 100m);
        }

        if (remiseGlobalePct > 0)
        {
            var factor = 1 - remiseGlobalePct / 100m;
            ht *= factor;
            tva *= factor;
        }

        return (ht, tva, ht + tva);
    }

    public static (decimal ht, decimal tva, decimal ttc) FactureTotals(IEnumerable<FactureLigne> lignes, decimal remiseGlobalePct)
    {
        decimal ht = 0, tva = 0;
        foreach (var l in lignes)
        {
            var lht = LigneHT(l.Quantite, l.PrixUnitaireHT, l.Remise);
            ht += lht;
            tva += lht * (l.TauxTVA / 100m);
        }

        if (remiseGlobalePct > 0)
        {
            var factor = 1 - remiseGlobalePct / 100m;
            ht *= factor;
            tva *= factor;
        }

        return (ht, tva, ht + tva);
    }

    public static decimal FactureTtc(IEnumerable<FactureLigne> lignes, decimal remiseGlobalePct) =>
        FactureTotals(lignes, remiseGlobalePct).ttc;

    public static void SyncFactureTotalTtc(Facture facture) =>
        facture.TotalTtc = FactureTtc(facture.Lignes, facture.RemiseGlobale);

    public static (decimal ht, decimal tva, decimal ttc) FactureFournisseurTotals(IEnumerable<FactureFournisseurLigne> lignes, decimal remiseGlobalePct)
    {
        decimal ht = 0, tva = 0;
        foreach (var l in lignes)
        {
            var lht = LigneHT(l.Quantite, l.PrixUnitaireHT, l.Remise);
            ht += lht;
            tva += lht * (l.TauxTVA / 100m);
        }

        if (remiseGlobalePct > 0)
        {
            var factor = 1 - remiseGlobalePct / 100m;
            ht *= factor;
            tva *= factor;
        }

        return (ht, tva, ht + tva);
    }

    public static decimal FactureFournisseurTtc(IEnumerable<FactureFournisseurLigne> lignes, decimal remiseGlobalePct) =>
        FactureFournisseurTotals(lignes, remiseGlobalePct).ttc;

    public static void SyncFactureFournisseurTotalTtc(FactureFournisseur facture) =>
        facture.TotalTtc = FactureFournisseurTtc(facture.Lignes, facture.RemiseGlobale);

    public static decimal BonReceptionTtc(IEnumerable<BonReceptionLigne> lignes) =>
        BonReceptionTotals(lignes).ttc;

    public static void SyncBonReceptionTotalTtc(BonReception bonReception) =>
        bonReception.TotalTtc = BonReceptionTtc(bonReception.Lignes);

    public static (decimal ht, decimal tva, decimal ttc) AvoirTotals(IEnumerable<AvoirLigne> lignes)
    {
        decimal ht = 0, tva = 0;
        foreach (var l in lignes)
        {
            var lht = LigneHT(l.Quantite, l.PrixUnitaireHT, l.Remise);
            ht += lht;
            tva += lht * (l.TauxTVA / 100m);
        }

        return (ht, tva, ht + tva);
    }

    public static (decimal ht, decimal tva, decimal ttc) AvoirFournisseurTotals(IEnumerable<AvoirFournisseurLigne> lignes)
    {
        decimal ht = 0, tva = 0;
        foreach (var l in lignes)
        {
            var lht = LigneHT(l.Quantite, l.PrixUnitaireHT, l.Remise);
            ht += lht;
            tva += lht * (l.TauxTVA / 100m);
        }

        return (ht, tva, ht + tva);
    }

    /// <summary>Same semantics as <c>BLEditViewModel.RefreshTotals</c> (TVA included in TTC).</summary>
    public static (decimal ht, decimal tva, decimal ttc) BonLivraisonTotals(IEnumerable<BonLivraisonLigne> lignes)
    {
        decimal ht = 0, tva = 0;
        foreach (var l in lignes)
        {
            var lht = LigneHT(l.QuantiteLivree, l.PrixUnitaireHT, l.Remise);
            ht += lht;
            tva += lht * (l.TauxTVA / 100m);
        }

        return (ht, tva, ht + tva);
    }

    /// <summary>Same semantics as <c>BCEditViewModel.RefreshTotals</c> when TVA columns are shown.</summary>
    public static (decimal ht, decimal tva, decimal ttc) BonCommandeTotals(IEnumerable<BonCommandeLigne> lignes)
    {
        decimal ht = 0, tva = 0;
        foreach (var l in lignes)
        {
            var lht = LigneHT(l.QuantiteCommandee, l.PrixUnitaireHT, l.Remise);
            ht += lht;
            tva += lht * (l.TauxTVA / 100m);
        }

        return (ht, tva, ht + tva);
    }

    /// <summary>Same semantics as <c>BCVEditViewModel.RefreshTotals</c> when TVA columns are shown.</summary>
    public static (decimal ht, decimal tva, decimal ttc) BonCommandeClientTotals(IEnumerable<BonCommandeClientLigne> lignes)
    {
        decimal ht = 0, tva = 0;
        foreach (var l in lignes)
        {
            var lht = LigneHT(l.QuantiteCommandee, l.PrixUnitaireHT, l.Remise);
            ht += lht;
            tva += lht * (l.TauxTVA / 100m);
        }

        return (ht, tva, ht + tva);
    }

    /// <summary>HT/TVA/TTC from received quantities (BR lines have no remise).</summary>
    public static (decimal ht, decimal tva, decimal ttc) BonReceptionTotals(IEnumerable<BonReceptionLigne> lignes)
    {
        decimal ht = 0, tva = 0;
        foreach (var l in lignes)
        {
            var lht = l.QuantiteRecue * l.PrixUnitaireHT;
            ht += lht;
            tva += lht * (l.TauxTVA / 100m);
        }

        return (ht, tva, ht + tva);
    }
}
