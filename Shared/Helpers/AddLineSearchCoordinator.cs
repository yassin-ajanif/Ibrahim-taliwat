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
        Interlocked.Increment(ref _generation);
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
        if (Dispatcher.UIThread.CheckAccess())
            Results.Clear();
        else
            Dispatcher.UIThread.Post(Results.Clear);
    }

    public void QueueSearch(string? text)
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();
        _ = RunSearchAsync(text, Interlocked.Increment(ref _generation), _cts.Token);
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
                        if (generation == _generation) Results.Clear();
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
