using Avalonia.Controls;
using GestionCommerciale.Modules.Stock.Models;

namespace GestionCommerciale.Shared.Helpers;

public static class ProductAutoComplete
{
    public static AutoCompleteFilterPredicate<object?> ItemFilter { get; } = static (search, item) =>
    {
        if (item is not Produit p) return false;
        if (string.IsNullOrWhiteSpace(search)) return false;
        var q = search.Trim();
        static bool Match(string? s, string qq) =>
            !string.IsNullOrEmpty(s) && s.Contains(qq, StringComparison.OrdinalIgnoreCase);
        return Match(p.Reference, q) || Match(p.Designation, q) || Match(p.CodeBarre, q);
    };
}
