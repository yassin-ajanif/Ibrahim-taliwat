using GestionCommerciale.Modules.Services.Models;

namespace GestionCommerciale.Modules.Services;

public static class ServiceSearchFilter
{
    public static IQueryable<Service> WhereSearchMatches(this IQueryable<Service> query, string? searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return query;

        var t = searchTerm.Trim().ToLowerInvariant();
        return query.Where(s =>
            s.Reference.ToLower().Contains(t) ||
            s.Designation.ToLower().Contains(t));
    }
}
