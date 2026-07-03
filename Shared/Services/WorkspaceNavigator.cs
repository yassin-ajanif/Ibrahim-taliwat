using System.ComponentModel;
using System.Runtime.CompilerServices;
using GestionCommerciale.Shared.ViewModels;

namespace GestionCommerciale.Shared.Services;

public sealed class WorkspaceNavigator : IWorkspaceNavigator, INotifyPropertyChanged
{
    private BaseViewModel? _currentPage;

    public BaseViewModel? CurrentPage
    {
        get => _currentPage;
        private set
        {
            if (ReferenceEquals(_currentPage, value)) return;
            _currentPage = value;
            OnPropertyChanged();
            CurrentPageChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public event EventHandler? CurrentPageChanged;
    public event PropertyChangedEventHandler? PropertyChanged;

    public void Open(BaseViewModel page) => CurrentPage = page;

    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
