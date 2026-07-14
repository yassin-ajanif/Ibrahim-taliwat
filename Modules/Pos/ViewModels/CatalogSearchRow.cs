using Avalonia.Media.Imaging;
using GestionCommerciale.Shared.Helpers;

namespace GestionCommerciale.Modules.Pos.ViewModels;

public sealed class CatalogSearchRow : IDisposable
{
    public DocumentCatalogItem Item { get; }

    public CatalogSearchRow(DocumentCatalogItem item)
    {
        Item = item;
        Thumbnail = CreateThumbnail(item.ImageData);
    }

    public string Reference => Item.Reference;
    public string Designation => Item.Designation;
    public string? CodeBarre => Item.CodeBarre;
    public bool IsService => Item.Kind == DocumentCatalogKind.Service;
    public decimal PrixVenteTtc => Item.PrixVenteHT * (1 + Item.TauxTVA / 100m);
    public Bitmap? Thumbnail { get; }
    public bool HasImage => Thumbnail is not null;

    public void Dispose() => Thumbnail?.Dispose();

    private static Bitmap? CreateThumbnail(byte[]? bytes)
    {
        if (bytes is null || bytes.Length == 0)
            return null;

        try
        {
            using var ms = new MemoryStream(bytes);
            return new Bitmap(ms);
        }
        catch
        {
            return null;
        }
    }
}
