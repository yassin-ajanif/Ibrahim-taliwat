using System.Threading;
using System.Threading.Tasks;
using GestionCommerciale.Shared.Database;
using GestionCommerciale.Shared.Services;
using Microsoft.EntityFrameworkCore;

namespace GestionCommerciale.Modules.Livraison;

internal static class BonLivraisonDeleteReferencedMessage
{
    /// <summary>Returns a localized error body if the BL is referenced by factures; otherwise null.</summary>
    public static async Task<string?> BuildIfBlockedAsync(
        AppDbContext db,
        int bonLivraisonId,
        ILocaleService locale,
        CancellationToken cancellationToken = default)
    {
        var bl = await db.BonsLivraison.AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == bonLivraisonId, cancellationToken);

        if (bl?.FactureId == null)
            return null;

        var factNum = await db.Factures.AsNoTracking()
            .Where(f => f.Id == bl.FactureId)
            .Select(f => f.Numero)
            .FirstOrDefaultAsync(cancellationToken);

        return locale.Tf("BL_ErrDeleteAlreadyInvoiced", bl.Numero, factNum ?? $"#{bl.FactureId}");
    }
}
