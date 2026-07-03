using GestionCommerciale.Modules.Facturation.Services;
using GestionCommerciale.Modules.Tiers.Models;
using GestionCommerciale.Shared.Models.Pdf;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;

namespace GestionCommerciale.Shared.Services.Pdf;

public static class ClientAccountStatementPdfRenderer
{
    private static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("fr-FR");
    private const string TextPrimary = "#111827";
    private const string TextMuted = "#6B7280";
    private const string TableHeaderBg = "#E5E7EB";
    private const string TableBorder = "#D1D5DB";
    private const string TableRowAlt = "#F9FAFB";
    private const float HeaderLogoWidth = 128f;
    private const float HeaderLogoHeight = 78f;
    private const float HeaderTitleFontSize = 17f;
    private const float HeaderCompanyFontSize = 16f;

    public static byte[] Render(
        string societeNom,
        string devise,
        Tiers client,
        DocumentPartyPdfInfo party,
        ClientAccountStatementResult statement,
        byte[]? logoBytes)
    {
        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.MarginHorizontal(36);
                page.MarginVertical(28);
                page.DefaultTextStyle(x => x.FontSize(9.5f).FontColor(TextPrimary));

                page.Header().Column(header =>
                {
                    header.Spacing(8);
                    header.Item().Row(row =>
                    {
                        if (logoBytes is { Length: > 0 })
                            row.ConstantItem(HeaderLogoWidth).Height(HeaderLogoHeight).Image(logoBytes).FitArea();
                        row.RelativeItem().AlignRight().Column(col =>
                        {
                            col.Item().Text(societeNom).Bold().FontSize(HeaderCompanyFontSize);
                            col.Item().Text("ETAT DES FACTURES ET REGLEMENTS").Bold().FontSize(HeaderTitleFontSize);
                        });
                    });
                    header.Item().PaddingTop(4).Row(row =>
                    {
                        row.RelativeItem();
                        row.ConstantItem(220).Border(1).BorderColor(TableBorder).Padding(8).Column(col =>
                        {
                            col.Item().Text("Client").Bold().FontSize(10);
                            col.Item().Text(party.Nom).FontSize(10);
                            if (!string.IsNullOrWhiteSpace(party.Adresse))
                                col.Item().Text(party.Adresse!).FontSize(9).FontColor(TextMuted);
                            if (!string.IsNullOrWhiteSpace(party.Ice))
                                col.Item().Text($"ICE : {party.Ice}").FontSize(9).FontColor(TextMuted);
                        });
                    });
                });

                page.Content().PaddingTop(12).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(62);
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(2);
                        columns.ConstantColumn(72);
                        columns.ConstantColumn(72);
                        columns.ConstantColumn(72);
                    });

                    table.Header(header =>
                    {
                        HeaderCell(header.Cell(), "DATE");
                        HeaderCell(header.Cell(), "Désignation");
                        HeaderCell(header.Cell(), "Observation");
                        HeaderCell(header.Cell(), "Montant Débit", alignRight: true);
                        HeaderCell(header.Cell(), "Montant Crédit", alignRight: true);
                        HeaderCell(header.Cell(), "SOLDE", alignRight: true);
                    });

                    var i = 0;
                    foreach (var row in statement.Rows)
                    {
                        var bg = i % 2 == 1 ? TableRowAlt : "#FFFFFF";
                        BodyCell(table.Cell().Background(bg), row.Date.ToString("dd/MM/yyyy", Culture));
                        BodyCell(table.Cell().Background(bg), row.Designation);
                        BodyCell(table.Cell().Background(bg), row.Observation);
                        BodyCell(table.Cell().Background(bg), row.Debit > 0 ? Fmt(row.Debit) : string.Empty, alignRight: true);
                        BodyCell(table.Cell().Background(bg), row.Credit > 0 ? Fmt(row.Credit) : string.Empty, alignRight: true);
                        BodyCell(table.Cell().Background(bg), Fmt(row.Balance), alignRight: true, bold: true);
                        i++;
                    }

                    table.Cell().ColumnSpan(5).Background(TableHeaderBg).Border(0.5f).BorderColor(TableBorder)
                        .Padding(6).AlignRight().Text("TOTAL").Bold();
                    table.Cell().Background(TableHeaderBg).Border(0.5f).BorderColor(TableBorder)
                        .Padding(6).AlignRight().Text(Fmt(statement.SoldeActuel)).Bold();
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Page ");
                    text.CurrentPageNumber();
                    text.Span(" / ");
                    text.TotalPages();
                });
            });
        });

        return doc.GeneratePdf();
    }

    private static string Fmt(decimal value) => value.ToString("N2", Culture);

    private static void HeaderCell(IContainer cell, string text, bool alignRight = false)
    {
        var c = cell.Background(TableHeaderBg).Border(0.5f).BorderColor(TableBorder).Padding(5);
        if (alignRight)
            c.AlignRight().Text(text).Bold().FontSize(8.5f);
        else
            c.Text(text).Bold().FontSize(8.5f);
    }

    private static void BodyCell(IContainer cell, string text, bool alignRight = false, bool bold = false)
    {
        var c = cell.Border(0.5f).BorderColor(TableBorder).Padding(5);
        if (alignRight)
            c = c.AlignRight();
        var t = c.Text(text).FontSize(8.5f);
        if (bold) t.Bold();
    }
}
