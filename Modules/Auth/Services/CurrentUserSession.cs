using GestionCommerciale.Modules.Auth.Models;

namespace GestionCommerciale.Modules.Auth.Services;

public sealed class CurrentUserSession : ICurrentUserSession
{
    public bool IsAuthenticated { get; private set; }
    public int? UserId { get; private set; }
    public Role? Role { get; private set; }
    public string? Nom { get; private set; }
    public bool IsAdmin => Role == Models.Role.Admin;

    public bool CanAccessClients => IsAuthenticated && Role is Models.Role.Admin or Models.Role.Commercial;
    public bool CanAccessFournisseurs => IsAuthenticated && Role is Models.Role.Admin or Models.Role.Magasinier;
    public bool CanAccessStock => IsAuthenticated && Role is Models.Role.Admin or Models.Role.Magasinier;
    public bool CanAccessDevis => IsAuthenticated && Role is Models.Role.Admin or Models.Role.Commercial;
    public bool CanAccessBL => IsAuthenticated && Role is Models.Role.Admin or Models.Role.Commercial or Models.Role.Magasinier;
    public bool CanAccessBR => IsAuthenticated && Role is Models.Role.Admin or Models.Role.Magasinier;
    public bool CanAccessBC => IsAuthenticated && Role is Models.Role.Admin or Models.Role.Magasinier;
    public bool CanAccessFacturation => IsAuthenticated && Role is Models.Role.Admin or Models.Role.Comptable;
    public bool CanAccessAvoir => IsAuthenticated && Role is Models.Role.Admin or Models.Role.Comptable;
    public bool CanAccessReporting => IsAuthenticated && Role is Models.Role.Admin or Models.Role.Comptable;
    public bool CanAccessUsers => false;
    public bool CanAccessSettings => IsAuthenticated && Role == Models.Role.Admin;

    public void SetDefaultAdminSession()
    {
        UserId = null;
        Role = Models.Role.Admin;
        Nom = "Administrateur";
        IsAuthenticated = true;
    }

    public void Clear()
    {
        UserId = null;
        Role = null;
        Nom = null;
        IsAuthenticated = false;
    }
}
