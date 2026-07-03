using System.Drawing.Printing;
using PdfiumViewer;
using WinFormsDialogResult = System.Windows.Forms.DialogResult;
using WinFormsPrintDialog = System.Windows.Forms.PrintDialog;

namespace GestionCommerciale.Shared.Services.Printing;

/// <summary>
/// Prints a PDF via PdfiumViewer and the native Windows print dialog (PrintDlgEx).
/// </summary>
public static class WindowsNativePdfPrinter
{
    public sealed record PrintResult(bool Success, bool CancelledByUser, string? ErrorMessage);

    public static Task<PrintResult> PrintAsync(
        string pdfPath,
        string documentTitle,
        CancellationToken cancellationToken = default) =>
        RunOnStaAsync(() => PrintWithSystemDialog(pdfPath, documentTitle), cancellationToken);

    public static Task<PrintResult> PrintWithSettingsAsync(
        string pdfPath,
        string documentTitle,
        DocumentPrintOptions settings,
        CancellationToken cancellationToken = default) =>
        RunOnStaAsync(() => PrintWithSettings(pdfPath, documentTitle, settings), cancellationToken);

    private static PrintResult PrintWithSystemDialog(string pdfPath, string documentTitle)
    {
        var fullPath = ValidatePdfPath(pdfPath);

        using var document = PdfDocument.Load(fullPath);
        using var printDocument = document.CreatePrintDocument(PdfPrintMode.ShrinkToMargin);
        printDocument.DocumentName = NormalizeTitle(documentTitle);

        using var dialog = new WinFormsPrintDialog
        {
            AllowSomePages = true,
            AllowSelection = false,
            UseEXDialog = true,
            Document = printDocument
        };

        if (dialog.ShowDialog() != WinFormsDialogResult.OK)
            return new PrintResult(Success: false, CancelledByUser: true, ErrorMessage: null);

        printDocument.Print();
        return new PrintResult(Success: true, CancelledByUser: false, ErrorMessage: null);
    }

    private static PrintResult PrintWithSettings(string pdfPath, string documentTitle, DocumentPrintOptions settings)
    {
        var fullPath = ValidatePdfPath(pdfPath);

        using var document = PdfDocument.Load(fullPath);
        using var printDocument = document.CreatePrintDocument(PdfPrintMode.ShrinkToMargin);
        printDocument.DocumentName = NormalizeTitle(documentTitle);

        var printer = new PrinterSettings
        {
            PrinterName = settings.PrinterName,
            FromPage = Math.Max(1, settings.FromPage),
            ToPage = Math.Max(settings.FromPage, settings.ToPage),
            PrintRange = PrintRange.SomePages
        };

        if (settings.FromPage <= 1 && settings.ToPage >= document.PageCount)
        {
            printer.PrintRange = PrintRange.AllPages;
        }
        else
        {
            printer.PrintRange = PrintRange.SomePages;
            printer.FromPage = Math.Max(1, settings.FromPage);
            printer.ToPage = Math.Min(document.PageCount, settings.ToPage);
        }

        printer.DefaultPageSettings.Color = settings.Color;

        if (!string.IsNullOrWhiteSpace(settings.PaperName))
        {
            foreach (PaperSize paper in printer.PaperSizes)
            {
                if (paper.PaperName.Equals(settings.PaperName, StringComparison.OrdinalIgnoreCase))
                {
                    printer.DefaultPageSettings.PaperSize = paper;
                    break;
                }
            }
        }

        printDocument.PrinterSettings = printer;
        printDocument.DefaultPageSettings = printer.DefaultPageSettings;
        printDocument.Print();

        return new PrintResult(Success: true, CancelledByUser: false, ErrorMessage: null);
    }

    private static string ValidatePdfPath(string pdfPath)
    {
        var fullPath = Path.GetFullPath(pdfPath);
        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"Le fichier PDF est introuvable : {fullPath}", fullPath);

        if (!fullPath.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Seuls les fichiers PDF peuvent être imprimés.");

        return fullPath;
    }

    private static string NormalizeTitle(string documentTitle) =>
        string.IsNullOrWhiteSpace(documentTitle) ? "Document" : documentTitle;

    private static PrintResult Fail(string message) =>
        new(Success: false, CancelledByUser: false, ErrorMessage: message);

    private static Task<PrintResult> RunOnStaAsync(Func<PrintResult> action, CancellationToken cancellationToken)
    {
        var tcs = new TaskCompletionSource<PrintResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        cancellationToken.Register(() => tcs.TrySetCanceled(cancellationToken));

        var thread = new Thread(() =>
        {
            try
            {
                tcs.TrySetResult(action());
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }
        })
        {
            IsBackground = true,
            Name = "PdfPrintDialog"
        };
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();

        return tcs.Task;
    }
}
