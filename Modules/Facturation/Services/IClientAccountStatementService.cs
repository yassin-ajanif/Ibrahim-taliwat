namespace GestionCommerciale.Modules.Facturation.Services;

public interface IClientAccountStatementService
{
    Task<ClientAccountStatementResult> GetStatementAsync(int clientId, CancellationToken cancellationToken = default);
}
