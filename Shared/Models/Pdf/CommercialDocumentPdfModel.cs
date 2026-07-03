namespace GestionCommerciale.Shared.Models.Pdf;

public sealed class CommercialDocumentPdfModel
{
    public string CompanyName { get; init; } = string.Empty;
    /// <summary>Shown under the company name in the header (e.g. FACTURE, DEVIS).</summary>
    public string DocumentKindLabel { get; init; } = string.Empty;
    public IReadOnlyList<PdfKeyValueLine> DocumentInfoLines { get; init; } = Array.Empty<PdfKeyValueLine>();
    public IReadOnlyList<PdfKeyValueLine> PartyInfoLines { get; init; } = Array.Empty<PdfKeyValueLine>();
    public IReadOnlyList<PdfTableColumn> Columns { get; init; } = Array.Empty<PdfTableColumn>();
    public IReadOnlyList<IReadOnlyList<string>> Rows { get; init; } = Array.Empty<IReadOnlyList<string>>();
    public PdfTableSummaryRow? SummaryRow { get; init; }
    public decimal TotalHt { get; init; }
    public decimal TotalTva { get; init; }
    public decimal TotalTtc { get; init; }
    /// <summary>When false (Montant TTC column hidden in UI), totals box shows only HT; amount-in-words uses HT.</summary>
    public bool ShowTaxAndTtcInTotalsBox { get; init; } = true;
    public string Devise { get; init; } = "MAD";
    public string? AmountInWords { get; init; }
    public string? Note { get; init; }
    public IReadOnlyList<PdfKeyValueLine> ConditionLines { get; init; } = Array.Empty<PdfKeyValueLine>();
    public IReadOnlyList<string> FooterLines { get; init; } = Array.Empty<string>();
}
