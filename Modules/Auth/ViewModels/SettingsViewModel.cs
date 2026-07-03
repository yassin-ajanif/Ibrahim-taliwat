using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionCommerciale.Modules.Auth.Services;
using GestionCommerciale.Shared.Database;
using GestionCommerciale.Shared.Helpers;
using GestionCommerciale.Shared.Services;
using GestionCommerciale.Shared.ViewModels;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace GestionCommerciale.Modules.Auth.ViewModels;

public partial class SettingsViewModel : BaseViewModel
{
    private readonly IAppSettingsService _settings;
    private readonly IDialogService _dialog;
    private readonly ICurrentUserSession _session;
    private readonly ILocaleService _locale;
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly IBackupService _backup;

    public UiLanguageOption[] LanguageOptions { get; } =
    [
        new() { Code = "fr", Label = "Français" },
        new() { Code = "ar", Label = "العربية" }
    ];

    public SettingsViewModel(
        IAppSettingsService settings,
        IDialogService dialog,
        ICurrentUserSession session,
        ILocaleService locale,
        IDbContextFactory<AppDbContext> dbFactory,
        IBackupService backup)
    {
        _settings = settings;
        _dialog = dialog;
        _session = session;
        _locale = locale;
        _dbFactory = dbFactory;
        _backup = backup;
        _locale.CultureApplied += (_, _) => RefreshSettingsLabels();
        RefreshSettingsLabels();
    }

    [ObservableProperty] private string _societeNom = string.Empty;
    [ObservableProperty] private string _societeAdresse = string.Empty;
    [ObservableProperty] private string _societeIce = string.Empty;
    [ObservableProperty] private string? _societeLogoPath;
    [ObservableProperty] private string _societeMentionsLegales = string.Empty;
    [ObservableProperty] private string _tauxTvaText = "20";
    [ObservableProperty] private bool _blocageStock = true;
    [ObservableProperty] private bool _enableVirtualKeyboard;
    [ObservableProperty] private int _devisValiditeJours = 30;
    [ObservableProperty] private string _devise = "MAD";
    [ObservableProperty] private UiLanguageOption? _selectedLanguageOption;

    [ObservableProperty] private string _lblLoad = string.Empty;
    [ObservableProperty] private string _lblSave = string.Empty;
    [ObservableProperty] private string _lblSocieteNom = string.Empty;
    [ObservableProperty] private string _lblAdresse = string.Empty;
    [ObservableProperty] private string _lblIce = string.Empty;
    [ObservableProperty] private string _lblMentionsLegales = string.Empty;
    [ObservableProperty] private string _lblLogo = string.Empty;
    [ObservableProperty] private string _lblPickLogo = string.Empty;
    [ObservableProperty] private string _lblTva = string.Empty;
    [ObservableProperty] private string _lblBl = string.Empty;
    [ObservableProperty] private string _lblBlocageStock = string.Empty;
    [ObservableProperty] private string _lblDevisJours = string.Empty;
    [ObservableProperty] private string _lblEnableVirtualKeyboard = string.Empty;
    [ObservableProperty] private string _lblDevise = string.Empty;
    [ObservableProperty] private string _lblUiLanguage = string.Empty;
    [ObservableProperty] private string _lblDangerZone = string.Empty;
    [ObservableProperty] private string _btnFormatSystem = string.Empty;
    [ObservableProperty] private bool _backupEnabled;
    [ObservableProperty] private int _backupIntervalHours = 24;
    [ObservableProperty] private int _backupRetentionDays = 30;
    [ObservableProperty] private string _backupDirectory = string.Empty;
    [ObservableProperty] private int _backupCount;
    [ObservableProperty] private string _lastBackupDateStr = string.Empty;
    [ObservableProperty] private string _backupIntervalUnit = "Hours";
    [ObservableProperty] private string _lblBackupIntervalUnit = string.Empty;
    public List<string> BackupIntervalUnitOptions { get; } = ["Minutes", "Hours"];

