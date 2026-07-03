namespace GestionCommerciale.Shared.Models.Pdf;

/// <summary>PDF header logo sizing.</summary>
public sealed class PdfLogoLayoutOptions
{
    public const int FixedLogoWidthPx = 800;
    private const float ScreenDpi = 96f;
    private const float PdfPointsPerInch = 72f;

    public int WidthPx { get; init; } = FixedLogoWidthPx;

    public bool UsesManualPixelWidth => WidthPx > 0;

    public static PdfLogoLayoutOptions Default { get; } = new();

    public float ManualWidthPoints => WidthPx * PdfPointsPerInch / ScreenDpi;
}
