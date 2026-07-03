using System.ComponentModel;
using Avalonia.Controls;
using GestionCommerciale.Shared.Helpers;
using GestionCommerciale.Shared.Services;
using Microsoft.Extensions.DependencyInjection;

namespace GestionCommerciale.Shared.Views;

public partial class ListPaginationBar : UserControl
{
    private PaginationHelper? _pagination;
    private ILocaleService? _locale;

    public ListPaginationBar()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        DataContextChanged += OnDataContextChanged;
    }

    private void OnLoaded(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        _locale = App.Services.GetRequiredService<ILocaleService>();
        ApplyLocale();
        _locale.CultureApplied += (_, _) => ApplyLocale();
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (_pagination is not null)
            _pagination.PropertyChanged -= OnPaginationPropertyChanged;

        _pagination = DataContext as PaginationHelper;
        if (_pagination is not null)
            _pagination.PropertyChanged += OnPaginationPropertyChanged;

        UpdateTotalLabel();
    }

    private void OnPaginationPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(PaginationHelper.TotalCount))
            UpdateTotalLabel();
    }

    private void ApplyLocale()
    {
        if (_locale is null) return;

        ToolTip.SetTip(FirstButton, _locale.T("Pag_First"));
        ToolTip.SetTip(PreviousButton, _locale.T("Pag_Previous"));
        ToolTip.SetTip(NextButton, _locale.T("Pag_Next"));
        ToolTip.SetTip(LastButton, _locale.T("Pag_Last"));
        UpdateTotalLabel();
    }

    private void UpdateTotalLabel()
    {
        if (_locale is null) return;
        var count = _pagination?.TotalCount ?? 0;
        TotalCountText.Text = _locale.Tf("Pag_Items", count);
    }
}
