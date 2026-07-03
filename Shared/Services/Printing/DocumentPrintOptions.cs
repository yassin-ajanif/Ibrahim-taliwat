namespace GestionCommerciale.Shared.Services.Printing;

public sealed class DocumentPrintOptions
{
    public required string PrinterName { get; init; }
    public int FromPage { get; init; } = 1;
    public int ToPage { get; init; } = 1;
    public string? PaperName { get; init; }
    public bool Color { get; init; } = true;
}
