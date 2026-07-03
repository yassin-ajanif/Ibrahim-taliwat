using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GestionCommerciale.Shared.Database;
using GestionCommerciale.Shared.Services;
using Microsoft.EntityFrameworkCore;

namespace GestionCommerciale.Modules.Devis;

internal static class DevisDeleteReferencedMessage
{
    /// <summary>Returns a localized error body if the devis is referenced by BLs or factures; otherwise null.</summary>
    public static async Task<string?> BuildIfBlockedAsync(
        AppDbContext db,
        int devisId,
        ILocaleService locale,
        CancellationToken cancellationToken = default)
    {
        var blNums = await db.BonsLivraison.AsNoTracking()
            .Where(b => b.DevisId == devisId)
            .OrderBy(b => b.Numero)
            .Select(b => b.Numero)
            .ToListAsync(cancellationToken);

        var factNums = await db.Factures.AsNoTracking()
            .Where(f => f.DevisId == devisId)
            .OrderBy(f => f.Numero)
            .Select(f => f.Numero)
            .ToListAsync(cancellationToken);

        if (blNums.Count == 0 && factNums.Count == 0)
            return null;

        var lines = new List<string> { locale.T("Devis_ErrDeleteReferencedIntro") };
        if (blNums.Count > 0)
            lines.Add(locale.Tf("Devis_ErrRefBl", string.Join(", ", blNums)));
        if (factNums.Count > 0)
            lines.Add(locale.Tf("Devis_ErrRefFact", string.Join(", ", factNums)));

        return string.Join(Environment.NewLine, lines);
    }
}
