using GestionCommerciale.Shared.Database;

namespace GestionCommerciale.Shared.Services;

public sealed class LicenseService : ILicenseService
{
    public bool IsTrialActive(AppSettingsRow settings) => false;

    public bool IsLicensed(AppSettingsRow settings) => true;

    public bool IsTrialExpired(AppSettingsRow settings) => false;

    public bool ValidateLicenseKey(string key) => true;
}

