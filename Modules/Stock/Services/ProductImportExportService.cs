using System.Globalization;
using System.Text;
using GestionCommerciale.Modules.Stock.Models;
using GestionCommerciale.Shared.Database;
using GestionCommerciale.Shared.Services;
using Microsoft.EntityFrameworkCore;

namespace GestionCommerciale.Modules.Stock.Services;

public sealed class ProductImportExportService : IProductImportExportService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly IStockMovementService _stock;
    private readonly ILocaleService _locale;

    public ProductImportExportService(
        IDbContextFactory<AppDbContext> dbFactory,
        IStockMovementService stock,
        ILocaleService locale)
    {
        _dbFactory = dbFactory;
        _stock = stock;
        _locale = locale;
    }

    public async Task<byte[]> ExportCsvAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        var products = await db.Produits
            .AsNoTracking()
            .Include(p => p.Categorie)
            .OrderBy(p => p.Reference)
            .ToListAsync(cancellationToken);

        var fr = CultureInfo.GetCultureInfo("fr-FR");
        var sb = new StringBuilder();
        sb.AppendLine("Reference;CodeBarre;Designation;Unite;PrixAchatHT;PrixVenteHT;TauxTVA;StockActuel;StockMinimum;Categorie;Actif");

        foreach (var p in products)
        {
            var cat = p.Categorie?.Nom ?? "";
            sb.AppendLine(
                $"{EscapeCsv(p.Reference)};{EscapeCsv(p.CodeBarre ?? "")};{EscapeCsv(p.Designation)};{EscapeCsv(p.Unite)};" +
                $"{p.PrixAchatHT.ToString("N2", fr)};{p.PrixVenteHT.ToString("N2", fr)};{p.TauxTVA.ToString("N2", fr)};" +
                $"{p.StockActuel.ToString("N2", fr)};{p.StockMinimum.ToString("N2", fr)};{EscapeCsv(cat)};{(p.Actif ? "Oui" : "Non")}");
        }

        var preamble = Encoding.UTF8.GetPreamble();
        var body = Encoding.UTF8.GetBytes(sb.ToString());
        return [.. preamble, .. body];
    }

    public async Task<(int Imported, int Updated, int Errors)> ImportCsvAsync(byte[] csvData, CancellationToken cancellationToken = default)
    {
        var text = Encoding.UTF8.GetString(csvData).TrimStart('\uFEFF');
        var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (lines.Length < 2) return (0, 0, 1);

        var fr = CultureInfo.GetCultureInfo("fr-FR");
        var imported = 0;
        var updated = 0;
        var errors = 0;
        var importNote = _locale.T("Stock_ImportNote");

        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        var categoryCache = await db.Categories.ToDictionaryAsync(c => c.Nom, c => c.Id, cancellationToken);
        var newProductOpeningStock = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

        for (var i = 1; i < lines.Length; i++)
        {
            try
            {
                var cols = SplitCsvLine(lines[i]);
                if (cols.Length < 11) { errors++; continue; }

                var reference = cols[0].Trim();
                if (string.IsNullOrWhiteSpace(reference)) { errors++; continue; }

                var codeBarre = cols[1].Trim();
                var designation = cols[2].Trim();
                var unite = cols[3].Trim();
                var prixAchatHt = decimal.Parse(cols[4].Trim(), NumberStyles.Any, fr);
                var prixVenteHt = decimal.Parse(cols[5].Trim(), NumberStyles.Any, fr);
                var tauxTva = decimal.Parse(cols[6].Trim(), NumberStyles.Any, fr);
                var stockActuel = decimal.Parse(cols[7].Trim(), NumberStyles.Any, fr);
                var stockMin = decimal.Parse(cols[8].Trim(), NumberStyles.Any, fr);
                var categorieNom = cols[9].Trim();
                var actif = cols[10].Trim().Equals("Oui", StringComparison.OrdinalIgnoreCase);

                int? categorieId = null;
                if (!string.IsNullOrWhiteSpace(categorieNom))
                {
                    if (!categoryCache.TryGetValue(categorieNom, out var cid))
                    {
                        var cat = new Categorie { Nom = categorieNom };
                        db.Categories.Add(cat);
                        await db.SaveChangesAsync(cancellationToken);
                        cid = cat.Id;
                        categoryCache[categorieNom] = cid;
                    }
                    categorieId = cid;
                }

                var existing = await db.Produits.FirstOrDefaultAsync(p => p.Reference == reference, cancellationToken);
                if (existing is not null)
                {
                    existing.CodeBarre = codeBarre;
                    existing.Designation = designation;
                    existing.Unite = unite;
                    existing.PrixAchatHT = prixAchatHt;
                    existing.PrixVenteHT = prixVenteHt;
                    existing.TauxTVA = tauxTva;
                    existing.StockMinimum = stockMin;
                    existing.CategorieId = categorieId;
                    existing.Actif = actif;

                    if (existing.StockActuel != stockActuel)
                    {
                        var delta = stockActuel - existing.StockActuel;
                        await _stock.ApplyMovementAsync(
                            db,
                            existing.Id,
                            TypeMouvement.Ajustement,
                            delta,
                            StockMovementService.OrigineTypeImport,
                            null,
                            importNote,
                            null,
                            cancellationToken);
                    }

                    updated++;
                }
                else
                {
                    db.Produits.Add(new Produit
                    {
                        Reference = reference,
                        CodeBarre = codeBarre,
                        Designation = designation,
                        Unite = unite,
                        PrixAchatHT = prixAchatHt,
                        PrixVenteHT = prixVenteHt,
                        TauxTVA = tauxTva,
                        StockActuel = 0,
                        StockMinimum = stockMin,
                        CategorieId = categorieId,
                        Actif = actif
                    });
                    if (stockActuel != 0)
                        newProductOpeningStock[reference] = stockActuel;
                    imported++;
                }
            }
            catch
            {
                errors++;
            }
        }

        await db.SaveChangesAsync(cancellationToken);

        foreach (var (reference, openingStock) in newProductOpeningStock)
        {
            var produit = await db.Produits.FirstAsync(p => p.Reference == reference, cancellationToken);
            await _stock.ApplyMovementAsync(
                db,
                produit.Id,
                TypeMouvement.Ajustement,
                openingStock,
                StockMovementService.OrigineTypeImport,
                null,
                importNote,
                null,
                cancellationToken);
        }

        if (newProductOpeningStock.Count > 0)
            await db.SaveChangesAsync(cancellationToken);

        return (imported, updated, errors);
    }

    private static string EscapeCsv(string value)
    {
        if (value.Contains(';') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }

    private static string[] SplitCsvLine(string line)
    {
        var result = new List<string>();
        var current = new StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var c = line[i];
            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ';' && !inQuotes)
            {
                result.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }
        result.Add(current.ToString());
        return [.. result];
    }
}
