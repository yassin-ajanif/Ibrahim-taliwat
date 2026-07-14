using System.Collections.ObjectModel;
using Avalonia.Threading;
using GestionCommerciale.Shared.Services;

namespace GestionCommerciale.Shared.Helpers;

/// <summary>Debounced DB catalog search for the "Ajouter un article" autocomplete.</summary>
public sealed class AddLineCatalogSearchCoordinator
{
    private readonly ICatalogSearchService _search;
    private CancellationTokenSource? _cts;
    private int _generation;

    public AddLineCatalogSearchCoordinator(ICatalogSearchService search) => _search = search;

    public ObservableCollection<DocumentCatalogItem> Results { get; } = [];

    public void Clear()
    {
        CancelSearch();
        // Never clear Results on the same call stack as an AutoCompleteBox selection
        // change — Avalonia crashes with ArgumentOutOfRangeException in SelectionModel.
        PostClearResults();
    }

    /// <summary>
    /// Clears pick + search text after a catalog selection, once AutoCompleteBox finishes committing.
    /// Results are cleared on a later dispatcher turn so SelectionModel is not mid-update.
    /// </summary>
    public void ResetAfterPick(Action resetSearchField, Action? onCompleted = null)
    {
        CancelSearch();
        Dispatcher.UIThread.Post(() =>
        {
            resetSearchField();
            // Second hop: SelectedItem/Text bindings must apply before ItemsSource Clear.
            // Clearing in the same Post as SelectedItem=null still crashes AutoCompleteBox.
            Dispatcher.UIThread.Post(() =>
            {
                if (Results.Count > 0)
                    Results.Clear();
                onCompleted?.Invoke();
            }, DispatcherPriority.Background);
        }, DispatcherPriority.Background);
    }

    public void QueueSearch(string? text)
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();
        _ = RunSearchAsync(text, Interlocked.Increment(ref _generation), _cts.Token);
    }

    private void CancelSearch()
    {
        Interlocked.Increment(ref _generation);
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
    }

    private void PostClearResults()
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (Results.Count > 0)
                Results.Clear();
        }, DispatcherPriority.Background);
    }

    private async Task RunSearchAsync(string? text, int generation, CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(100, cancellationToken);
            if (string.IsNullOrWhiteSpace(text))
            {
                if (generation == _generation)
                {
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        if (generation == _generation && Results.Count > 0)
                            Results.Clear();
                    });
                }
                return;
            }

            var items = await _search.SearchCatalogAsync(text, cancellationToken: cancellationToken);
            if (generation != _generation || cancellationToken.IsCancellationRequested)
                return;

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (generation != _generation || cancellationToken.IsCancellationRequested)
                    return;
                Results.Clear();
                foreach (var item in items)
                    Results.Add(item);
            });
        }
        catch (OperationCanceledException) { }
    }
}
