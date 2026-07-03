using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using GestionCommerciale.Infrastructure;
using GestionCommerciale.Modules.Auth.Services;
using GestionCommerciale.Modules.Auth.ViewModels;
using GestionCommerciale.Shared.Database;
using GestionCommerciale.Shared.Services;
using GestionCommerciale.ViewModels;
using GestionCommerciale.Views;
using System.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GestionCommerciale;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            BindingPlugins.DataValidators.RemoveAt(0);

            var sc = new ServiceCollection();
            sc.AddGestionCommerciale();
            Services = sc.BuildServiceProvider();

            AppDbContext? db = null;
            try
            {
                db = Services.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext();
                db.Database.Migrate();
                EnsureSocieteMentionsLegalesColumn(db);
                EnsureFactureEstPayeeColumn(db);
                EnsureTrialLicenseColumns(db);
                DbSeeder.Seed(db);
            }
            catch (SqliteException ex) when (
                ex.SqliteErrorCode == 1 &&
                ex.Message.Contains("already exists", StringComparison.OrdinalIgnoreCase)) { }
            finally { db?.Dispose(); }

            var mainVm = Services.GetRequiredService<MainWindowViewModel>();
            desktop.MainWindow = new MainWindow { DataContext = mainVm };
            desktop.MainWindow.Opened += OnMainWindowOpened;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private async void OnMainWindowOpened(object? sender, EventArgs e)
    {
        var window = (Window)sender!;
        window.Opened -= OnMainWindowOpened;

        var licenseService = Services.GetRequiredService<ILicenseService>();
        var appSettingsService = Services.GetRequiredService<IAppSettingsService>();
        var dialogService = Services.GetRequiredService<IDialogService>();

        var settings = await appSettingsService.GetAsync(default);

        App.Services.GetRequiredService<VirtualKeyboardService>().IsEnabled = settings.EnableVirtualKeyboard;
        App.Services.GetRequiredService<IPeriodicBackupService>().Start();

        if (!licenseService.IsLicensed(settings))
        {
            if (settings.TrialStartedAt is null)
            {
                settings.TrialStartedAt = DateTime.UtcNow;
                await appSettingsService.SaveAsync(settings, default);
            }
            else if (licenseService.IsTrialExpired(settings))
            {
                while (true)
                {
                    var enteredKey = await dialogService.PromptLicenseAsync(
                        "Licence expirée",
                        "Votre période d'essai de 3 jours est expirée. Veuillez entrer votre clé de licence pour continuer.");

                    if (enteredKey is null)
                        Environment.Exit(0);

                    if (licenseService.ValidateLicenseKey(enteredKey))
                    {
                        settings.LicenseKey = enteredKey;
                        await appSettingsService.SaveAsync(settings, default);
                        break;
                    }

                    await dialogService.ShowErrorAsync("Clé invalide",
                        "La clé de licence saisie est incorrecte. Veuillez réessayer.");
                }
            }
        }

        await Services.GetRequiredService<ILocaleService>().InitializeAsync(default);

        var root = Services.GetRequiredService<RootNavigator>();
        var auth = Services.GetRequiredService<IAuthService>();
        var loggedIn = await auth.LoginAsync(DbSeeder.DefaultAdminEmail, DbSeeder.DefaultAdminPassword, default);

        root.SetRoot(loggedIn
            ? Services.GetRequiredService<AppShellViewModel>()
            : Services.GetRequiredService<LoginViewModel>());
    }

    /// <summary>Ensures TrialStartedAt and LicenseKey columns exist on AppSettings for older DBs.</summary>
    private static void EnsureTrialLicenseColumns(AppDbContext db)
    {
        var conn = db.Database.GetDbConnection();
        var wasClosed = conn.State != ConnectionState.Open;
        if (wasClosed) conn.Open();
        try
        {
            using var check = conn.CreateCommand();
            check.CommandText = "SELECT COUNT(*) FROM pragma_table_info('AppSettings') WHERE name = 'TrialStartedAt'";
            var hasCol = Convert.ToInt64(check.ExecuteScalar() ?? 0L) > 0;
            if (!hasCol)
            {
                using var alter = conn.CreateCommand();
                alter.CommandText = "ALTER TABLE AppSettings ADD COLUMN TrialStartedAt TEXT NULL;";
                alter.ExecuteNonQuery();
            }

            using var check2 = conn.CreateCommand();
            check2.CommandText = "SELECT COUNT(*) FROM pragma_table_info('AppSettings') WHERE name = 'LicenseKey'";
            hasCol = Convert.ToInt64(check2.ExecuteScalar() ?? 0L) > 0;
            if (!hasCol)
            {
                using var alter = conn.CreateCommand();
                alter.CommandText = "ALTER TABLE AppSettings ADD COLUMN LicenseKey TEXT NULL;";
                alter.ExecuteNonQuery();
            }
        }
        finally
        {
            if (wasClosed && conn.State == ConnectionState.Open)
                conn.Close();
        }
    }

    /// <summary>Repairs SQLite DBs where the model expects SocieteMentionsLegales but Migrate did not add it.</summary>
    private static void EnsureSocieteMentionsLegalesColumn(AppDbContext db)
    {
        var conn = db.Database.GetDbConnection();
        var wasClosed = conn.State != ConnectionState.Open;
        if (wasClosed) conn.Open();
        try
        {
            using var check = conn.CreateCommand();
            check.CommandText = "SELECT COUNT(*) FROM pragma_table_info('AppSettings') WHERE name = 'SocieteMentionsLegales'";
            var n = Convert.ToInt64(check.ExecuteScalar() ?? 0L);
            if (n > 0) return;

            using (var alter = conn.CreateCommand())
            {
                alter.CommandText = "ALTER TABLE AppSettings ADD COLUMN SocieteMentionsLegales TEXT NULL;";
                alter.ExecuteNonQuery();
            }

            try
            {
                using var hist = conn.CreateCommand();
                hist.CommandText =
                    "INSERT OR IGNORE INTO \"__EFMigrationsHistory\" (\"MigrationId\", \"ProductVersion\") " +
                    "VALUES ('20260430220000_AddSocieteMentionsLegales', '9.0.0');";
                hist.ExecuteNonQuery();
            }
            catch
            {
                // No migrations table (e.g. legacy EnsureCreated DB); column is enough for runtime.
            }
        }
        finally
        {
            if (wasClosed && conn.State == ConnectionState.Open)
                conn.Close();
        }
    }

    /// <summary>
    /// Repairs SQLite DBs where Factures still has legacy Statut but the model expects EstPayee
    /// (e.g. Migrate skipped or history out of sync).
    /// </summary>
    private static void EnsureFactureEstPayeeColumn(AppDbContext db)
    {
        var conn = db.Database.GetDbConnection();
        var wasClosed = conn.State != ConnectionState.Open;
        if (wasClosed) conn.Open();
        try
        {
            using var tableCheck = conn.CreateCommand();
            tableCheck.CommandText =
                "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='Factures';";
            var tableExists = Convert.ToInt64(tableCheck.ExecuteScalar() ?? 0L) > 0;
            if (!tableExists) return;

            using var colCheck = conn.CreateCommand();
            colCheck.CommandText =
                "SELECT COUNT(*) FROM pragma_table_info('Factures') WHERE name = 'EstPayee';";
            if (Convert.ToInt64(colCheck.ExecuteScalar() ?? 0L) > 0) return;

            using var statutCheck = conn.CreateCommand();
            statutCheck.CommandText =
                "SELECT COUNT(*) FROM pragma_table_info('Factures') WHERE name = 'Statut';";
            var hasStatut = Convert.ToInt64(statutCheck.ExecuteScalar() ?? 0L) > 0;

            using (var alter = conn.CreateCommand())
            {
                alter.CommandText = "ALTER TABLE Factures ADD COLUMN EstPayee INTEGER NOT NULL DEFAULT 0;";
                alter.ExecuteNonQuery();
            }

            if (hasStatut)
            {
                using (var upd = conn.CreateCommand())
                {
                    // Legacy StatutFacture.Payee == 3
                    upd.CommandText = "UPDATE Factures SET EstPayee = 1 WHERE Statut = 3;";
                    upd.ExecuteNonQuery();
                }

                using (var drop = conn.CreateCommand())
                {
                    drop.CommandText = "ALTER TABLE Factures DROP COLUMN Statut;";
                    drop.ExecuteNonQuery();
                }
            }

            try
            {
                using var hist = conn.CreateCommand();
                hist.CommandText =
                    "INSERT OR IGNORE INTO \"__EFMigrationsHistory\" (\"MigrationId\", \"ProductVersion\") " +
                    "VALUES ('20260501145728_FactureEstPayeeRemoveStatut', '9.0.0');";
                hist.ExecuteNonQuery();
            }
            catch
            {
                // No migrations table; schema fix is enough for runtime.
            }
        }
        finally
        {
            if (wasClosed && conn.State == ConnectionState.Open)
                conn.Close();
        }
    }
}
