using System.Globalization;
using GestionCommerciale.Shared.Database;

namespace GestionCommerciale.Shared.Helpers;

public static class CurrencyHelper
{
    public static string Format(decimal amount, string currencyCode = "MAD")
    {
        var c = CultureInfo.GetCultureInfo("fr-FR");
        return amount.ToString("N2", c) + " " + currencyCode;
    }

    public static string FromSettings(AppSettingsRow cfg) =>
        string.IsNullOrWhiteSpace(cfg.Devise) ? string.Empty : cfg.Devise.Trim();
}
