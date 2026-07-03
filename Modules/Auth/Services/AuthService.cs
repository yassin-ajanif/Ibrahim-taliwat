using GestionCommerciale.Shared.Database;

namespace GestionCommerciale.Modules.Auth.Services;

public sealed class AuthService : IAuthService
{
    private readonly ICurrentUserSession _session;

    public AuthService(ICurrentUserSession session) => _session = session;

    public Task<bool> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email) || password is null)
        {
            _session.Clear();
            return Task.FromResult(false);
        }

        var ok = email.Trim().Equals(DbSeeder.DefaultAdminEmail, StringComparison.OrdinalIgnoreCase)
                 && password == DbSeeder.DefaultAdminPassword;
        if (ok)
            _session.SetDefaultAdminSession();
        else
            _session.Clear();
        return Task.FromResult(ok);
    }

    public Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        _session.Clear();
        return Task.CompletedTask;
    }
}
