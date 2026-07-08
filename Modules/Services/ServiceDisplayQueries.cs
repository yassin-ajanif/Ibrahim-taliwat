using GestionCommerciale.Modules.Services.Models;
using Microsoft.EntityFrameworkCore;

namespace GestionCommerciale.Modules.Services;

/// <summary>EF queries that avoid loading large <see cref="Service.ImageData"/> blobs into list UIs.</summary>
public static class ServiceDisplayQueries
{
    public static IQueryable<Service> SelectForListWithoutImageData(this IQueryable<Service> source) =>
        source
            .Select(s => new Service
            {
                Id = s.Id,
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt,
                CreatedByUserId = s.CreatedByUserId,
                Reference = s.Reference,
                Designation = s.Designation,
                Unite = s.Unite,
                PrixVenteHT = s.PrixVenteHT,
                CoutHT = s.CoutHT,
                TauxTVA = s.TauxTVA,
                Actif = s.Actif,
                Note = s.Note,
                ImageData = null
            });
}