    [ObservableProperty] private string _lblBackup = string.Empty;
    [ObservableProperty] private string _lblBackupEnabled = string.Empty;
    [ObservableProperty] private string _lblBackupInterval = string.Empty;
    [ObservableProperty] private string _lblBackupRetention = string.Empty;
    [ObservableProperty] private string _lblBackupDirectory = string.Empty;
    [ObservableProperty] private string _lblPickBackupDir = string.Empty;
    [ObservableProperty] private string _lblBackupNow = string.Empty;
    [ObservableProperty] private string _lblLastBackup = string.Empty;
    [ObservableProperty] private bool _backupExpanded;

    public string BackupArrow => BackupExpanded ? "\u25BC" : "\u25B6";

    partial void OnBackupExpandedChanged(bool value) => OnPropertyChanged(nameof(BackupArrow));

    [RelayCommand]
    private void ToggleBackup() => BackupExpanded = !BackupExpanded;

    [ObservableProperty] private string _wmSociete = string.Empty;
    [ObservableProperty] private string _wmAdresse = string.Empty;
    [ObservableProperty] private string _wmIce = string.Empty;
    [ObservableProperty] private string _wmMentionsLegales = string.Empty;
    [ObservableProperty] private string _wmLogoPath = string.Empty;
    [ObservableProperty] private string _wmTva = string.Empty;
    [ObservableProperty] private string _wmDevise = string.Empty;

    [ObservableProperty] private int _numberingYear;
    [ObservableProperty] private string _lblNumbering = string.Empty;
    [ObservableProperty] private string _lblNumberingHelp = string.Empty;
    [ObservableProperty] private string _lblNumberingYear = string.Empty;
    [ObservableProperty] private string _lblNumberingDocument = string.Empty;
    [ObservableProperty] private string _lblNumberingLastOutside = string.Empty;
    [ObservableProperty] private string _lblNumberingNext = string.Empty;
    [ObservableProperty] private string _lblNumberingInDb = string.Empty;
    [ObservableProperty] private bool _numberingExpanded;

    public string NumberingArrow => NumberingExpanded ? "\u25BC" : "\u25B6";

    partial void OnNumberingExpandedChanged(bool value) => OnPropertyChanged(nameof(NumberingArrow));

    [RelayCommand]
    private void ToggleNumbering() => NumberingExpanded = !NumberingExpanded;

    public ObservableCollection<DocumentNumberingSettingRow> NumberingRows { get; } = [];

    private Dictionary<string, Dictionary<int, int>> _numberingFloors = new(StringComparer.OrdinalIgnoreCase);

