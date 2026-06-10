using Bookify_API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Bookify_API.DTOs;

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

        [HttpGet("{prestataireId}")]
        public async Task<IActionResult> GetDisponibiltes(int prestataireId)
        {
            var slots = await context.Disponibilites
                .Where(d => d.IdPres == prestataireId)
                .OrderBy(d => d.JourSemaine)
                .ThenBy(d => d.HeureDebut)
                .Select(d => new
                {
                    id         = d.IdDispo,
                    jour       = d.JourSemaine,
                    heureDebut = d.HeureDebut.ToString(@"hh\:mm"),
                    heureFin   = d.HeureFin.ToString(@"hh\:mm"),
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

        [HttpGet("mine")]
        [Authorize(Roles = "PRESTATAIRE")]
        public async Task<IActionResult> GetMyDisponibiltes()
        {
            var myUserId = GetUserId();
            var myPrestataire = await context.Prestataires.FirstOrDefaultAsync(p => p.IdUtili == myUserId);
            if (myPrestataire == null) return Unauthorized(new { message = "Profil prestataire introuvable." });

            var slots = await context.Disponibilites
                .Where(d => d.IdPres == myPrestataire.IdPres)
                .OrderBy(d => d.JourSemaine)
                .ThenBy(d => d.HeureDebut)
                .Select(d => new
                {
                    id         = d.IdDispo,
                    jour       = d.JourSemaine,
                    heureDebut = d.HeureDebut.ToString(@"hh\:mm"),
                    heureFin   = d.HeureFin.ToString(@"hh\:mm"),
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

        [HttpPost]
        [Authorize(Roles = "PRESTATAIRE")]
        public async Task<IActionResult> AddDisponibilte([FromBody]SetDispoDto dto)
        {
            var myUserId = GetUserId();
            var myPrestataire = await context.Prestataires.FirstOrDefaultAsync(p => p.IdUtili == myUserId);
            
            if (myPrestataire == null) return Unauthorized(new { message = "Profil prestataire introuvable." });

            var dispo = new Disponibilite
            {
                IdPres      = myPrestataire.IdPres, // infer from token
                JourSemaine = dto.Jour,
                HeureDebut  = TimeSpan.Parse(dto.HeureDebut),
                HeureFin    = TimeSpan.Parse(dto.HeureFin),
                Disponible  = dto.Disponible
            };
            context.Disponibilites.Add(dispo);
            await context.SaveChangesAsync();
            return Ok(new { id = dispo.IdDispo });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "PRESTATAIRE")]
        public async Task<IActionResult> UpdateDisponibilite(int id, [FromBody]SetDispoDto dto)
        {
            var myUserId = GetUserId();
            var myPrestataire = await context.Prestataires.FirstOrDefaultAsync(p => p.IdUtili == myUserId);
            if (myPrestataire == null) return Unauthorized(new { message = "Profil prestataire introuvable." });

            var dispo = await context.Disponibilites.FindAsync(id);
            if (dispo == null) return NotFound();

            if (dispo.IdPres != myPrestataire.IdPres) return Forbid();

            dispo.JourSemaine = dto.Jour;
            dispo.HeureDebut  = TimeSpan.Parse(dto.HeureDebut);
            dispo.HeureFin    = TimeSpan.Parse(dto.HeureFin);
            dispo.Disponible  = dto.Disponible;

            await context.SaveChangesAsync();
            return Ok(new { message = "Créneau modifié." });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "PRESTATAIRE")]
        public async Task<IActionResult> DeleteDisponibilite(int id)
        {
            var myUserId = GetUserId();
            var myPrestataire = await context.Prestataires.FirstOrDefaultAsync(p => p.IdUtili == myUserId);
            if (myPrestataire == null) return Unauthorized(new { message = "Profil prestataire introuvable." });

            var dispo = await context.Disponibilites.FindAsync(id);
            if(dispo == null) return NotFound();

            if (dispo.IdPres != myPrestataire.IdPres) return Forbid();

            context.Disponibilites.Remove(dispo);
            await context.SaveChangesAsync();
            return Ok(new { message = "Créneau supprimé." });
        }
    }
}