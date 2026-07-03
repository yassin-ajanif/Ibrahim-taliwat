namespace GestionCommerciale.Shared.Services;

/// <summary>Immutable column visibility for document line PDF tables (matches UI checkboxes / ui-preferences.json).</summary>
public sealed record DocumentLineColumnVisibility(
    bool ShowReference = true,
    bool ShowDesignation = true,
    bool ShowQuantite = true,
    bool ShowConditionnement = true,
    bool ShowPuHt = true,
    bool ShowRemise = true,
    bool ShowTva = true,
    bool ShowMontantHt = true,
    bool ShowMontantTtc = true)
{
    public static DocumentLineColumnVisibility AllVisible { get; } = new();
}
