using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GestionCommerciale.Shared.Database;
using GestionCommerciale.Shared.Services;
using Microsoft.EntityFrameworkCore;

namespace GestionCommerciale.Modules.CommandeFournisseur;

internal static class BonCommandeDeleteReferencedMessage
{
    /// <summary>Returns a localized error body if the BC is referenced by bons de réception; otherwise null.</summary>
    public static async Task<string?> BuildIfBlockedAsync(
        AppDbContext db,
        int bonCommandeId,
        ILocaleService locale,
        CancellationToken cancellationToken = default)
    {
        var brNums = await db.BonsReception.AsNoTracking()
            .Where(r => r.BonCommandeId == bonCommandeId)
            .OrderBy(r => r.Numero)
            .Select(r => r.Numero)
            .ToListAsync(cancellationToken);

        if (brNums.Count == 0)
            return null;

        return string.Join(Environment.NewLine,
            locale.T("BC_ErrDeleteReferencedIntro"),
            locale.Tf("BC_ErrRefBr", string.Join(", ", brNums)));
    }
}
