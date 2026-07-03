namespace GestionCommerciale.Shared.Helpers;

public static class DocumentListFilter
{
    public static bool Matches(string? searchText, params string?[] haystacks)
    {
        var q = searchText?.Trim();
        if (string.IsNullOrEmpty(q)) return true;
        foreach (var h in haystacks)
        {
            if (!string.IsNullOrEmpty(h) && h.Contains(q, StringComparison.CurrentCultureIgnoreCase))
                return true;
        }
        return false;
    }
}
