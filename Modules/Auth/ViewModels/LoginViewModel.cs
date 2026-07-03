using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionCommerciale.Modules.Auth.Services;
using GestionCommerciale.Shared.Database;
using GestionCommerciale.Shared.Services;
using GestionCommerciale.Shared.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace GestionCommerciale.Modules.Auth.ViewModels;

public partial class LoginViewModel : BaseViewModel
{
    private readonly IAuthService _auth;
    private readonly RootNavigator _root;
    private readonly IServiceProvider _sp;
    private readonly IDialogService _dialog;
    private readonly ILocaleService _locale;

    public LoginViewModel(
        IAuthService auth,
        RootNavigator rootNavigator,
        IServiceProvider sp,
        IDialogService dialog,
        ILocaleService locale)
    {
        _auth = auth;
        _root = rootNavigator;
        _sp = sp;
        _dialog = dialog;
        _locale = locale;
        _locale.CultureApplied += (_, _) => RefreshLoginUi();
        RefreshLoginUi();
        Email = DbSeeder.DefaultAdminEmail;
    }

    [ObservableProperty] private string _lAppTitle = string.Empty;
    [ObservableProperty] private string _lSubtitle = string.Empty;
    [ObservableProperty] private string _wmEmail = string.Empty;
    [ObservableProperty] private string _wmPassword = string.Empty;
    [ObservableProperty] private string _btnConnect = string.Empty;

    [ObservableProperty] private string _email = string.Empty;
    [ObservableProperty] private string _password = string.Empty;

    private void RefreshLoginUi()
    {
        Title = _locale.T("Login_Title");
        LAppTitle = _locale.T("Login_AppTitle");
        LSubtitle = _locale.T("Login_Subtitle");
        WmEmail = _locale.T("Wm_Email");
        WmPassword = _locale.T("Wm_Password");
        BtnConnect = _locale.T("Btn_Connect");
    }

    [RelayCommand]
    private async Task LoginAsync(CancellationToken cancellationToken)
    {
        IsBusy = true;
        try
        {
            if (!await _auth.LoginAsync(Email, Password, cancellationToken))
            {
                await _dialog.ShowErrorAsync(_locale.T("Login_Title"), _locale.T("Login_ErrBadCreds"), cancellationToken);
                return;
            }

            _root.SetRoot(_sp.GetRequiredService<AppShellViewModel>());
        }
        finally
        {
            IsBusy = false;
        }
    }
}
