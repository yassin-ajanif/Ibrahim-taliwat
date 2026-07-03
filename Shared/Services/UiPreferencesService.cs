using System.Text.Json;
using GestionCommerciale.Shared.Database;
using GestionCommerciale.Shared.ViewModels;

namespace GestionCommerciale.Shared.Services;

public sealed class UiPreferencesService : IUiPreferencesService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    private static readonly string PrefsPath =
        Path.Combine(DatabasePath.GetDirectory(), "ui-preferences.json");

    public void LoadDocumentLineColumns(string sectionKey, DocumentLineGridColumnState state)
    {
        var root = ReadRoot();
        if (!root.DocumentLineColumns.TryGetValue(sectionKey, out var c) || c == null)
            return;

        state.ShowReference = c.ShowReference;
        state.ShowDesignation = c.ShowDesignation;
        state.ShowQuantite = c.ShowQuantite;
        state.ShowConditionnement = c.ShowConditionnement;
        state.ShowPuHt = c.ShowPuHt;
        state.ShowRemise = c.ShowRemise;
        state.ShowTva = c.ShowTva;
        state.ShowMontantHt = c.ShowMontantHt;
        state.ShowMontantTtc = c.ShowMontantTtc;
    }

    public DocumentLineColumnVisibility GetDocumentLineColumnVisibility(string sectionKey)
    {
        var root = ReadRoot();
        if (!root.DocumentLineColumns.TryGetValue(sectionKey, out var c) || c == null)
            return DocumentLineColumnVisibility.AllVisible;

        return new DocumentLineColumnVisibility(
            c.ShowReference,
            c.ShowDesignation,
            c.ShowQuantite,
            c.ShowConditionnement,
            c.ShowPuHt,
            c.ShowRemise,
            c.ShowTva,
            c.ShowMontantHt,
            c.ShowMontantTtc);
    }

    public void SaveDocumentLineColumns(string sectionKey, DocumentLineGridColumnState state)
    {
        var root = ReadRoot();
        root.Version = 1;
        root.DocumentLineColumns[sectionKey] = new DocumentLineColumnsPrefs
        {
            ShowReference = state.ShowReference,
            ShowDesignation = state.ShowDesignation,
            ShowQuantite = state.ShowQuantite,
            ShowConditionnement = state.ShowConditionnement,
            ShowPuHt = state.ShowPuHt,
            ShowRemise = state.ShowRemise,
            ShowTva = state.ShowTva,
            ShowMontantHt = state.ShowMontantHt,
            ShowMontantTtc = state.ShowMontantTtc
        };

        WriteRoot(root);
    }

    private static UiPreferencesRoot ReadRoot()
    {
        try
        {
            if (!File.Exists(PrefsPath))
                return new UiPreferencesRoot();

            var json = File.ReadAllText(PrefsPath);
            if (string.IsNullOrWhiteSpace(json))
                return new UiPreferencesRoot();

            return JsonSerializer.Deserialize<UiPreferencesRoot>(json, JsonOptions) ?? new UiPreferencesRoot();
        }
        catch
        {
            return new UiPreferencesRoot();
        }
    }

    private static void WriteRoot(UiPreferencesRoot root)
    {
        try
        {
            var dir = Path.GetDirectoryName(PrefsPath);
            if (!string.IsNullOrWhiteSpace(dir))
                Directory.CreateDirectory(dir);

            var json = JsonSerializer.Serialize(root, JsonOptions);
            File.WriteAllText(PrefsPath, json);
        }
        catch
        {
            // Preference persistence should never block user flow.
        }
    }

    private sealed class UiPreferencesRoot
    {
        public int Version { get; set; } = 1;
        public Dictionary<string, DocumentLineColumnsPrefs> DocumentLineColumns { get; set; } = new();
    }

    private sealed class DocumentLineColumnsPrefs
    {
        public bool ShowReference { get; set; } = true;
        public bool ShowDesignation { get; set; } = true;
        public bool ShowQuantite { get; set; } = true;
        public bool ShowConditionnement { get; set; } = true;
        public bool ShowPuHt { get; set; } = true;
        public bool ShowRemise { get; set; } = true;
        public bool ShowTva { get; set; } = true;
        public bool ShowMontantHt { get; set; } = true;
        public bool ShowMontantTtc { get; set; } = true;
    }
}
