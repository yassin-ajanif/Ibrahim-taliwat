using GestionCommerciale.Shared.ViewModels;

namespace GestionCommerciale.Shared.Services;

public interface IUiPreferencesService
{
    void LoadDocumentLineColumns(string sectionKey, DocumentLineGridColumnState state);
    void SaveDocumentLineColumns(string sectionKey, DocumentLineGridColumnState state);

    /// <summary>Reads persisted column visibility for PDF/export; all <c>true</c> when the section is missing.</summary>
    DocumentLineColumnVisibility GetDocumentLineColumnVisibility(string sectionKey);
}
