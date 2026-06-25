using System;
using System.Linq;
using Bookify_API.DTOs;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Bookify_API.Models;

namespace Bookify_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AvisController : BaseController
    {
        private readonly BookifyDbContext _context;

        public AvisController(BookifyDbContext context)
        {
            _context = context;
        }

        [HttpGet("prestataire/{prestataireId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAvisByPrestataire(int prestataireId)
        {
            var avisList = await _context.Avis
                .Include(a => a.Utilisateur)
                .Where(a => a.IdPrestataire == prestataireId)
                .OrderByDescending(a => a.DateCreation)
                .Select(a => new
                {
                    id = a.IdAvis,
                    providerId = a.IdPrestataire,
                    clientName = a.Utilisateur.NomComplet ?? a.Utilisateur.Email,
                    clientAvatar = a.Utilisateur.Avatar,
                    rating = a.Note,
                    comment = a.Commentaire,
                    date = a.DateCreation.ToString("dd MMMM yyyy")
                })
                .ToListAsync();

            return Ok(avisList);
        }

        [HttpGet("client/{clientId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAvisByClient(int clientId)
        {
            var avisList = await _context.Avis
                .Include(a => a.Prestataire)
                .Include(a => a.RendezVous)
                .ThenInclude(r => r.IdSerNavigation) // assuming IdSerNavigation is Service
                .Where(a => a.IdUtilisateur == clientId)
                .OrderByDescending(a => a.DateCreation)
                .Select(a => new
                {
                    id = a.IdAvis,
                    providerId = a.IdPrestataire,
                    providerName = a.Prestataire.IdUtiliNavigation != null ? a.Prestataire.IdUtiliNavigation.NomComplet : "Prestataire", // The user for provider is IdUtiliNavigation
                    service = a.RendezVous != null && a.RendezVous.IdSerNavigation != null ? a.RendezVous.IdSerNavigation.Nom : "Service",
                    date = a.DateCreation.ToString("dd MMMM yyyy"),
                    rating = a.Note,
                    comment = a.Commentaire
                })
                .ToListAsync();

            return Ok(avisList);
        }



        [HttpPost]
        public async Task<IActionResult> CreateAvis([FromBody] CreateAvisDto dto)
        {
            var clientId = GetUserId();

            if (dto.Note < 1 || dto.Note > 5)
            {
                return BadRequest(new { message = "La note doit être comprise entre 1 et 5." });
            }

            // Update past appointments to TERMINE before checking
            var now = DateTime.Now;
            var pastRdvs = await _context.RendezVous
                .Where(r => (r.Statut == "EN_ATTENTE" || r.Statut == "ACCEPTE")
                    && r.IdUtili == clientId
                    && r.IdPres == dto.IdPrestataire
                    && r.DateDebut < now)
                .ToListAsync();
            if (pastRdvs.Count > 0)
            {
                foreach (var r in pastRdvs) r.Statut = "TERMINE";
                await _context.SaveChangesAsync();
            }

            bool hadAppointment;
            if (dto.IdRendezVous != null)
            {
                // Verify the specific rendez-vous belongs to the client and prestataire
                hadAppointment = await _context.RendezVous.AnyAsync(r =>
                    r.IdRendezVous == dto.IdRendezVous
                    && r.IdUtili == clientId
                    && r.IdPres == dto.IdPrestataire
                    && (r.Statut == "TERMINE" || r.Statut == "ACCEPTE"));
            }
            else
            {
                // No specific RDV provided, check for any qualifying appointment
                hadAppointment = await _context.RendezVous.AnyAsync(r =>
                    r.IdUtili == clientId
                    && r.IdPres == dto.IdPrestataire
                    && (r.Statut == "TERMINE" || r.Statut == "ACCEPTE"));
            }

            if (!hadAppointment)
            {
                return BadRequest(new { message = "Vous ne pouvez évaluer que les prestataires avec qui vous avez eu un rendez-vous terminé ou accepté." });
            }

            var nouvelAvis = new Avis
            {
                IdUtilisateur = clientId,
                IdPrestataire = dto.IdPrestataire,
                IdRendezVous = dto.IdRendezVous,
                Note = dto.Note,
                Commentaire = dto.Commentaire,
                DateCreation = DateTime.Now
            };

            _context.Avis.Add(nouvelAvis);
            await _context.SaveChangesAsync();

            // Now calculate average AFTER the new avis is saved in DB
            var avg = await _context.Avis
                .Where(a => a.IdPrestataire == dto.IdPrestataire)
                .AverageAsync(a => (double?)a.Note) ?? 0;

            var prestataire = await _context.Prestataires.FindAsync(dto.IdPrestataire);
            if (prestataire != null)
            {
                prestataire.Note = (decimal)avg;
                await _context.SaveChangesAsync();
            }

            return Ok(new { message = "Avis ajouté avec succès.", avisId = nouvelAvis.IdAvis });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAvis(int id)
        {
            var avis = await _context.Avis.FindAsync(id);
            if (avis == null)
            {
                return NotFound(new { message = "Avis non trouvé." });
            }

            var userId = GetUserId();
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            if (avis.IdUtilisateur != userId && userRole != "ADMIN")
            {
                return Forbid();
            }

            var prestataireId = avis.IdPrestataire;
            _context.Avis.Remove(avis);
            await _context.SaveChangesAsync();

            // Recalculate average AFTER deletion is saved
            var prestataire = await _context.Prestataires.FindAsync(prestataireId);
            if (prestataire != null)
            {
                var avg = await _context.Avis
                    .Where(a => a.IdPrestataire == prestataireId)
                    .AverageAsync(a => (double?)a.Note) ?? 0;
                prestataire.Note = (decimal)avg;
                await _context.SaveChangesAsync();
            }

            return Ok(new { message = "Avis supprimé avec succès." });
        }
    }
}
