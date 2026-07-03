using CommunityToolkit.Mvvm.Input;
using GestionCommerciale.Shared.Services;
using GestionCommerciale.Shared.ViewModels;

namespace GestionCommerciale.ViewModels;

public partial class MainWindowViewModel : BaseViewModel
{
    private readonly ILocaleService _locale;
    private readonly IAppUpdateService _updates;

    public MainWindowViewModel(RootNavigator rootNavigator, ILocaleService locale, IAppUpdateService updates)
    {
        Root = rootNavigator;
        _locale = locale;
        _updates = updates;
        _locale.CultureApplied += (_, _) => RefreshLabels();
        _updates.UpdateStateChanged += (_, _) => RefreshUpdateBanner();
        RefreshLabels();
        RefreshUpdateBanner();
        _ = _updates.CheckForUpdatesAsync();
    }

    public RootNavigator Root { get; }

    public bool IsUpdateBannerVisible => _updates.IsUpdateAvailable;

    public string UpdateBannerText { get; private set; } = string.Empty;

    [RelayCommand]
    private async Task ApplyUpdateAsync() => await _updates.DownloadAndApplyUpdateAsync();

    private void RefreshLabels()
    {
        Title = $"{_locale.T("Win_AppTitle")} v{_updates.DisplayVersion}";
        RefreshUpdateBanner();
    }

    private void RefreshUpdateBanner()
    {
        if (!_updates.IsUpdateAvailable)
        {
            UpdateBannerText = string.Empty;
            OnPropertyChanged(nameof(IsUpdateBannerVisible));
            OnPropertyChanged(nameof(UpdateBannerText));
            return;
        }

        var key = _updates.IsUpdateDownloaded ? "Update_BannerReady" : "Update_Banner";
        var version = _updates.AvailableVersion ?? string.Empty;
        UpdateBannerText = string.Format(_locale.T(key), version);
        OnPropertyChanged(nameof(IsUpdateBannerVisible));
        OnPropertyChanged(nameof(UpdateBannerText));
    }
}
