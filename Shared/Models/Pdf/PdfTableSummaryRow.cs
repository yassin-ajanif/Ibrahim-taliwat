namespace GestionCommerciale.Shared.Models.Pdf;

/// <summary>Footer row inside the grid: first cell spans <see cref="LeadingSpan"/> columns, then one cell per entry in <see cref="Values"/>.</summary>
public sealed class PdfTableSummaryRow
{
    public int LeadingSpan { get; init; }
    public string Label { get; init; } = string.Empty;
    public IReadOnlyList<string> Values { get; init; } = Array.Empty<string>();
}