    private void RefreshSettingsLabels()
    {
        Title = _locale.T("Settings_Title");
        LblLoad = _locale.T("Settings_Load");
        LblSave = _locale.T("Settings_Save");
        LblSocieteNom = _locale.T("Settings_SocieteNom");
        LblAdresse = _locale.T("Settings_Adresse");
        LblIce = _locale.T("Settings_Ice");
        LblMentionsLegales = _locale.T("Settings_MentionsLegales");
        LblLogo = _locale.T("Settings_Logo");
        LblPickLogo = _locale.T("Settings_PickLogo");
        LblTva = _locale.T("Settings_Tva");
        LblBl = _locale.T("Settings_BL");
        LblBlocageStock = _locale.T("Settings_BlocageStock");
        LblDevisJours = _locale.T("Settings_DevisJours");
        LblEnableVirtualKeyboard = _locale.T("Settings_EnableVirtualKeyboard");
        LblDevise = _locale.T("Settings_Devise");
        LblUiLanguage = _locale.T("Settings_UiLanguage");
        LblDangerZone = _locale.T("Settings_DangerZone");
        BtnFormatSystem = _locale.T("Settings_BtnFormatSystem");
        LblBackup = _locale.T("Settings_Backup");
        LblBackupEnabled = _locale.T("Settings_BackupEnabled");
        LblBackupInterval = _locale.T("Settings_BackupInterval");
        LblBackupIntervalUnit = _locale.T("Settings_BackupIntervalUnit");
        LblBackupRetention = _locale.T("Settings_BackupRetention");
        LblBackupDirectory = _locale.T("Settings_BackupDirectory");
        LblPickBackupDir = _locale.T("Settings_PickBackupDir");
        LblBackupNow = _locale.T("Settings_BackupNow");
        LblLastBackup = _locale.T("Settings_LastBackup");
        WmSociete = _locale.T("Settings_WmSociete");
        WmAdresse = _locale.T("Settings_WmAdresse");
        WmIce = _locale.T("Settings_WmIce");
        WmMentionsLegales = _locale.T("Settings_WmMentionsLegales");
        WmLogoPath = _locale.T("Settings_WmLogoPath");
        WmTva = _locale.T("Settings_WmTva");
        WmDevise = _locale.T("Settings_WmDevise");
        LblNumbering = _locale.T("Settings_Numbering");
        LblNumberingHelp = _locale.T("Settings_NumberingHelp");
        LblNumberingYear = _locale.T("Settings_NumberingYear");
        LblNumberingDocument = _locale.T("Settings_NumberingDocument");
        LblNumberingLastOutside = _locale.T("Settings_NumberingLastOutside");
        LblNumberingNext = _locale.T("Settings_NumberingNext");
        LblNumberingInDb = _locale.T("Settings_NumberingInDb");
        foreach (var row in NumberingRows)
        {
            var kind = DocumentNumberKind.All.FirstOrDefault(k => string.Equals(k.Prefix, row.Prefix, StringComparison.OrdinalIgnoreCase));
            if (kind is not null)
                row.DocumentLabel = _locale.T(kind.LabelKey);
        }
    }

    private async Task LoadNumberingRowsAsync(AppDbContext db, AppSettingsRow row, CancellationToken cancellationToken)
    {
        var year = DateTime.Now.Year;
        NumberingYear = year;
        _numberingFloors = DocumentNumberingFloorsStorage.Parse(row.DocumentNumberingFloorsJson);
        NumberingRows.Clear();

        foreach (var kind in DocumentNumberKind.All)
        {
            var numeros = await DocumentNumberingQuery.LoadNumerosAsync(db, kind.Prefix, cancellationToken);
            var dbMax = DocumentNumberingHelper.GetMaxSequenceFromNumeros(numeros, kind.Prefix, year);
            var lastOutside = DocumentNumberingFloorsStorage.GetLastUsedOutside(_numberingFloors, kind.Prefix, year);
            NumberingRows.Add(new DocumentNumberingSettingRow
            {
                Prefix = kind.Prefix,
                DocumentLabel = _locale.T(kind.LabelKey),
                NumberingYear = year,
                DbMaxSequence = dbMax,
                LastUsedOutside = lastOutside
            });
        }
    }

    private string BuildNumberingFloorsJson()
    {
        foreach (var row in NumberingRows)
            DocumentNumberingFloorsStorage.SetLastUsedOutside(_numberingFloors, row.Prefix, row.NumberingYear, row.LastUsedOutside);
        return DocumentNumberingFloorsStorage.Serialize(_numberingFloors);
    }

    [RelayCommand]
    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        if (!_session.CanAccessSettings)
        {
            await _dialog.ShowErrorAsync(_locale.T("Settings_Title"), _locale.T("Settings_ErrAdmin"), cancellationToken);
            return;
        }

        var row = await _settings.GetAsync(cancellationToken);
        SocieteNom = row.SocieteNom;
        SocieteAdresse = row.SocieteAdresse;
        SocieteIce = row.SocieteICE;
        SocieteMentionsLegales = row.SocieteMentionsLegales ?? string.Empty;
        SocieteLogoPath = row.SocieteLogoPath;
        BlocageStock = row.BlocageSiStockInsuffisant;
        EnableVirtualKeyboard = row.EnableVirtualKeyboard;
        DevisValiditeJours = row.DevisValiditeJoursDefaut;
        Devise = row.Devise;
        SelectedLanguageOption = LanguageOptions.FirstOrDefault(o =>
                                     string.Equals(o.Code, row.UiLanguage, StringComparison.OrdinalIgnoreCase))
                                 ?? LanguageOptions[0];
        try
        {
            var arr = JsonSerializer.Deserialize<List<decimal>>(row.TauxTVAJson);
            TauxTvaText = arr == null ? "20" : string.Join(",", arr.Select(x => x.ToString(CultureInfo.InvariantCulture)));
        }
        catch
        {
            TauxTvaText = "20";
        }

