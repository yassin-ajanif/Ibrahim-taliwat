using GestionCommerciale.Shared.ViewModels;

namespace GestionCommerciale.Shared.Services;

public interface IWorkspaceNavigator
{
    BaseViewModel? CurrentPage { get; }
    void Open(BaseViewModel page);
    event EventHandler? CurrentPageChanged;
}
