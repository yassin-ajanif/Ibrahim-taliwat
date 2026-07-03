namespace GestionCommerciale.Shared.Models.Pdf;

public sealed record PdfTableColumn(
    string Header,
    float RelativeWidth = 1f,
    PdfTextAlignment Align = PdfTextAlignment.Start);