        BackupEnabled = row.BackupEnabled;
        BackupIntervalHours = row.BackupIntervalHours;
        BackupIntervalUnit = string.IsNullOrWhiteSpace(row.BackupIntervalUnit) ? "Hours" : row.BackupIntervalUnit;
        BackupRetentionDays = row.BackupRetentionDays;
        BackupDirectory = row.BackupDirectory;
        BackupCount = await _backup.GetBackupCountAsync(row.BackupDirectory, cancellationToken);
        LastBackupDateStr = row.LastBackupDate.HasValue
            ? row.LastBackupDate.Value.ToLocalTime().ToString("g")
            : string.Empty;

        await using (var db = await _dbFactory.CreateDbContextAsync(cancellationToken))
            await LoadNumberingRowsAsync(db, row, cancellationToken);

        RefreshSettingsLabels();
    }

    [RelayCommand]
    private async Task SaveAsync(CancellationToken cancellationToken)
    {
        if (!_session.CanAccessSettings) return;
        List<decimal> taux;
        try
        {
            taux = TauxTvaText.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(s => decimal.Parse(s, CultureInfo.InvariantCulture)).ToList();
            if (taux.Count == 0) taux = [20];
        }
        catch
        {
            await _dialog.ShowErrorAsync(_locale.T("Settings_Title"), _locale.T("Settings_ErrTva"), cancellationToken);
            return;
        }

        var lang = SelectedLanguageOption?.Code ?? "fr";
        var existing = await _settings.GetAsync(cancellationToken);
        var row = new AppSettingsRow
        {
            Id = 1,
            SocieteNom = SocieteNom,
            SocieteAdresse = SocieteAdresse,
            SocieteICE = SocieteIce,
            SocieteMentionsLegales = string.IsNullOrWhiteSpace(SocieteMentionsLegales) ? null : SocieteMentionsLegales.Trim(),
            SocieteLogoPath = SocieteLogoPath,
            TauxTVAJson = JsonSerializer.Serialize(taux),
            BlocageSiStockInsuffisant = BlocageStock,
            EnableVirtualKeyboard = EnableVirtualKeyboard,
            DevisValiditeJoursDefaut = DevisValiditeJours,
            Devise = Devise,
            UiLanguage = lang,
            BackupEnabled = BackupEnabled,
            BackupIntervalHours = BackupIntervalHours,
            BackupIntervalUnit = BackupIntervalUnit,
            BackupRetentionDays = BackupRetentionDays,
            BackupDirectory = BackupDirectory,
            TrialStartedAt = existing.TrialStartedAt,
            LicenseKey = existing.LicenseKey,
            LastBackupDate = existing.LastBackupDate,
            DocumentNumberingFloorsJson = BuildNumberingFloorsJson()
        };
        await _settings.SaveAsync(row, cancellationToken);
        _locale.ApplyLanguage(lang);
        RefreshSettingsLabels();
        await using (var db = await _dbFactory.CreateDbContextAsync(cancellationToken))
            await LoadNumberingRowsAsync(db, row, cancellationToken);
        await _dialog.ShowInfoAsync(_locale.T("Settings_Title"), _locale.T("Settings_Saved"), cancellationToken);
    }

    [RelayCommand]
    private async Task PickBackupDirectoryAsync(CancellationToken cancellationToken)
    {
        var path = await _dialog.PickFolderAsync(cancellationToken);
        if (!string.IsNullOrWhiteSpace(path))
        {
            BackupDirectory = path;
            BackupCount = await _backup.GetBackupCountAsync(path, cancellationToken);
            await SaveAsync(cancellationToken);
        }
    }

    [RelayCommand]
    private async Task CreateBackupNowAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(BackupDirectory))
        {
            await _dialog.ShowErrorAsync(_locale.T("Settings_Backup"), _locale.T("Settings_BackupNoDir"), cancellationToken);
            return;
        }

        IsBusy = true;
        try
        {
            var result = await _backup.CreateBackupAsync(BackupDirectory, cancellationToken);
            if (result is null)
            {
                await _dialog.ShowErrorAsync(_locale.T("Settings_Backup"), _locale.T("Settings_BackupFailed"), cancellationToken);
                return;
            }

            LastBackupDateStr = DateTime.Now.ToString("g");
            BackupCount = await _backup.GetBackupCountAsync(BackupDirectory, cancellationToken);
            await _backup.CleanupOldBackupsAsync(BackupDirectory, BackupRetentionDays, cancellationToken);
            await _dialog.ShowInfoAsync(_locale.T("Settings_Backup"), _locale.T("Settings_BackupDone"), cancellationToken);
        }
        catch (Exception ex)
        {
            await _dialog.ShowErrorAsync(_locale.T("Settings_Backup"), ex.Message, cancellationToken);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task PickLogoAsync(CancellationToken cancellationToken)
    {
        var path = await _dialog.PickOpenFileAsync(_locale.T("Settings_Logo"), new[] { "*.png", "*.jpg", "*.jpeg" }, cancellationToken);
        SocieteLogoPath = path;
    }

    [RelayCommand]
    private async Task FormatSystemAsync(CancellationToken cancellationToken)
    {
        if (!_session.CanAccessSettings)
        {
            await _dialog.ShowErrorAsync(_locale.T("Settings_Title"), _locale.T("Settings_ErrAdmin"), cancellationToken);
            return;
        }

        var password = await _dialog.PromptPasswordAsync(
            _locale.T("Settings_Title"),
            _locale.T("Settings_FormatAskPassword"),
            cancellationToken);
        if (string.IsNullOrWhiteSpace(password))
            return;

        if (!_session.IsAdmin || password != DbSeeder.DefaultAdminPassword)
        {
            await _dialog.ShowErrorAsync(_locale.T("Settings_Title"), _locale.T("Settings_FormatBadPassword"), cancellationToken);
            return;
        }

        var confirm = await _dialog.ConfirmAsync(_locale.T("Settings_Title"), _locale.T("Settings_FormatConfirm"), cancellationToken);
        if (!confirm)
            return;

        await ResetDatabaseAsync(cancellationToken);
        CleanupAppDataFiles();
        await _dialog.ShowInfoAsync(_locale.T("Settings_Title"), _locale.T("Settings_FormatDone"), cancellationToken);
        await LoadAsync(cancellationToken);
    }

    private async Task ResetDatabaseAsync(CancellationToken cancellationToken)
    {
        var dbPath = Path.Combine(DatabasePath.GetDirectory(), "data.db");

        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();

        for (var i = 0; i < 5; i++)
        {
            try
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
                break;
            }
            catch
            {
                if (i == 4) throw;
                await Task.Delay(500, cancellationToken);
            }
        }

        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        await db.Database.MigrateAsync(cancellationToken);
        DbSeeder.Seed(db);
    }

    private static void CleanupAppDataFiles()
    {
        var root = DatabasePath.GetDirectory();
        var dbPath = Path.Combine(root, "data.db");

        if (!Directory.Exists(root))
            return;

        foreach (var file in Directory.GetFiles(root, "*", SearchOption.AllDirectories))
        {
            if (string.Equals(file, dbPath, StringComparison.OrdinalIgnoreCase))
                continue;

            try { File.Delete(file); }
            catch { }
        }

        foreach (var dir in Directory.GetDirectories(root, "*", SearchOption.AllDirectories)
                     .OrderByDescending(x => x.Length))
        {
            try
            {
                if (!Directory.EnumerateFileSystemEntries(dir).Any())
                    Directory.Delete(dir, false);
            }
            catch { }
        }
    }
}
