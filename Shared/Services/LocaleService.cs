using System;
using System.Globalization;

namespace GestionCommerciale.Shared.Services;

public sealed class LocaleService : ILocaleService
{
    private readonly IAppSettingsService _appSettings;
    private string _language = "fr";

    public LocaleService(IAppSettingsService appSettings) => _appSettings = appSettings;

    public string CurrentLanguage => _language;

    public bool IsRightToLeft => _language.StartsWith("ar", StringComparison.OrdinalIgnoreCase);

    public event EventHandler? CultureApplied;

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var row = await _appSettings.GetAsync(cancellationToken);
        ApplyLanguage(string.IsNullOrWhiteSpace(row.UiLanguage) ? "fr" : row.UiLanguage);
    }

    public void ApplyLanguage(string? languageTag)
    {
        _language = Normalize(languageTag);
        var culture = _language.StartsWith("ar", StringComparison.OrdinalIgnoreCase)
            ? new CultureInfo("ar")
            : new CultureInfo("fr-FR");

        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;

        CultureApplied?.Invoke(this, EventArgs.Empty);
    }

    public string T(string key) => UiTranslations.Get(key, _language);

    public string Tf(string key, params object?[] args)
    {
        var template = T(key);
        if (args == null || args.Length == 0)
            return template;
        try
        {
            return string.Format(CultureInfo.CurrentUICulture, template, args);
        }
        catch (FormatException)
        {
            return template;
        }
    }

    private static string Normalize(string? languageTag)
    {
        if (string.IsNullOrWhiteSpace(languageTag)) return "fr";
        var t = languageTag.Trim().ToLowerInvariant();
        return t.StartsWith("ar", StringComparison.Ordinal) ? "ar" : "fr";
    }
}
