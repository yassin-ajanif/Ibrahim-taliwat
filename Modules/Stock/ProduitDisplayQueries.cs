using GestionCommerciale.Modules.Stock.Models;
using Microsoft.EntityFrameworkCore;

namespace GestionCommerciale.Modules.Stock;

/// <summary>Case-insensitive filter on référence, désignation et code-barres.</summary>
public static class ProduitSearchFilter
{
    public static IQueryable<Produit> WhereSearchMatches(this IQueryable<Produit> query, string? searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return query;

        var t = searchTerm.Trim().ToLowerInvariant();
        return query.Where(p =>
            p.Reference.ToLower().Contains(t) ||
            p.Designation.ToLower().Contains(t) ||
            (p.CodeBarre != null && p.CodeBarre.ToLower().Contains(t)));
    }
}

/// <summary>EF queries that avoid loading large <see cref="Produit.ImageData"/> blobs into list UIs.</summary>
public static class ProduitDisplayQueries
{
    public static IQueryable<Produit> SelectForListWithoutImageData(this IQueryable<Produit> source) =>
        source
            .OrderBy(p => p.Reference)
            .Select(p => new Produit
            {
                Id = p.Id,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                CreatedByUserId = p.CreatedByUserId,
                Reference = p.Reference,
                CodeBarre = p.CodeBarre,
                Designation = p.Designation,
                Unite = p.Unite,
                PrixAchatHT = p.PrixAchatHT,
                PrixVenteHT = p.PrixVenteHT,
                TauxTVA = p.TauxTVA,
                StockActuel = p.StockActuel,
                StockMinimum = p.StockMinimum,
                CategorieId = p.CategorieId,
                Actif = p.Actif,
                Categorie = p.Categorie,
                ImageData = null
            });
}
