using System.ComponentModel;
using System.Runtime.CompilerServices;
using GestionCommerciale.Shared.ViewModels;

namespace GestionCommerciale.Shared.Services;

public sealed class RootNavigator : IRootNavigator, INotifyPropertyChanged
{
    private BaseViewModel? _currentRoot;

    public BaseViewModel? CurrentRoot
    {
        get => _currentRoot;
        private set
        {
            if (ReferenceEquals(_currentRoot, value)) return;
            _currentRoot = value;
            OnPropertyChanged();
            CurrentRootChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public event EventHandler? CurrentRootChanged;
    public event PropertyChangedEventHandler? PropertyChanged;

    public void SetRoot(BaseViewModel viewModel) => CurrentRoot = viewModel;

    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
