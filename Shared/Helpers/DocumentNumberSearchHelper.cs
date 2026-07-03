using System.Globalization;

namespace GestionCommerciale.Shared.Helpers;

/// <summary>
/// Shared list search: text matches numero substring + party name; digit-only terms match hyphen segments by integer (e.g. 1 → …-0001).
/// </summary>
public static class DocumentNumberSearchHelper
{
    public const int ResultCap = 300;
    public const int NumericScanCap = 5000;

    public static bool IsNumericSearchTerm(string term) =>
        term.Length > 0
        && term.All(char.IsAsciiDigit)
        && int.TryParse(term, NumberStyles.Integer, CultureInfo.InvariantCulture, out _);

    /// <summary>
    /// Digit-only queries match hyphen-separated parts as integers without requiring leading zeros.
    /// Single-digit terms do not use substring match on <paramref name="numero"/> so "1" does not match …-0061.
    /// </summary>
    public static bool MatchesNumeroAndParty(string numero, string partyNom, string term)
    {
        var ord = StringComparison.OrdinalIgnoreCase;
        if (partyNom.Contains(term, ord))
            return true;

        if (term.All(char.IsAsciiDigit)
            && int.TryParse(term, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n))
        {
            if (NumeroHasSegmentValue(numero, n))
                return true;
            if (term.Length >= 2 && numero.Contains(term, ord))
                return true;
            return false;
        }

        return numero.Contains(term, ord);
    }

    private static bool NumeroHasSegmentValue(string numero, int n)
    {
        foreach (var part in numero.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (int.TryParse(part, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v) && v == n)
                return true;
        }

        return false;
    }
}
