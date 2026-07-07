using Avalonia.Controls;

namespace GestionCommerciale.Shared.Helpers;

public static class DocumentCatalogAutoComplete
{
    public static AutoCompleteFilterPredicate<object?> ItemFilter { get; } = static (search, item) =>
    {
        if (item is not DocumentCatalogItem entry) return false;
        if (string.IsNullOrWhiteSpace(search)) return false;
        var q = search.Trim();
        static bool Match(string? s, string qq) =>
            !string.IsNullOrEmpty(s) && s.Contains(qq, StringComparison.OrdinalIgnoreCase);
        return Match(entry.Reference, q)
               || Match(entry.Designation, q)
               || Match(entry.CodeBarre, q);
    };
}
