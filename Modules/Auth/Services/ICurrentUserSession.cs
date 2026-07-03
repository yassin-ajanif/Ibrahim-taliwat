using GestionCommerciale.Modules.Auth.Models;

namespace GestionCommerciale.Modules.Auth.Services;

public interface ICurrentUserSession
{
    bool IsAuthenticated { get; }
    int? UserId { get; }
    Role? Role { get; }
    string? Nom { get; }
    bool IsAdmin { get; }

    /// <summary>Connexion avec l’administrateur local (sans table Users).</summary>
    void SetDefaultAdminSession();
    void Clear();
    bool CanAccessClients { get; }
    bool CanAccessFournisseurs { get; }
    bool CanAccessStock { get; }
    bool CanAccessDevis { get; }
    bool CanAccessBL { get; }
    bool CanAccessBR { get; }
    bool CanAccessBC { get; }
    bool CanAccessFacturation { get; }
    bool CanAccessAvoir { get; }
    bool CanAccessReporting { get; }
    bool CanAccessUsers { get; }
    bool CanAccessSettings { get; }
}
