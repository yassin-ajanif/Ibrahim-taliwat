using GestionCommerciale.Modules.Tiers.Models;

namespace GestionCommerciale.Shared.Models.Pdf;

/// <summary>Client or supplier identity for PDF party panel.</summary>
public sealed record DocumentPartyPdfInfo(
    string Nom,
    string? Ice = null,
    string? Adresse = null,
    string? Telephone = null,
    string? Email = null)
{
    public static DocumentPartyPdfInfo FromTiers(Tiers t)
    {
        var adresse = string.Join(", ", new[] { t.Adresse, t.Ville }.Where(s => !string.IsNullOrWhiteSpace(s)));
        return new DocumentPartyPdfInfo(
            t.Nom,
            string.IsNullOrWhiteSpace(t.ICE) ? null : t.ICE.Trim(),
            string.IsNullOrWhiteSpace(adresse) ? null : adresse.Trim(),
            string.IsNullOrWhiteSpace(t.Telephone) ? null : t.Telephone.Trim(),
            string.IsNullOrWhiteSpace(t.Email) ? null : t.Email.Trim());
    }
}
