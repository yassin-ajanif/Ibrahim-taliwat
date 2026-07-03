using GestionCommerciale.Shared.Database;
using Microsoft.EntityFrameworkCore;

namespace GestionCommerciale.Modules.Auth.ViewModels;

internal static class DocumentNumberingQuery
{
    public static Task<List<string>> LoadNumerosAsync(AppDbContext db, string prefix, CancellationToken cancellationToken) =>
        prefix.ToUpperInvariant() switch
        {
            "DEV" => db.Devis.AsNoTracking().Select(d => d.Numero).ToListAsync(cancellationToken),
            "BL" => db.BonsLivraison.AsNoTracking().Select(d => d.Numero).ToListAsync(cancellationToken),
            "BR" => db.BonsReception.AsNoTracking().Select(d => d.Numero).ToListAsync(cancellationToken),
            "BC" => db.BonsCommande.AsNoTracking().Select(d => d.Numero).ToListAsync(cancellationToken),
            "BCC" => db.BonsCommandeClient.AsNoTracking().Select(d => d.Numero).ToListAsync(cancellationToken),
            "FAC" => db.Factures.AsNoTracking().Select(d => d.Numero).ToListAsync(cancellationToken),
            "FAF" => db.FacturesFournisseurs.AsNoTracking().Select(d => d.Numero).ToListAsync(cancellationToken),
            "AVO" => db.Avoirs.AsNoTracking().Select(d => d.Numero).ToListAsync(cancellationToken),
            "AVF" => db.AvoirsFournisseurs.AsNoTracking().Select(d => d.Numero).ToListAsync(cancellationToken),
            _ => Task.FromResult(new List<string>())
        };
}
