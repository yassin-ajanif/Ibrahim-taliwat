using GestionCommerciale.Shared.Database;
using GestionCommerciale.Shared.Services;
using Microsoft.EntityFrameworkCore;

namespace GestionCommerciale.Modules.CommandeClient;

internal static class BonCommandeClientDeleteReferencedMessage
{
    public static async Task<string?> BuildIfBlockedAsync(
        AppDbContext db,
        int bonCommandeClientId,
        ILocaleService locale,
        CancellationToken cancellationToken = default)
    {
        var blNums = await db.BonsLivraison.AsNoTracking()
            .Where(b => b.BonCommandeClientId == bonCommandeClientId)
            .OrderBy(b => b.Numero)
            .Select(b => b.Numero)
            .ToListAsync(cancellationToken);

        if (blNums.Count == 0)
            return null;

        return string.Join(Environment.NewLine,
            locale.T("BCC_ErrDeleteReferencedIntro"),
            locale.Tf("BCC_ErrRefBl", string.Join(", ", blNums)));
    }
}
