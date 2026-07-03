namespace GestionCommerciale.Shared.Database;

public static class DbSeeder
{
    public const string DefaultAdminEmail = "admin@local";
    public const string DefaultAdminPassword = "admin";
    public const string DefaultClientName = "Client Comptoire";

    public static void Seed(AppDbContext db)
    {
        if (!db.AppSettings.Any())
        {
            db.AppSettings.Add(new AppSettingsRow { Id = 1 });
            db.SaveChanges();
        }

        if (!db.Tiers.Any(t => t.Nom == DefaultClientName))
        {
            db.Tiers.Add(new GestionCommerciale.Modules.Tiers.Models.Tiers
            {
                Nom = DefaultClientName,
                Type = GestionCommerciale.Modules.Tiers.Models.TypeTiers.Client,
                Actif = true
            });
            db.SaveChanges();
        }
    }
}
