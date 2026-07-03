using GestionCommerciale.Shared.ViewModels;

namespace GestionCommerciale.Shared.Services;

public interface IRootNavigator
{
    BaseViewModel? CurrentRoot { get; }
    void SetRoot(BaseViewModel viewModel);
    event EventHandler? CurrentRootChanged;
}
