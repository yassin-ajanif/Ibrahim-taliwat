using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionCommerciale.Shared.Helpers;
using GestionCommerciale.Shared.Services;

namespace GestionCommerciale.Shared.ViewModels;

public partial class DatePresetChipsModel : ObservableObject
{
    private readonly ILocaleService _locale;
    private readonly Action<DateTime, DateTime> _onRangeApplied;

    public DatePresetChipsModel(ILocaleService locale, Action<DateTime, DateTime> onRangeApplied)
    {
        _locale = locale;
        _onRangeApplied = onRangeApplied;
        _locale.CultureApplied += (_, _) => RefreshLabels();
        RefreshLabels();
    }

    [ObservableProperty] private string _btnToday = string.Empty;
    [ObservableProperty] private string _btnWeek = string.Empty;
    [ObservableProperty] private string _btnMonth = string.Empty;
    [ObservableProperty] private string _btnYear = string.Empty;

    [ObservableProperty] private bool _presetTodayActive;
    [ObservableProperty] private bool _presetWeekActive;
    [ObservableProperty] private bool _presetMonthActive;
    [ObservableProperty] private bool _presetYearActive;

    private void RefreshLabels()
    {
        BtnToday = _locale.T("Reports_PresetToday");
        BtnWeek = _locale.T("Reports_PresetWeek");
        BtnMonth = _locale.T("Reports_PresetMonth");
        BtnYear = _locale.T("Reports_PresetYear");
    }

    public void ClearSelection()
    {
        PresetTodayActive = false;
        PresetWeekActive = false;
        PresetMonthActive = false;
        PresetYearActive = false;
    }

    public void SyncSelection(DateTime from, DateTime to)
    {
        foreach (var preset in new[] { DateRangePreset.Today, DateRangePreset.Week, DateRangePreset.Month, DateRangePreset.Year })
        {
            var (f, t) = DateRangePresets.GetRange(preset);
            if (f.Date == from.Date && t.Date == to.Date)
            {
                SetActive(preset);
                return;
            }
        }
        ClearSelection();
    }

    private void SetActive(DateRangePreset preset)
    {
        PresetTodayActive = preset == DateRangePreset.Today;
        PresetWeekActive = preset == DateRangePreset.Week;
        PresetMonthActive = preset == DateRangePreset.Month;
        PresetYearActive = preset == DateRangePreset.Year;
    }

    [RelayCommand]
    private void SetToday() => Apply(DateRangePreset.Today);

    [RelayCommand]
    private void SetWeek() => Apply(DateRangePreset.Week);

    [RelayCommand]
    private void SetMonth() => Apply(DateRangePreset.Month);

    [RelayCommand]
    private void SetYear() => Apply(DateRangePreset.Year);

    private void Apply(DateRangePreset preset)
    {
        var (from, to) = DateRangePresets.GetRange(preset);
        SetActive(preset);
        _onRangeApplied(from, to);
    }
}
