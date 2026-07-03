namespace GestionCommerciale.Shared.Helpers;

public static class DocumentNumberKind
{
    public sealed record Entry(string Prefix, string LabelKey);

    public static readonly Entry[] All =
    [
        new("DEV", "Nav_Devis"),
        new("BCC", "Nav_BCC"),
        new("BL", "Nav_BL"),
        new("FAC", "Nav_Factures"),
        new("AVO", "Nav_Avoirs"),
        new("BC", "Nav_BC"),
        new("BR", "Nav_BR"),
        new("FAF", "Nav_FacturesFournisseur"),
        new("AVF", "Nav_AvoirFournisseur"),
    ];
}
