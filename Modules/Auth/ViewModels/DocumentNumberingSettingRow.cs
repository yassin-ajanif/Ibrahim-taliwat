using CommunityToolkit.Mvvm.ComponentModel;
using GestionCommerciale.Shared.Helpers;

namespace GestionCommerciale.Modules.Auth.ViewModels;

public partial class DocumentNumberingSettingRow : ObservableObject
{
    public string Prefix { get; init; } = string.Empty;
    [ObservableProperty] private string _documentLabel = string.Empty;
    public int NumberingYear { get; init; }
    public int DbMaxSequence { get; init; }

    [ObservableProperty] private int _lastUsedOutside;

    public string NextPreview => NumberingHelper.Generate(Prefix, Math.Max(DbMaxSequence, LastUsedOutside), NumberingYear);

    partial void OnLastUsedOutsideChanged(int value) => OnPropertyChanged(nameof(NextPreview));
}
