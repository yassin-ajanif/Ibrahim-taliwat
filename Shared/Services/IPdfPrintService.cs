namespace GestionCommerciale.Shared.Services;

public interface IPdfPrintService
{
    Task PrintPdfAsync(byte[] pdfBytes, string documentTitle, CancellationToken cancellationToken = default);
}
