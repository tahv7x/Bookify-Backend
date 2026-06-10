using Bookify_API.DTOs;
using Bookify_API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
namespace Bookify_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RendezVousController : BaseController
    {
        private readonly BookifyDbContext context;
        public RendezVousController(BookifyDbContext context)
        {
            this.context = context;
        }

        [HttpPost]
        [Authorize(Roles = "CLIENT")]
        public async Task<IActionResult>Create(CreateRendezVous dto)
        {
            var clientIdStr = User.FindFirst("id")?.Value;
            if (clientIdStr == null) return Unauthorized();
            int clientId = int.Parse(clientIdStr);

            var prestataire = await context.Prestataires.FindAsync(dto.idPres);
            if(prestataire == null)
            {
                return NotFound(new { message = "Prestataire introuvable" });
            }

            var service = await context.Services.FirstOrDefaultAsync(s => s.IdService == dto.idServ && s.IdPres == dto.idPres);
            if(service == null)
            {
                return NotFound(new { message = "Service Introuvable" });
            }
            if(dto.DateDebut >= dto.DateFin)
            {
                return BadRequest(new { message = "la date debut doit etre avant la date de fin" });
            }
            if(dto.DateDebut < DateTime.Now)
            {
                return BadRequest(new { message = "la date doit etre dans le futur" });
            }
            var rdv = new RendezVou
            {
                IdUtili = clientId,
                IdPres = dto.idPres,
                IdSer = dto.idServ,
                DateDebut = dto.DateDebut,
                DateFin = dto.DateFin,
                Statut = "EN_ATTENTE",
                DateCreation = DateTime.Now
            };
            context.RendezVous.Add(rdv);
            return await SaveAsyncChanges(context, new { message = "Rendez-vous créé avec succès", rdv.IdRendezVous });
        }
        [HttpGet("client/{id}")]
        [Authorize(Roles = "CLIENT")]
        public async Task<IActionResult>GetByClient(int id)
        {
            var tokenId = User.FindFirst("id")?.Value;
            if(tokenId == null || tokenId != id.ToString())
            {
                return Forbid();
            }
            var rdvs = await context.RendezVous
                .Where(r => r.IdUtili == id)
                .Include(r => r.IdPresNavigation)
                    .ThenInclude(p => p.IdUtiliNavigation)
                .Include(r => r.IdSerNavigation)
                    .OrderByDescending(r => r.DateCreation)
                 .Select(r => new
                 {
                     r.IdRendezVous,
                     r.DateDebut,
                     r.DateFin,
                     r.Statut,
                     r.DateCreation,
                     Service = new
                     {
                         r.IdSerNavigation.IdService,
                         r.IdSerNavigation.Nom,
                         r.IdSerNavigation.Prix
                     },
                     prestataire = new
                     {
                         r.IdPresNavigation.IdPres,
                         nomComplet = r.IdPresNavigation.IdUtiliNavigation.NomComplet,
                         email = r.IdPresNavigation.IdUtiliNavigation.Email,
                         telephone = r.IdPresNavigation.IdUtiliNavigation.Telephone,
                         specialite = r.IdPresNavigation.Speciallite,
                         avatar = r.IdPresNavigation.IdUtiliNavigation.Avatar
                     },

                 })
                 .ToListAsync();
            return Ok(rdvs);
        }

        [HttpGet("prestataire/{id}")]
        [Authorize(Roles = "PRESTATAIRE")]
        public async Task<IActionResult> GetByPrestataire(int id)
        {
            var tokenId = User.FindFirst("id")?.Value;
            var prestataire = await context.Prestataires
                .FirstOrDefaultAsync(p => p.IdUtili == int.Parse(tokenId) && p.IdPres == id);
            if (prestataire == null) return Forbid();

            var rdvs = await context.RendezVous
                .Where(r => r.IdPres == id)
                .Include(r => r.IdUtiliNavigation)
                .Include(r => r.IdSerNavigation)
                .OrderByDescending(r=> r.DateCreation)
                .Select(r => new
                {
                    r.IdRendezVous,
                    r.DateDebut,
                    r.DateFin,
                    r.Statut,
                    r.DateCreation,
                    service = new
                    {
                        r.IdSerNavigation.Nom,
                        r.IdSerNavigation.Prix
                    },
                    client = new
                    {
                        r.IdUtiliNavigation.IdUtilisateur,
                        r.IdUtiliNavigation.NomComplet,
                        r.IdUtiliNavigation.Email,
                        r.IdUtiliNavigation.Telephone,
                        r.IdUtiliNavigation.Avatar
                    }
                })
                .ToListAsync();
            return Ok(rdvs);
        }
        [HttpPut("{id}/accept")]
        [Authorize(Roles = "PRESTATAIRE")]
        public async Task<IActionResult> Accept(int id)
        {
            var rdv = await context.RendezVous
                .Include(r => r.IdPresNavigation)
                .FirstOrDefaultAsync(r => r.IdRendezVous == id);
            if (rdv == null) return NotFound(new { message = "Rendez-Vous Introuvable" });
            var tokenId = int.Parse(User.FindFirst("id")?.Value);
            if (rdv.IdPresNavigation.IdUtili != tokenId)
            {
                return Forbid();
            }
            if (rdv.Statut != "EN_ATTENTE")
            {
                return BadRequest(new { message = "Ce rendez-vous ne peut plus être modifié" });
            }
            rdv.Statut = "ACCEPTE";
            await context.Notifications.AddAsync(new Notification
            {
                UtilisateurId = rdv.IdUtili,
                Title = "Rendez-Vous Accepté",
                Message = $"Votre rendez-vous du {rdv.DateDebut:dd/MM/yyyy à HH:mm} a été confirmé",
                IsRead = false,
                RendezVousId = rdv.IdRendezVous
            });

            return await SaveAsyncChanges(context, new { Message = "Rendez-Vous accepté" });
        }

        [HttpPut("{id}/refuse")]
        [Authorize(Roles = "PRESTATAIRE")]
        public async Task<IActionResult> Refuse(int id)
        {
            var rdv = await context.RendezVous
                .Include(r => r.IdPresNavigation)
                .FirstOrDefaultAsync(r => r.IdRendezVous == id);
            if (rdv == null) return NotFound(new { message = "Rendez-vous introuvable" });

            var tokenId = int.Parse(User.FindFirst("id")!.Value);
            if (rdv.IdPresNavigation.IdUtili != tokenId) return Forbid();

            if (rdv.Statut != "EN_ATTENTE")
                return BadRequest(new { message = "Ce rendez-vous ne peut plus être modifié" });

            rdv.Statut = "REFUSE";
            await context.Notifications.AddAsync(new Notification
            {
                UtilisateurId = rdv.IdUtili,
                Title = "Rendez-Vous Refusé",
                Message = $"Votre rendez-vous du {rdv.DateDebut:dd/MM/yyyy à HH:mm} a été refusé",
                IsRead = false,
                RendezVousId = rdv.IdRendezVous
            });
            return await SaveAsyncChanges(context, new { message = "Rendez-vous refusé" });
        }

        [HttpPut("{id}/reschedule")]
        [Authorize(Roles = "CLIENT")]
        public async Task<IActionResult> Reschedule(int id, [FromBody] RescheduleDto dto)
        {
            var rdv = await context.RendezVous
                .Include(r => r.IdPresNavigation)
                    .ThenInclude(p => p.IdUtiliNavigation)
                .Include(r => r.IdUtiliNavigation)
                .FirstOrDefaultAsync(r => r.IdRendezVous == id);
            
            if (rdv == null) return NotFound(new { message = "Rendez-vous introuvable" });

            var tokenId = int.Parse(User.FindFirst("id")!.Value);
            if (rdv.IdUtili != tokenId) return Forbid();

            if (rdv.Statut != "EN_ATTENTE")
                return BadRequest(new { message = "Seuls les rendez-vous en attente peuvent être reprogrammés" });

            if (dto.DateDebut >= dto.DateFin)
                return BadRequest(new { message = "La date de début doit être avant la date de fin" });

            if (dto.DateDebut < DateTime.Now)
                return BadRequest(new { message = "La date doit être dans le futur" });

            rdv.DateDebut = dto.DateDebut;
            rdv.DateFin = dto.DateFin;
            
            await context.Notifications.AddAsync(new Notification
            {
                UtilisateurId = rdv.IdPresNavigation.IdUtili,
                Title = "Demande de Rendez-Vous Modifiée",
                Message = $"Le client {rdv.IdUtiliNavigation.NomComplet} a modifié la date de sa demande de rendez-vous pour le {rdv.DateDebut:dd/MM/yyyy à HH:mm}",
                IsRead = false,
                RendezVousId = rdv.IdRendezVous
            });

            return await SaveAsyncChanges(context, new { message = "Rendez-vous reprogrammé avec succès" });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "CLIENT")]
        public async Task<IActionResult> Cancel(int id)
        {
            var rdv = await context.RendezVous.FindAsync(id);
            if (rdv == null) return NotFound(new { message = "Rendez-vous introuvable" });

            var tokenId = int.Parse(User.FindFirst("id")!.Value);
            if (rdv.IdUtili != tokenId) return Forbid();

            if (rdv.Statut == "TERMINE")
                return BadRequest(new { message = "Impossible d'annuler un rendez-vous terminé" });

            rdv.Statut = "ANNULE";
            await context.Notifications.AddAsync(new Notification
            {
                UtilisateurId = rdv.IdUtili,
                Title = "Rendez-Vous Annulé",
                Message = $"Votre rendez-vous du {rdv.DateDebut:dd/MM/yyyy à HH:mm} a été annulé",
                IsRead = false,
                RendezVousId = rdv.IdRendezVous
            });
            return await SaveAsyncChanges(context, new { message = "Rendez-vous annulé" });
        }
    }
}
