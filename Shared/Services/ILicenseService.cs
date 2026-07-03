using GestionCommerciale.Shared.Database;

namespace GestionCommerciale.Shared.Services;

public interface ILicenseService
{
    bool IsTrialActive(AppSettingsRow settings);
    bool IsLicensed(AppSettingsRow settings);
    bool IsTrialExpired(AppSettingsRow settings);
    bool ValidateLicenseKey(string key);
}
