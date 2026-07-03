using GestionCommerciale.Modules.Stock.Models;
using GestionCommerciale.Modules.Stock.Services;
using GestionCommerciale.Shared.Database;
using Microsoft.EntityFrameworkCore;

namespace GestionCommerciale.Modules.Stock;

public static class MouvementStockQueries
{
    public static IQueryable<MouvementStock> WherePartyNameMatches(
        this IQueryable<MouvementStock> query,
        AppDbContext db,
        string? searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return query;

        var t = searchTerm.Trim().ToLowerInvariant();

        return query.Where(m =>
            (m.OrigineType == StockMovementService.OrigineTypeBonLivraison
             && m.OrigineId != null
             && db.BonsLivraison.Any(bl =>
                 bl.Id == m.OrigineId
                 && db.Tiers.Any(tier => tier.Id == bl.ClientId && tier.Nom.ToLower().Contains(t))))
            || (m.OrigineType == StockMovementService.OrigineTypeAvoir
                && m.OrigineId != null
                && db.Avoirs.Any(a =>
                    a.Id == m.OrigineId
                    && db.Tiers.Any(tier => tier.Id == a.ClientId && tier.Nom.ToLower().Contains(t))))
            || (m.OrigineType == StockMovementService.OrigineTypeBonReception
                && m.OrigineId != null
                && db.BonsReception.Any(br =>
                    br.Id == m.OrigineId
                    && db.Tiers.Any(tier => tier.Id == br.FournisseurId && tier.Nom.ToLower().Contains(t))))
            || (m.OrigineType == StockMovementService.OrigineTypeAvoirFournisseur
                && m.OrigineId != null
                && db.AvoirsFournisseurs.Any(a =>
                    a.Id == m.OrigineId
                    && db.Tiers.Any(tier => tier.Id == a.FournisseurId && tier.Nom.ToLower().Contains(t)))));
    }
}
