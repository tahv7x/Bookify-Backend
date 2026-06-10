using Bookify_API.DTOs;
using Bookify_API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bookify_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServicesController : BaseController
    {
        private readonly BookifyDbContext context;

        public ServicesController(BookifyDbContext context)
        {
            this.context = context;
        }

        [HttpGet("mine")]
        [Authorize(Roles = "PRESTATAIRE")]
        public async Task<IActionResult> GetMyServices()
        {
            var myUserId = GetUserId();
            var myPrestataire = await context.Prestataires.FirstOrDefaultAsync(p => p.IdUtili == myUserId);
            
            if (myPrestataire == null) return Unauthorized(new { message = "Profil prestataire introuvable." });

            var services = await context.Services
                .Where(s => s.IdPres == myPrestataire.IdPres)
                .Select(s => new ServiceDto
                {
                    IdService = s.IdService,
                    Nom = s.Nom,
                    Description = s.Description,
                    Prix = s.Prix ?? 0,
                    Duree = s.Duree,
                    UniteDuree = s.UniteDuree
                })
                .ToListAsync();

            return Ok(services);
        }

        [HttpPost]
        [Authorize(Roles = "PRESTATAIRE")]
        public async Task<IActionResult> AddService([FromBody] ServiceDto dto)
        {
            var myUserId = GetUserId();
            var myPrestataire = await context.Prestataires.FirstOrDefaultAsync(p => p.IdUtili == myUserId);
            
            if (myPrestataire == null) return Unauthorized(new { message = "Profil prestataire introuvable." });

            var service = new Service
            {
                IdPres = myPrestataire.IdPres,
                Nom = dto.Nom,
                Description = dto.Description,
                Prix = dto.Prix,
                Duree = dto.Duree,
                UniteDuree = dto.UniteDuree
            };

            context.Services.Add(service);
            return await SaveAsyncChanges(context, new { message = "Service ajouté avec succès.", serviceId = service.IdService });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "PRESTATAIRE")]
        public async Task<IActionResult> UpdateService(int id, [FromBody] ServiceDto dto)
        {
            var myUserId = GetUserId();
            var myPrestataire = await context.Prestataires.FirstOrDefaultAsync(p => p.IdUtili == myUserId);
            
            if (myPrestataire == null) return Unauthorized(new { message = "Profil prestataire introuvable." });

            var service = await context.Services.FindAsync(id);
            if (service == null) return NotFound(new { message = "Service introuvable." });

            if (service.IdPres != myPrestataire.IdPres) return Forbid();

            service.Nom = dto.Nom;
            service.Description = dto.Description;
            service.Prix = dto.Prix;
            service.Duree = dto.Duree;
            service.UniteDuree = dto.UniteDuree;

            return await SaveAsyncChanges(context, new { message = "Service modifié avec succès." });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "PRESTATAIRE")]
        public async Task<IActionResult> DeleteService(int id)
        {
            var myUserId = GetUserId();
            var myPrestataire = await context.Prestataires.FirstOrDefaultAsync(p => p.IdUtili == myUserId);
            
            if (myPrestataire == null) return Unauthorized(new { message = "Profil prestataire introuvable." });

            var service = await context.Services.FindAsync(id);
            if (service == null) return NotFound(new { message = "Service introuvable." });

            if (service.IdPres != myPrestataire.IdPres) return Forbid();

            context.Services.Remove(service);
            return await SaveAsyncChanges(context, new { message = "Service supprimé avec succès." });
        }
    }
}
