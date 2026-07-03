namespace GestionCommerciale.Shared.Services;

public interface IProductImportExportService
{
    Task<byte[]> ExportCsvAsync(CancellationToken cancellationToken = default);
    Task<(int Imported, int Updated, int Errors)> ImportCsvAsync(byte[] csvData, CancellationToken cancellationToken = default);
}
