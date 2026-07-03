namespace GestionCommerciale.Modules.Auth.Services;

public interface IAuthService
{
    Task<bool> LoginAsync(string email, string password, CancellationToken cancellationToken = default);
    Task LogoutAsync(CancellationToken cancellationToken = default);
}
