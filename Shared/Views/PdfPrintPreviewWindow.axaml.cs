using Avalonia.Controls;
using GestionCommerciale.Shared.ViewModels;

namespace GestionCommerciale.Shared.Views;

public partial class PdfPrintPreviewWindow : Window
{
    public PdfPrintPreviewWindow()
    {
        InitializeComponent();
        Closing += (_, e) =>
        {
            if (DataContext is PdfPrintPreviewViewModel vm)
                vm.Dispose();
        };
    }
}
