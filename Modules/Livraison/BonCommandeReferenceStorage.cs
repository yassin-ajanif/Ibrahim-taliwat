namespace GestionCommerciale.Modules.Livraison;

internal static class BonCommandeReferenceStorage
{
    private const char Separator = '\u001E';

    public static (string BonCommandeReference, string UserNote) Parse(string storedNote)
    {
        if (string.IsNullOrEmpty(storedNote))
            return (string.Empty, string.Empty);

        var idx = storedNote.IndexOf(Separator);
        if (idx < 0)
            return (string.Empty, storedNote);

        return (storedNote[..idx], storedNote[(idx + 1)..]);
    }

    public static string Format(string bonCommandeReference, string userNote)
    {
        var bcc = bonCommandeReference.Trim();
        if (string.IsNullOrEmpty(bcc))
            return userNote;

        return string.IsNullOrEmpty(userNote)
            ? bcc + Separator
            : bcc + Separator + userNote;
    }

    public static string? ResolveForPdf(string storedNote)
    {
        var (bcc, _) = Parse(storedNote);
        return string.IsNullOrWhiteSpace(bcc) ? null : bcc.Trim();
    }
}
