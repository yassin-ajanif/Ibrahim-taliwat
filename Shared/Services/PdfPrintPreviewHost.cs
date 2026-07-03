using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using GestionCommerciale.Shared.Services.Printing;
using GestionCommerciale.Shared.ViewModels;
using GestionCommerciale.Shared.Views;

namespace GestionCommerciale.Shared.Services;

internal static class PdfPrintPreviewHost
{
    public static async Task ShowAsync(
        string pdfPath,
        string documentTitle,
        ILocaleService locale,
        CancellationToken cancellationToken = default)
    {
        var owner = Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
            ? desktop.MainWindow
            : null;

        var window = new PdfPrintPreviewWindow();
        var vm = new PdfPrintPreviewViewModel(
            pdfPath,
            documentTitle,
            locale,
            async ct =>
            {
                var result = await WindowsNativePdfPrinter.PrintAsync(pdfPath, documentTitle, ct);
                if (!result.Success && !result.CancelledByUser)
                    throw new InvalidOperationException(result.ErrorMessage ?? "L'impression a échoué.");
                return result.Success;
            });

        var closed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        vm.CloseRequested += _ =>
        {
            closed.TrySetResult();
            if (window.IsVisible)
                window.Close();
        };
        window.Closed += (_, _) => closed.TrySetResult();
        window.DataContext = vm;

        if (owner != null)
            await window.ShowDialog(owner);
        else
            window.Show();

        await closed.Task.WaitAsync(cancellationToken);
    }
}
