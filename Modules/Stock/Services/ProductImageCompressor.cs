using SkiaSharp;

namespace GestionCommerciale.Modules.Stock.Services;

public static class ProductImageCompressor
{
    public const int MaxEdgePixels = 1024;
    /// <summary>JPEG quality 0–100 (Skia).</summary>
    public const int JpegQuality = 78;

    /// <summary>Load an image file, downscale if needed, encode as JPEG.</summary>
    public static byte[] CompressFileToJpeg(string path)
    {
        using var original = SKBitmap.Decode(path)
            ?? throw new InvalidOperationException("Impossible de décoder l'image.");

        var w = original.Width;
        var h = original.Height;
        if (w <= MaxEdgePixels && h <= MaxEdgePixels)
            return EncodeJpeg(original);

        var scale = Math.Min((float)MaxEdgePixels / w, (float)MaxEdgePixels / h);
        var nw = Math.Max(1, (int)(w * scale));
        var nh = Math.Max(1, (int)(h * scale));
        using var scaled = original.Resize(new SKImageInfo(nw, nh), SKFilterQuality.Medium)
            ?? throw new InvalidOperationException("Impossible de redimensionner l'image.");

        return EncodeJpeg(scaled);
    }

    private static byte[] EncodeJpeg(SKBitmap bitmap)
    {
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, JpegQuality);
        if (data == null)
            throw new InvalidOperationException("Impossible d'encoder en JPEG.");
        return data.ToArray();
    }
}
