namespace GestionCommerciale.Shared.Services;

public interface IDialogService
{
    Task ShowInfoAsync(string title, string message, CancellationToken cancellationToken = default, int autoCloseMs = 0);
    Task ShowErrorAsync(string title, string message, CancellationToken cancellationToken = default);
    Task<bool> ConfirmAsync(string title, string message, CancellationToken cancellationToken = default);
    Task<string?> PromptPasswordAsync(string title, string message, CancellationToken cancellationToken = default);
    Task<string?> PickOpenFileAsync(string title, IReadOnlyList<string> patterns, CancellationToken cancellationToken = default);
    Task<string?> PickFolderAsync(CancellationToken cancellationToken = default);
    Task<string?> PickSaveFileAsync(string title, string suggestedFileName, IReadOnlyList<string> patterns, CancellationToken cancellationToken = default);
    Task<bool> SavePickedFileBytesAsync(string title, string suggestedFileName, IReadOnlyList<string> patterns, byte[] content, CancellationToken cancellationToken = default);
    Task<string?> PromptLicenseAsync(string title, string message, CancellationToken cancellationToken = default);
    Task<string?> ShowPromptAsync(string title, string message, CancellationToken cancellationToken = default);
    Task<(DateTime from, DateTime to)?> PickDateRangeAsync(string title, CancellationToken cancellationToken = default);
    Task<List<int>?> ShowBlPickerAsync(string title, IReadOnlyList<(int Id, string Numero, DateTime Date, string MontantLabel)> availableBls, CancellationToken cancellationToken = default);
    Task<List<int>?> ShowBrPickerAsync(string title, IReadOnlyList<(int Id, string Numero, DateTime Date, string MontantLabel)> availableBrs, CancellationToken cancellationToken = default);
}
