namespace GestionCommerciale.Shared.Services;

public interface IDocumentNumberService
{
    Task<string> NextDevisAsync(CancellationToken cancellationToken = default);
    Task<string> NextBLAsync(CancellationToken cancellationToken = default);
    Task<string> NextBRAsync(CancellationToken cancellationToken = default);
    Task<string> NextBCAsync(CancellationToken cancellationToken = default);
    Task<string> NextBCClientAsync(CancellationToken cancellationToken = default);
    Task<string> NextFactureAsync(CancellationToken cancellationToken = default);
    Task<string> NextFactureFournisseurAsync(CancellationToken cancellationToken = default);
    Task<string> NextAvoirAsync(CancellationToken cancellationToken = default);
    Task<string> NextAvoirFournisseurAsync(CancellationToken cancellationToken = default);
}
