using Bookify_API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bookify_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DisponibilitesController : BaseController
    {
        private readonly BookifyDbContext context;

        public DisponibilitesController(BookifyDbContext context)
        {
            this.context = context;
        }

        [HttpGet("mine")]
        [Authorize(Roles = "PRESTATAIRE")]
        public async Task<IActionResult> GetMyDisponibilites()
        {
            var myUserId = GetUserId();
            var myPrestataire = await context.Prestataires.FirstOrDefaultAsync(p => p.IdUtili == myUserId);
            if (myPrestataire == null) return Unauthorized(new { message = "Profil prestataire introuvable." });

            return await GetDisponibilites(myPrestataire.IdPres);
        }

        [HttpGet("{prestataireId:int}")]
        public async Task<IActionResult> GetDisponibilites(int prestataireId)
        {
            var slots = await context.Disponibilites
                .Where(d => d.IdPres == prestataireId)
                .OrderBy(d => d.JourSemaine)
                .ThenBy(d => d.HeureDebut)
                .Select(d => new
                {
                    id         = d.IdDispo,
                    jour       = d.JourSemaine,
                    heureDebut = d.HeureDebut.HasValue ? d.HeureDebut.Value.ToString(@"hh\:mm") : null,
                    heureFin   = d.HeureFin.HasValue ? d.HeureFin.Value.ToString(@"hh\:mm") : null,
                    disponible = d.Disponible
                })
                .ToListAsync();

            var grouped = slots
                .GroupBy(s => s.jour)
                .Select(g => new
                {
                    day   = g.Key,
                    slots = g.Select(s => new
                    {
                        id        = s.id,
                        time      = s.heureDebut,
                        endTime   = s.heureFin,
                        available = s.disponible
                    })
                });
            return Ok(grouped);
        }
    }
}
