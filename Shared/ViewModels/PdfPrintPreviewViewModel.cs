using System.Collections.ObjectModel;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionCommerciale.Shared.Services;
using PdfiumViewer;
using DrawingImage = System.Drawing.Image;
using DrawingImageFormat = System.Drawing.Imaging.ImageFormat;

namespace GestionCommerciale.Shared.ViewModels;

public sealed partial class PdfPrintPreviewViewModel : ObservableObject, IDisposable
{
    private readonly PdfDocument _document;
    private readonly ILocaleService _locale;
    private readonly Func<CancellationToken, Task<bool>> _printWithSystemDialog;

    public event Action<bool>? CloseRequested;

    public PdfPrintPreviewViewModel(
        string pdfPath,
        string documentTitle,
        ILocaleService locale,
        Func<CancellationToken, Task<bool>> printWithSystemDialog)
    {
        _document = PdfDocument.Load(pdfPath);
        _locale = locale;
        _printWithSystemDialog = printWithSystemDialog;
        DocumentTitle = documentTitle;
        RefreshLocalizedLabels();
        _ = LoadPreviewPagesAsync();
    }

    public ObservableCollection<Bitmap> PreviewPages { get; } = [];

    [ObservableProperty] private string _documentTitle = string.Empty;
    [ObservableProperty] private string _titleLabel = string.Empty;
    [ObservableProperty] private string _btnCancel = string.Empty;
    [ObservableProperty] private string _btnPrint = string.Empty;
    [ObservableProperty] private string _btnZoomOut = string.Empty;
    [ObservableProperty] private string _btnZoomIn = string.Empty;
    [ObservableProperty] private bool _isLoadingPreview = true;
    [ObservableProperty] private bool _isPrinting;
    [ObservableProperty] private double _zoomScale = 1.0;

    public string ZoomLabel => $"{(int)Math.Round(ZoomScale * 100)} %";

    partial void OnZoomScaleChanged(double value) => OnPropertyChanged(nameof(ZoomLabel));

    private void RefreshLocalizedLabels()
    {
        TitleLabel = _locale.T("PrintPreview_Title");
        BtnCancel = _locale.T("Btn_Cancel");
        BtnPrint = _locale.T("Btn_Print");
        BtnZoomOut = _locale.T("PrintPreview_ZoomOut");
        BtnZoomIn = _locale.T("PrintPreview_ZoomIn");
    }

    private async Task LoadPreviewPagesAsync()
    {
        IsLoadingPreview = true;
        try
        {
            var pages = await Task.Run(RenderAllPages);
            PreviewPages.Clear();
            foreach (var page in pages)
                PreviewPages.Add(page);
        }
        finally
        {
            IsLoadingPreview = false;
        }
    }

    private IReadOnlyList<Bitmap> RenderAllPages()
    {
        const float dpi = 120f;
        var result = new List<Bitmap>(_document.PageCount);

        for (var i = 0; i < _document.PageCount; i++)
        {
            var size = _document.PageSizes[i];
            var width = Math.Max(1, (int)(size.Width / 72f * dpi));
            var height = Math.Max(1, (int)(size.Height / 72f * dpi));
            using DrawingImage rendered = _document.Render(i, width, height, dpi, dpi, false);
            result.Add(ToAvaloniaBitmap(rendered));
        }

        return result;
    }

    private static Bitmap ToAvaloniaBitmap(DrawingImage image)
    {
        using var ms = new MemoryStream();
        image.Save(ms, DrawingImageFormat.Png);
        ms.Position = 0;
        return new Bitmap(ms);
    }

    [RelayCommand]
    private void Cancel() => CloseRequested?.Invoke(false);

    [RelayCommand]
    private void ZoomIn() =>
        ZoomScale = Math.Min(2.5, Math.Round((ZoomScale + 0.15) * 100) / 100);

    [RelayCommand]
    private void ZoomOut() =>
        ZoomScale = Math.Max(0.35, Math.Round((ZoomScale - 0.15) * 100) / 100);

    [RelayCommand]
    private void ZoomReset() => ZoomScale = 1.0;

    [RelayCommand]
    private async Task PrintAsync(CancellationToken cancellationToken)
    {
        try
        {
            IsPrinting = true;
            if (await _printWithSystemDialog(cancellationToken))
                CloseRequested?.Invoke(true);
        }
        finally
        {
            IsPrinting = false;
        }
    }

    public void Dispose()
    {
        _document.Dispose();
        foreach (var page in PreviewPages)
            page.Dispose();
        PreviewPages.Clear();
    }
}
