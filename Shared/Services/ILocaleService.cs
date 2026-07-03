namespace GestionCommerciale.Shared.Services;

/// <summary>Application UI language (French / Arabic) and RTL layout.</summary>
public interface ILocaleService
{
    /// <summary>BCP-like tag: <c>fr</c> or <c>ar</c>.</summary>
    string CurrentLanguage { get; }

    bool IsRightToLeft { get; }

    /// <summary>Fired after culture and language tag change (apply to window FlowDirection, refresh labels).</summary>
    event EventHandler? CultureApplied;

    Task InitializeAsync(CancellationToken cancellationToken = default);

    /// <summary>Persists via settings save separately; this only applies runtime culture + raises <see cref="CultureApplied"/>.</summary>
    void ApplyLanguage(string? languageTag);

    string T(string key);

    string Tf(string key, params object?[] args);
}
