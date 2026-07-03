namespace GestionCommerciale.Shared.Helpers;

public static class DocumentNumberingHelper
{
    public static int GetMaxSequenceFromNumeros(IEnumerable<string> numeros, string prefix, int year)
    {
        var last = 0;
        var prefixYear = $"{prefix}-{year}-";
        foreach (var n in numeros)
        {
            if (string.IsNullOrEmpty(n) || !n.StartsWith(prefixYear, StringComparison.Ordinal))
                continue;

            var tail = n[prefixYear.Length..];
            if (int.TryParse(tail, out var num) && num > last)
                last = num;
        }

        return last;
    }

    public static int ResolveNextSequence(int maxInDatabase, int lastUsedOutside) =>
        Math.Max(maxInDatabase, Math.Max(0, lastUsedOutside)) + 1;
}
