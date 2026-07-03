using GestionCommerciale.Shared.Services.Printing;

namespace GestionCommerciale.Shared.Services;

internal static class WindowsPdfPrintHost
{
    public static async Task PrintAsync(string pdfPath, string documentTitle, CancellationToken cancellationToken = default)
    {
        var result = await WindowsNativePdfPrinter.PrintAsync(pdfPath, documentTitle, cancellationToken);

        if (result.CancelledByUser)
            return;

        if (!result.Success)
            throw new InvalidOperationException(result.ErrorMessage ?? "L'impression a échoué.");
    }
}
