using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace GestionCommerciale.Shared.Helpers;

public partial class PaginationHelper : ObservableObject
{
    private readonly Action _onPageChanged;

    public PaginationHelper(Action onPageChanged)
    {
        _onPageChanged = onPageChanged;
    }

    [ObservableProperty] private int _currentPage = 1;
    [ObservableProperty] private int _pageSize = 50;
    [ObservableProperty] private int _totalCount;

    public int TotalPages => int.Max(1, (int)Math.Ceiling((double)TotalCount / int.Max(1, PageSize)));

    public bool HasPrevious => CurrentPage > 1;
    public bool HasNext => CurrentPage < TotalPages;

    public int Skip => (CurrentPage - 1) * PageSize;

    [RelayCommand]
    private void GoToPage(int page)
    {
        if (page < 1 || page > TotalPages || page == CurrentPage) return;
        CurrentPage = page;
        _onPageChanged();
    }

    [RelayCommand]
    private void NextPage()
    {
        if (HasNext) GoToPage(CurrentPage + 1);
    }

    [RelayCommand]
    private void PreviousPage()
    {
        if (HasPrevious) GoToPage(CurrentPage - 1);
    }

    [RelayCommand]
    private void FirstPage()
    {
        if (HasPrevious) GoToPage(1);
    }

    [RelayCommand]
    private void LastPage()
    {
        if (HasNext) GoToPage(TotalPages);
    }

    public void Reset(int? totalCount = null)
    {
        CurrentPage = 1;
        if (totalCount.HasValue)
            TotalCount = totalCount.Value;
        _onPageChanged();
    }

    partial void OnTotalCountChanged(int value)
    {
        OnPropertyChanged(nameof(TotalPages));
        OnPropertyChanged(nameof(HasNext));
        OnPropertyChanged(nameof(HasPrevious));
    }

    partial void OnCurrentPageChanged(int value)
    {
        OnPropertyChanged(nameof(HasPrevious));
        OnPropertyChanged(nameof(HasNext));
    }

    partial void OnPageSizeChanged(int value)
    {
        OnPropertyChanged(nameof(TotalPages));
        OnPropertyChanged(nameof(HasNext));
        OnPropertyChanged(nameof(HasPrevious));
    }
}
