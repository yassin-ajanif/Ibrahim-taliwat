namespace GestionCommerciale.Shared.Helpers;

public static class DocumentListFormat
{
    public static string NotePreview(string? note)
    {
        var t = note?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(t)) return "—";
        return t.Length > 52 ? t[..49] + "…" : t;
    }
}
