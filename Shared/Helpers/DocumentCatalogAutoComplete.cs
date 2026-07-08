using Avalonia.Controls;

namespace GestionCommerciale.Shared.Helpers;

/// <summary>
/// AutoComplete filter for server-side catalog search: items are already filtered by
/// <see cref="Services.ICatalogSearchService"/> — accept every result in the dropdown.
/// </summary>
public static class DocumentCatalogAutoComplete
{
    public static AutoCompleteFilterPredicate<object?> ItemFilter { get; } =
        static (_, item) => item is DocumentCatalogItem;
}
