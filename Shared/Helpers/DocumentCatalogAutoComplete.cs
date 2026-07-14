using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;

namespace GestionCommerciale.Shared.Helpers;

/// <summary>
/// AutoComplete filter for server-side catalog search: items are already filtered by
/// <see cref="Services.ICatalogSearchService"/> — accept every result in the dropdown.
/// Clears leftover pick text (Avalonia keeps ValueMember text after SelectedItem is nulled).
/// </summary>
public static class DocumentCatalogAutoComplete
{
    public static AutoCompleteFilterPredicate<object?> ItemFilter { get; } =
        static (_, item) => item is DocumentCatalogItem;

    public static readonly AttachedProperty<bool> ClearTextAfterPickProperty =
        AvaloniaProperty.RegisterAttached<AutoCompleteBox, bool>(
            "ClearTextAfterPick",
            typeof(DocumentCatalogAutoComplete));

    public static void SetClearTextAfterPick(AutoCompleteBox element, bool value) =>
        element.SetValue(ClearTextAfterPickProperty, value);

    public static bool GetClearTextAfterPick(AutoCompleteBox element) =>
        element.GetValue(ClearTextAfterPickProperty);

    static DocumentCatalogAutoComplete()
    {
        ClearTextAfterPickProperty.Changed.AddClassHandler<AutoCompleteBox>(OnClearTextAfterPickChanged);
    }

    private static void OnClearTextAfterPickChanged(AutoCompleteBox box, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is true)
        {
            box.PropertyChanged += OnBoxPropertyChanged;
            box.GotFocus += OnBoxGotFocus;
        }
        else
        {
            box.PropertyChanged -= OnBoxPropertyChanged;
            box.GotFocus -= OnBoxGotFocus;
        }
    }

    private static void OnBoxPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (sender is not AutoCompleteBox box) return;
        if (e.Property != AutoCompleteBox.SelectedItemProperty || e.NewValue is not null)
            return;

        Dispatcher.UIThread.Post(() => ClearText(box), DispatcherPriority.Background);
    }

    private static void OnBoxGotFocus(object? sender, GotFocusEventArgs e)
    {
        if (sender is not AutoCompleteBox box) return;
        ClearText(box);
    }

    private static void ClearText(AutoCompleteBox box)
    {
        if (box.SelectedItem is not null) return;
        box.SetCurrentValue(AutoCompleteBox.TextProperty, string.Empty);
        box.Text = string.Empty;
    }
}
