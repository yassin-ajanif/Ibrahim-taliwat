namespace GestionCommerciale.Modules.Facturation.Services;

public interface IAvoirWorkflowService
{
    Task CreerEtValiderAsync(int avoirId, int? userId, CancellationToken cancellationToken = default);
}
