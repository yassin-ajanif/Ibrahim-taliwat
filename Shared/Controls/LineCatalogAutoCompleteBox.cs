using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Utils;
using GestionCommerciale.Modules.CommandeFournisseur.ViewModels;
using GestionCommerciale.Modules.Devis.ViewModels;
using GestionCommerciale.Modules.Facturation.ViewModels;
using GestionCommerciale.Modules.Livraison.ViewModels;
using GestionCommerciale.Modules.Stock.Models;

namespace GestionCommerciale.Shared.Controls;

/// <summary>
/// AutoComplete for document lines: filters while typing but applies catalog data only after an explicit pick (click / Enter), not on exact text match.
/// </summary>
public class LineCatalogAutoCompleteBox : AutoCompleteBox
{
    private const string PartSelectingItemsControl = "PART_SelectingItemsControl";

    public event EventHandler<Produit>? CatalogProductCommitted;

    protected override ISelectionAdapter? GetSelectionAdapterPart(INameScope nameScope)
    {
        if (!LineCatalogAutoComplete.GetEnableExplicitLinePick(this))
            return base.GetSelectionAdapterPart(nameScope);

        SelectingItemsControl? selector = nameScope.Find<SelectingItemsControl>(PartSelectingItemsControl);
        if (selector is null)
            return base.GetSelectionAdapterPart(nameScope);
        if (selector is ISelectionAdapter existingAdapter)
            return existingAdapter;

        return new LineCatalogSelectionAdapter(this, selector);
    }

    protected override void OnPopulated(PopulatedEventArgs e)
    {
        base.OnPopulated(e);
    }

    protected override void OnSelectionChanged(SelectionChangedEventArgs e)
    {
        base.OnSelectionChanged(e);
    }

    private sealed class LineCatalogSelectionAdapter : SelectingItemsControlSelectionAdapter
    {
        private readonly LineCatalogAutoCompleteBox _owner;

        public LineCatalogSelectionAdapter(LineCatalogAutoCompleteBox owner, SelectingItemsControl selector)
            : base(selector)
        {
            _owner = owner;
        }

        protected override void OnCommit()
        {
            if (SelectorControl?.SelectedItem is Produit p)
                _owner.CatalogProductCommitted?.Invoke(_owner, p);
            base.OnCommit();
        }
    }
}

public static class LineCatalogAutoComplete
{
    public static readonly AttachedProperty<bool> EnableExplicitLinePickProperty =
        AvaloniaProperty.RegisterAttached<LineCatalogAutoCompleteBox, bool>(
            "EnableExplicitLinePick",
            typeof(LineCatalogAutoComplete));

    public static void SetEnableExplicitLinePick(LineCatalogAutoCompleteBox box, bool value) =>
        box.SetValue(EnableExplicitLinePickProperty, value);

    public static bool GetEnableExplicitLinePick(LineCatalogAutoCompleteBox box) =>
        box.GetValue(EnableExplicitLinePickProperty);

    static LineCatalogAutoComplete()
    {
        EnableExplicitLinePickProperty.Changed.AddClassHandler<LineCatalogAutoCompleteBox>(OnEnableExplicitLinePickChanged);
    }

    private static void OnEnableExplicitLinePickChanged(LineCatalogAutoCompleteBox box, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is true)
            box.CatalogProductCommitted += OnCatalogProductCommitted;
        else
            box.CatalogProductCommitted -= OnCatalogProductCommitted;
    }

    private static void OnCatalogProductCommitted(object? sender, Produit p)
    {
        if (sender is not LineCatalogAutoCompleteBox acb) return;
        switch (acb.DataContext)
        {
            case DevisLineRow d:
                d.ApplyCatalogProduct(p);
                break;
            case FactureLineRow f:
                f.ApplyCatalogProduct(p);
                break;
            case BCLineRow b:
                b.ApplyCatalogProduct(p);
                break;
            case BLLineRow bl:
                bl.ApplyCatalogProduct(p);
                break;
        }
    }
}
