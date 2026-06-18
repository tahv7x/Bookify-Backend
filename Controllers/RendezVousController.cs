using Bookify_API.DTOs;
using Bookify_API.Models;
using Bookify_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace Bookify_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RendezVousController : BaseController
    {
        private readonly BookifyDbContext context;
        private readonly EmailService emailService;

        public RendezVousController(BookifyDbContext context, EmailService emailService)
        {
            this.context = context;
            this.emailService = emailService;
        }

        private async Task<bool> IsHourlySlotAvailable(int idPres, DateTime dateDebut, DateTime dateFin)
        {
            var dayOfWeek = dateDebut.DayOfWeek;
            string dayShort = dayOfWeek switch
            {
                DayOfWeek.Monday => "Lun",
                DayOfWeek.Tuesday => "Mar",
                DayOfWeek.Wednesday => "Mer",
                DayOfWeek.Thursday => "Jeu",
                DayOfWeek.Friday => "Ven",
                DayOfWeek.Saturday => "Sam",
                DayOfWeek.Sunday => "Dim",
                _ => ""
            };
            string dayLong = dayOfWeek switch
            {
                DayOfWeek.Monday => "Lundi",
                DayOfWeek.Tuesday => "Mardi",
                DayOfWeek.Wednesday => "Mercredi",
                DayOfWeek.Thursday => "Jeudi",
                DayOfWeek.Friday => "Vendredi",
                DayOfWeek.Saturday => "Samedi",
                DayOfWeek.Sunday => "Dimanche",
                _ => ""
            };

            var timeDebut = dateDebut.TimeOfDay;
            var timeFin = dateFin.TimeOfDay;

            var hasAnyDispo = await context.Disponibilites.AnyAsync(d => d.IdPres == idPres);
            if (!hasAnyDispo) return true;

            return await context.Disponibilites
                .AnyAsync(d => d.IdPres == idPres 
                    && (d.JourSemaine == dayShort || d.JourSemaine == dayLong)
                    && d.Disponible == true
                    && d.HeureDebut <= timeDebut
                    && d.HeureFin >= timeFin);
        }

        private async Task<bool> HasOverlappingAcceptedRendezVous(int idPres, DateTime start, DateTime end, int? excludeRdvId = null)
        {
            return await context.RendezVous
                .AnyAsync(r => 
                    r.IdPres == idPres 
                    && r.Statut == "ACCEPTE" 
                    && r.DateDebut < end 
                    && (r.DateFin ?? r.DateDebut.AddHours(1)) > start 
                    && (!excludeRdvId.HasValue || r.IdRendezVous != excludeRdvId.Value));
        }

        private async Task<bool> IsFullDaySlotAvailable(int idPres, DateTime start, DateTime end)
        {
            var hasAnyDispo = await context.Disponibilites.AnyAsync(d => d.IdPres == idPres);
            if (!hasAnyDispo) return true;

            var currentDay = start.Date;
            var lastDay = end.Date;

            while (currentDay <= lastDay)
            {
                var dayOfWeek = currentDay.DayOfWeek;
                string dayShort = dayOfWeek switch
                {
                    DayOfWeek.Monday => "Lun",
                    DayOfWeek.Tuesday => "Mar",
                    DayOfWeek.Wednesday => "Mer",
                    DayOfWeek.Thursday => "Jeu",
                    DayOfWeek.Friday => "Ven",
                    DayOfWeek.Saturday => "Sam",
                    DayOfWeek.Sunday => "Dim",
                    _ => ""
                };
                string dayLong = dayOfWeek switch
                {
                    DayOfWeek.Monday => "Lundi",
                    DayOfWeek.Tuesday => "Mardi",
                    DayOfWeek.Wednesday => "Mercredi",
                    DayOfWeek.Thursday => "Jeudi",
                    DayOfWeek.Friday => "Vendredi",
                    DayOfWeek.Saturday => "Samedi",
                    DayOfWeek.Sunday => "Dimanche",
                    _ => ""
                };

                var isDayAvailable = await context.Disponibilites
                    .AnyAsync(d => d.IdPres == idPres 
                        && (d.JourSemaine == dayShort || d.JourSemaine == dayLong)
                        && d.Disponible == true);

                if (!isDayAvailable) return false;

                currentDay = currentDay.AddDays(1);
            }

            return true;
        }

        private async Task<(DateTime Start, DateTime End)?> FindNextAvailableSlot(int idPres, DateTime searchFrom, int duree, bool isFullDay, int excludeRdvId)
        {
            var today = searchFrom.Date < DateTime.Today ? DateTime.Today : searchFrom.Date;

            if (isFullDay)
            {
                for (int dayOffset = 0; dayOffset < 30; dayOffset++)
                {
                    var start = today.AddDays(dayOffset).Date.AddHours(9);
                    var end = start.Date.AddDays(duree).AddHours(9).AddTicks(-1);

                    if (await IsFullDaySlotAvailable(idPres, start, end))
                    {
                        if (!await HasOverlappingAcceptedRendezVous(idPres, start, end, excludeRdvId))
                        {
                            return (start, end);
                        }
                    }
                }
            }
            else
            {
                var timeSlots = new[] { 
                    new TimeSpan(8, 0, 0), new TimeSpan(9, 0, 0), new TimeSpan(10, 0, 0), 
                    new TimeSpan(11, 0, 0), new TimeSpan(12, 0, 0), new TimeSpan(13, 0, 0), 
                    new TimeSpan(14, 0, 0), new TimeSpan(15, 0, 0), new TimeSpan(16, 0, 0), 
                    new TimeSpan(17, 0, 0), new TimeSpan(18, 0, 0) 
                };
                
                for (int dayOffset = 0; dayOffset < 30; dayOffset++)
                {
                    var date = today.AddDays(dayOffset);
                    foreach (var ts in timeSlots)
                    {
                        var start = date.Add(ts);
                        if (start < DateTime.Now || start < searchFrom) continue;
                        var end = start.AddMinutes(duree);

                        if (await IsHourlySlotAvailable(idPres, start, end))
                        {
                            if (!await HasOverlappingAcceptedRendezVous(idPres, start, end, excludeRdvId))
                            {
                                return (start, end);
                            }
                        }
                    }
                }
            }
            return null;
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> Get(int id)
        {
            var rdv = await context.RendezVous
                .Include(r => r.IdSerNavigation)
                .Include(r => r.IdPresNavigation)
                    .ThenInclude(p => p.IdUtiliNavigation)
                .FirstOrDefaultAsync(r => r.IdRendezVous == id);

            if (rdv == null) return NotFound();

            return Ok(new
            {
                idRendezVous = rdv.IdRendezVous,
                dateDebut = rdv.DateDebut,
                dateFin = rdv.DateFin,
                statut = rdv.Statut,
                lieu = rdv.Lieu,
                prestataire = new {
                    idPres = rdv.IdPresNavigation.IdPres,
                    nom = rdv.IdPresNavigation.IdUtiliNavigation.NomComplet,
                    avatar = rdv.IdPresNavigation.IdUtiliNavigation.Avatar,
                    specialite = rdv.IdPresNavigation.Speciallite
                },
                service = new {
                    idService = rdv.IdSerNavigation.IdService,
                    nom = rdv.IdSerNavigation.Nom,
                    prix = rdv.IdSerNavigation.Prix,
                    duree = rdv.IdSerNavigation.Duree,
                    isFullDay = rdv.IdSerNavigation.IsFullDay
                }
            });
        }

        [HttpPost]
        [Authorize(Roles = "CLIENT")]
        public async Task<IActionResult> Create(CreateRendezVous dto)
        {
            if (dto.DateDebut.Kind == DateTimeKind.Utc) dto.DateDebut = dto.DateDebut.ToLocalTime();
            if (dto.DateFin.Kind == DateTimeKind.Utc) dto.DateFin = dto.DateFin.ToLocalTime();

            var clientIdStr = User.FindFirst("id")?.Value;
            if (clientIdStr == null) return Unauthorized();
            int clientId = int.Parse(clientIdStr);
            var client = await context.Utilisateurs.FindAsync(clientId);

            var prestataire = await context.Prestataires
                .Include(p => p.IdUtiliNavigation)
                .FirstOrDefaultAsync(p => p.IdPres == dto.idPres);
            if (prestataire == null)
            {
                return NotFound(new { message = "Prestataire introuvable" });
            }

            var service = await context.Services.FirstOrDefaultAsync(s => s.IdService == dto.idServ && s.IdPres == dto.idPres);
            if (service == null)
            {
                return NotFound(new { message = "Service Introuvable" });
            }

            var isFullDayService = service.IsFullDay;
            var requestedDay = dto.DateDebut.Date;

            if (requestedDay < DateTime.Today)
            {
                return BadRequest(new { message = "la date doit etre dans le futur" });
            }

            DateTime finalDateDebut;
            DateTime finalDateFin;

            if (isFullDayService)
            {
                finalDateDebut = requestedDay.AddHours(9);
                int daysToAdd = (service.UniteDuree == "JOUR") ? service.Duree : 1;
                finalDateFin = requestedDay.AddDays(daysToAdd).AddHours(9).AddTicks(-1);
            }
            else
            {
                if (dto.DateDebut >= dto.DateFin)
                {
                    return BadRequest(new { message = "la date debut doit etre avant la date de fin" });
                }
                if (dto.DateDebut < DateTime.Now)
                {
                    return BadRequest(new { message = "la date doit etre dans le futur" });
                }

                finalDateDebut = dto.DateDebut;
                finalDateFin = dto.DateFin;
            }

            var hasOverlap = await HasOverlappingAcceptedRendezVous(dto.idPres, finalDateDebut, finalDateFin);
            if (hasOverlap)
            {
                return BadRequest(new { message = "Ce créneau horaire chevauche un autre rendez-vous accepté." });
            }

            if (!isFullDayService)
            {
                var isAvailable = await IsHourlySlotAvailable(dto.idPres, finalDateDebut, finalDateFin);
                if (!isAvailable)
                {
                    return BadRequest(new { message = "Le prestataire n'est pas disponible pour ce créneau horaire." });
                }
            }

            var rdv = new RendezVou
            {
                IdUtili = clientId,
                IdPres = dto.idPres,
                IdSer = dto.idServ,
                DateDebut = finalDateDebut,
                DateFin = finalDateFin,
                Lieu = dto.Lieu,
                Statut = "EN_ATTENTE",
                DateCreation = DateTime.Now
            };
            context.RendezVous.Add(rdv);
            var result = await SaveAsyncChanges(context, () => new { message = "Rendez-vous créé avec succès", rdv.IdRendezVous });
            if (result is OkObjectResult && client != null && prestataire?.IdUtiliNavigation != null)
            {
                try
                {
                    string dateStr = rdv.DateDebut.ToString("dd/MM/yyyy");
                    string timeStr = $"{rdv.DateDebut:HH:mm} - {rdv.DateFin?.ToString("HH:mm")}";

                    var clientHtml = emailService.BuildRendezVousEmail(
                        client.NomComplet, "Demande envoyée", "En attente", "#1A6FD1",
                        $"Votre demande a bien été envoyée à {prestataire.IdUtiliNavigation.NomComplet}.",
                        service.Nom, "Prestataire", prestataire.IdUtiliNavigation.NomComplet, dateStr, timeStr);
                    emailService.Send(client.Email, "Demande envoyée - Bookify", clientHtml, true);

                    var presHtml = emailService.BuildRendezVousEmail(
                        prestataire.IdUtiliNavigation.NomComplet, "Nouvelle demande", "Action requise", "#D97706",
                        $"Vous avez reçu une demande de {client.NomComplet}.",
                        service.Nom, "Client", client.NomComplet, dateStr, timeStr);
                    emailService.Send(prestataire.IdUtiliNavigation.Email, "Nouvelle demande - Bookify", presHtml, true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur Email: {ex.Message}");
                }
            }
            return result;
        }

        [HttpGet("prestataire/{prestataireId}/occupied")]
        public async Task<IActionResult> GetOccupiedSlots(int prestataireId)
        {
            var occupied = await context.RendezVous
                .Where(r => r.IdPres == prestataireId && r.Statut == "ACCEPTE" && r.DateDebut >= DateTime.Today.AddDays(-1))
                .Select(r => new {
                    dateDebut = r.DateDebut,
                    dateFin = r.DateFin
                })
                .ToListAsync();

            return Ok(occupied);
        }

        [HttpGet("client/{id}")]
        [Authorize(Roles = "CLIENT")]
        public async Task<IActionResult> GetByClient(int id)
        {
            var tokenId = User.FindFirst("id")?.Value;
            if (tokenId == null || tokenId != id.ToString())
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
                .OrderByDescending(r => r.DateCreation)
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
                .Include(r => r.IdPresNavigation).ThenInclude(p => p.IdUtiliNavigation)
                .Include(r => r.IdUtiliNavigation)
                .Include(r => r.IdSerNavigation)
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

            var finalFin = rdv.DateFin ?? rdv.DateDebut.AddHours(1);
            if (rdv.IdSerNavigation.IsFullDay)
            {
                int daysToAdd = (rdv.IdSerNavigation.UniteDuree == "JOUR") ? rdv.IdSerNavigation.Duree : 1;
                finalFin = rdv.DateDebut.AddDays(daysToAdd).AddTicks(-1);
            }

            var hasOverlap = await HasOverlappingAcceptedRendezVous(rdv.IdPres, rdv.DateDebut, finalFin, rdv.IdRendezVous);
            if (hasOverlap)
            {
                return BadRequest(new { message = "Ce créneau chevauche un autre rendez-vous accepté." });
            }

            rdv.Statut = "ACCEPTE";
            await context.SaveChangesAsync(); 

            // AUTO-RESCHEDULING ENGINE
            var overlappingPending = await context.RendezVous
                .Include(r => r.IdSerNavigation)
                .Where(r => r.IdPres == rdv.IdPres 
                         && r.Statut == "EN_ATTENTE" 
                         && r.IdRendezVous != rdv.IdRendezVous
                         && r.DateDebut < finalFin 
                         && (r.DateFin ?? r.DateDebut.AddHours(1)) > rdv.DateDebut)
                .ToListAsync();

            foreach (var pendingRdv in overlappingPending)
            {
                var isPendingFullDay = pendingRdv.IdSerNavigation.IsFullDay;
                var duree = pendingRdv.IdSerNavigation.Duree;

                // Le client veut qu'on passe au jour suivant directement
                var searchFrom = finalFin.Date.AddDays(1);
                
                // Si c'est FullDay, FindNextAvailableSlot attend le nombre de jours.
                // Sinon, il attend la durée en minutes.
                int slotDuree;
                if (isPendingFullDay)
                {
                    slotDuree = (pendingRdv.IdSerNavigation.UniteDuree == "JOUR") ? duree : 1;
                }
                else
                {
                    slotDuree = (pendingRdv.IdSerNavigation.UniteDuree == "HEURE") ? (duree * 60) : duree;
                }

                var newSlot = await FindNextAvailableSlot(pendingRdv.IdPres, searchFrom, slotDuree, isPendingFullDay, pendingRdv.IdRendezVous);
                if (newSlot.HasValue)
                {
                    var newStart = newSlot.Value.Start;
                    var newEnd = newSlot.Value.End;

                    var messageContent = $"BOOKIFY_AUTO_PROPOSAL|{pendingRdv.IdRendezVous}|{newStart:yyyy-MM-ddTHH:mm}|{newEnd:yyyy-MM-ddTHH:mm}|Votre rendez-vous a dû être décalé.";
                    context.Messages.Add(new Message
                    {
                        IdEnvoyeur = rdv.IdPresNavigation.IdUtili,
                        IdReceveur = pendingRdv.IdUtili,
                        Contenu = messageContent,
                        Lu = false
                    });

                    await context.Notifications.AddAsync(new Notification
                    {
                        UtilisateurId = pendingRdv.IdUtili,
                        Title = "Modification de votre rendez-vous",
                        Message = $"Le prestataire a dû déplacer votre rendez-vous. Veuillez vérifier vos messages pour accepter ou choisir un autre créneau.",
                        IsRead = false,
                        RendezVousId = pendingRdv.IdRendezVous
                    });

                    pendingRdv.DateDebut = newStart;
                    pendingRdv.DateFin = newEnd;
                }
            }

            await context.Notifications.AddAsync(new Notification
            {
                UtilisateurId = rdv.IdUtili,
                Title = "Rendez-Vous Accepté",
                Message = $"Votre rendez-vous du {rdv.DateDebut:dd/MM/yyyy à HH:mm} a été confirmé",
                IsRead = false,
                RendezVousId = rdv.IdRendezVous
            });

            var result = await SaveAsyncChanges(context, new { Message = "Rendez-Vous accepté" });
            if (result is OkObjectResult && rdv.IdUtiliNavigation != null && rdv.IdPresNavigation?.IdUtiliNavigation != null)
            {
                try
                {
                    string dateStr = rdv.DateDebut.ToString("dd/MM/yyyy");
                    string timeStr = $"{rdv.DateDebut:HH:mm} - {rdv.DateFin?.ToString("HH:mm")}";
                    var clientHtml = emailService.BuildRendezVousEmail(
                        rdv.IdUtiliNavigation.NomComplet, "Rendez-vous confirmé", "Accepté", "#10B981",
                        $"Bonne nouvelle ! {rdv.IdPresNavigation.IdUtiliNavigation.NomComplet} a accepté votre demande de rendez-vous.",
                        rdv.IdSerNavigation?.Nom ?? "Prestation", "Prestataire", rdv.IdPresNavigation.IdUtiliNavigation.NomComplet, dateStr, timeStr);
                    emailService.Send(rdv.IdUtiliNavigation.Email, "Confirmation de rendez-vous - Bookify", clientHtml, true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur Email: {ex.Message}");
                }
            }
            return result;
        }

        [HttpPut("{id}/refuse")]
        [Authorize(Roles = "PRESTATAIRE")]
        public async Task<IActionResult> Refuse(int id)
        {
            var rdv = await context.RendezVous
                .Include(r => r.IdPresNavigation).ThenInclude(p => p.IdUtiliNavigation)
                .Include(r => r.IdUtiliNavigation)
                .Include(r => r.IdSerNavigation)
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
            var result = await SaveAsyncChanges(context, new { message = "Rendez-vous refusé" });
            if (result is OkObjectResult && rdv.IdUtiliNavigation != null && rdv.IdPresNavigation?.IdUtiliNavigation != null)
            {
                try
                {
                    string dateStr = rdv.DateDebut.ToString("dd/MM/yyyy");
                    string timeStr = $"{rdv.DateDebut:HH:mm} - {rdv.DateFin?.ToString("HH:mm")}";
                    var clientHtml = emailService.BuildRendezVousEmail(
                        rdv.IdUtiliNavigation.NomComplet, "Rendez-vous refusé", "Refusé", "#EF4444",
                        $"Nous sommes désolés, {rdv.IdPresNavigation.IdUtiliNavigation.NomComplet} n'est pas disponible pour ce créneau.",
                        rdv.IdSerNavigation?.Nom ?? "Prestation", "Prestataire", rdv.IdPresNavigation.IdUtiliNavigation.NomComplet, dateStr, timeStr);
                    emailService.Send(rdv.IdUtiliNavigation.Email, "Rendez-vous refusé - Bookify", clientHtml, true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur Email: {ex.Message}");
                }
            }
            return result;
        }

        [HttpPut("{id}/propose-alternative")]
        [Authorize(Roles = "PRESTATAIRE")]
        public async Task<IActionResult> ProposeAlternative(int id, [FromBody] ProposeAlternativeDto dto)
        {
            var rdv = await context.RendezVous
                .Include(r => r.IdPresNavigation)
                .Include(r => r.IdUtiliNavigation)
                .Include(r => r.IdSerNavigation)
                .FirstOrDefaultAsync(r => r.IdRendezVous == id);

            if (rdv == null) return NotFound(new { message = "Rendez-vous introuvable" });

            var tokenId = int.Parse(User.FindFirst("id")!.Value);
            if (rdv.IdPresNavigation.IdUtili != tokenId) return Forbid();

            if (rdv.Statut != "EN_ATTENTE")
                return BadRequest(new { message = "Seuls les rendez-vous en attente peuvent être négociés" });

            var proposedDay = dto.ProposedDate;
            DateTime proposedEnd;

            if (rdv.IdSerNavigation.IsFullDay)
            {
                if (proposedDay.Date < DateTime.Today)
                    return BadRequest(new { message = "La date proposée doit être dans le futur." });
                proposedDay = proposedDay.Date.AddHours(9);
                int daysToAdd = (rdv.IdSerNavigation.UniteDuree == "JOUR") ? rdv.IdSerNavigation.Duree : 1;
                proposedEnd = proposedDay.Date.AddDays(daysToAdd).AddHours(9).AddTicks(-1);
            }
            else
            {
                if (proposedDay < DateTime.Now)
                    return BadRequest(new { message = "La date proposée doit être dans le futur." });
                if (!dto.ProposedEndDate.HasValue)
                    return BadRequest(new { message = "La date de fin proposée est requise pour les services horaires." });
                proposedEnd = dto.ProposedEndDate.Value;
            }

            var hasOverlap = await HasOverlappingAcceptedRendezVous(rdv.IdPres, proposedDay, proposedEnd, rdv.IdRendezVous);
            if (hasOverlap)
                return BadRequest(new { message = "Ce créneau chevauche un autre rendez-vous accepté." });

            if (!rdv.IdSerNavigation.IsFullDay)
            {
                var isAvailable = await IsHourlySlotAvailable(rdv.IdPres, proposedDay, proposedEnd);
                if (!isAvailable)
                    return BadRequest(new { message = "Le prestataire n'est pas disponible pour ce créneau horaire." });
            }

            var messageContent = $"BOOKIFY_PROPOSAL|{rdv.IdRendezVous}|{proposedDay:yyyy-MM-ddTHH:mm}|{proposedEnd:yyyy-MM-ddTHH:mm}|{(dto.MessageContent ?? string.Empty).Trim()}";
            context.Messages.Add(new Message
            {
                IdEnvoyeur = tokenId,
                IdReceveur = rdv.IdUtili,
                Contenu = messageContent,
                Lu = false
            });

            await context.Notifications.AddAsync(new Notification
            {
                UtilisateurId = rdv.IdUtili,
                Title = "Proposition de nouvelle date",
                Message = $"{rdv.IdPresNavigation.IdUtiliNavigation?.NomComplet ?? "Votre prestataire"} propose de déplacer votre rendez-vous au {proposedDay:dd/MM/yyyy HH:mm}.",
                IsRead = false,
                RendezVousId = rdv.IdRendezVous
            });

            await context.SaveChangesAsync();
            return Ok(new { message = "Proposition envoyée au client." });
        }

        [HttpPut("{id}/accept-proposal")]
        [Authorize(Roles = "CLIENT")]
        public async Task<IActionResult> AcceptProposal(int id, [FromBody] AcceptProposalDto dto)
        {
            if (dto.ProposedDate.Kind == DateTimeKind.Utc) dto.ProposedDate = dto.ProposedDate.ToLocalTime();
            if (dto.ProposedEndDate.HasValue && dto.ProposedEndDate.Value.Kind == DateTimeKind.Utc) dto.ProposedEndDate = dto.ProposedEndDate.Value.ToLocalTime();

            var rdv = await context.RendezVous
                .Include(r => r.IdPresNavigation)
                    .ThenInclude(p => p.IdUtiliNavigation)
                .Include(r => r.IdUtiliNavigation)
                .Include(r => r.IdSerNavigation)
                .FirstOrDefaultAsync(r => r.IdRendezVous == id);

            if (rdv == null) return NotFound(new { message = "Rendez-vous introuvable" });

            var tokenId = int.Parse(User.FindFirst("id")!.Value);
            if (rdv.IdUtili != tokenId) return Forbid();

            if (rdv.Statut != "EN_ATTENTE")
                return BadRequest(new { message = "Ce rendez-vous ne peut plus être modifié" });

            var proposedDay = dto.ProposedDate;
            DateTime proposedEnd;

            if (rdv.IdSerNavigation.IsFullDay)
            {
                if (proposedDay.Date < DateTime.Today)
                    return BadRequest(new { message = "La date proposée doit être dans le futur." });
                proposedDay = proposedDay.Date.AddHours(9);
                int daysToAdd = (rdv.IdSerNavigation.UniteDuree == "JOUR") ? rdv.IdSerNavigation.Duree : 1;
                proposedEnd = proposedDay.Date.AddDays(daysToAdd).AddHours(9).AddTicks(-1);
            }
            else
            {
                if (proposedDay < DateTime.Now)
                    return BadRequest(new { message = "La date proposée doit être dans le futur." });
                if (!dto.ProposedEndDate.HasValue)
                    return BadRequest(new { message = "La date de fin proposée est requise." });
                proposedEnd = dto.ProposedEndDate.Value;
            }

            var hasOverlap = await HasOverlappingAcceptedRendezVous(rdv.IdPres, proposedDay, proposedEnd, rdv.IdRendezVous);
            if (hasOverlap)
                return BadRequest(new { message = "Ce créneau chevauche un autre rendez-vous accepté." });

            if (!rdv.IdSerNavigation.IsFullDay)
            {
                var isAvailable = await IsHourlySlotAvailable(rdv.IdPres, proposedDay, proposedEnd);
                if (!isAvailable)
                    return BadRequest(new { message = "Le prestataire n'est pas disponible pour ce créneau horaire." });
            }

            rdv.DateDebut = proposedDay;
            rdv.DateFin = proposedEnd;
            rdv.Statut = "ACCEPTE";

            context.Messages.Add(new Message
            {
                IdEnvoyeur = tokenId,
                IdReceveur = rdv.IdPresNavigation.IdUtili,
                Contenu = $"BOOKIFY_PROPOSAL_ACCEPTED|{rdv.IdRendezVous}|{proposedDay:yyyy-MM-ddTHH:mm}|{proposedEnd:yyyy-MM-ddTHH:mm}",
                Lu = false
            });

            await context.Notifications.AddAsync(new Notification
            {
                UtilisateurId = rdv.IdPresNavigation.IdUtili,
                Title = "Nouvelle date acceptée",
                Message = $"{rdv.IdUtiliNavigation.NomComplet} a accepté la nouvelle date du {proposedDay:dd/MM/yyyy HH:mm}.",
                IsRead = false,
                RendezVousId = rdv.IdRendezVous
            });

            var result = await SaveAsyncChanges(context, new { message = "Nouvelle date acceptée et rendez-vous confirmé." });
            return result;
        }

        [HttpPut("{id}/reschedule")]
        [Authorize(Roles = "CLIENT")]
        public async Task<IActionResult> Reschedule(int id, [FromBody] RescheduleDto dto)
        {
            if (dto.DateDebut.Kind == DateTimeKind.Utc) dto.DateDebut = dto.DateDebut.ToLocalTime();
            if (dto.DateFin.Kind == DateTimeKind.Utc) dto.DateFin = dto.DateFin.ToLocalTime();

            var rdv = await context.RendezVous
                .Include(r => r.IdPresNavigation)
                    .ThenInclude(p => p.IdUtiliNavigation)
                .Include(r => r.IdUtiliNavigation)
                .Include(r => r.IdSerNavigation)
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

            var hasOverlap = await HasOverlappingAcceptedRendezVous(rdv.IdPres, dto.DateDebut, dto.DateFin, rdv.IdRendezVous);
            if (hasOverlap)
                return BadRequest(new { message = "Ce créneau chevauche un autre rendez-vous accepté." });

            if (!rdv.IdSerNavigation.IsFullDay)
            {
                var isAvailable = await IsHourlySlotAvailable(rdv.IdPres, dto.DateDebut, dto.DateFin);
                if (!isAvailable)
                    return BadRequest(new { message = "Le prestataire n'est pas disponible pour ce créneau horaire." });
            }

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

            var result = await SaveAsyncChanges(context, new { message = "Rendez-vous reprogrammé avec succès" });
            if (result is OkObjectResult && rdv.IdUtiliNavigation != null && rdv.IdPresNavigation?.IdUtiliNavigation != null)
            {
                try
                {
                    string dateStr = rdv.DateDebut.ToString("dd/MM/yyyy");
                    string timeStr = $"{rdv.DateDebut:HH:mm} - {rdv.DateFin?.ToString("HH:mm")}";

                    var clientHtml = emailService.BuildRendezVousEmail(
                        rdv.IdUtiliNavigation.NomComplet, "Demande modifiée", "En attente", "#1A6FD1",
                        $"Votre demande a bien été envoyée à {rdv.IdPresNavigation.IdUtiliNavigation.NomComplet}.",
                        rdv.IdSerNavigation?.Nom ?? "Prestation", "Prestataire", rdv.IdPresNavigation.IdUtiliNavigation.NomComplet, dateStr, timeStr);
                    emailService.Send(rdv.IdUtiliNavigation.Email, "Demande modifiée - Bookify", clientHtml, true);

                    var presHtml = emailService.BuildRendezVousEmail(
                        rdv.IdPresNavigation.IdUtiliNavigation.NomComplet, "Demande modifiée", "Action requise", "#D97706",
                        $"{rdv.IdUtiliNavigation.NomComplet} a modifié la date de sa demande.",
                        rdv.IdSerNavigation?.Nom ?? "Prestation", "Client", rdv.IdUtiliNavigation.NomComplet, dateStr, timeStr);
                    emailService.Send(rdv.IdPresNavigation.IdUtiliNavigation.Email, "Demande modifiée - Bookify", presHtml, true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur Email: {ex.Message}");
                }
            }
            return result;
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "CLIENT")]
        public async Task<IActionResult> Cancel(int id)
        {
            var rdv = await context.RendezVous
                .Include(r => r.IdPresNavigation)
                    .ThenInclude(p => p.IdUtiliNavigation)
                .Include(r => r.IdUtiliNavigation)
                .Include(r => r.IdSerNavigation)
                .FirstOrDefaultAsync(r => r.IdRendezVous == id);
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
            var result = await SaveAsyncChanges(context, new { message = "Rendez-vous annulé" });
            if (result is OkObjectResult && rdv.IdUtiliNavigation != null && rdv.IdPresNavigation?.IdUtiliNavigation != null)
            {
                try
                {
                    string dateStr = rdv.DateDebut.ToString("dd/MM/yyyy");
                    string timeStr = $"{rdv.DateDebut:HH:mm} - {rdv.DateFin?.ToString("HH:mm")}";

                    var clientHtml = emailService.BuildRendezVousEmail(
                        rdv.IdUtiliNavigation.NomComplet, "Rendez-vous annulé", "Annulé", "#6B7280",
                        $"Votre rendez-vous a bien été annulé.",
                        rdv.IdSerNavigation?.Nom ?? "Prestation", "Prestataire", rdv.IdPresNavigation.IdUtiliNavigation.NomComplet, dateStr, timeStr);
                    emailService.Send(rdv.IdUtiliNavigation.Email, "Rendez-vous annulé - Bookify", clientHtml, true);

                    var presHtml = emailService.BuildRendezVousEmail(
                        rdv.IdPresNavigation.IdUtiliNavigation.NomComplet, "Rendez-vous annulé", "Annulé", "#6B7280",
                        $"{rdv.IdUtiliNavigation.NomComplet} a annulé son rendez-vous.",
                        rdv.IdSerNavigation?.Nom ?? "Prestation", "Client", rdv.IdUtiliNavigation.NomComplet, dateStr, timeStr);
                    emailService.Send(rdv.IdPresNavigation.IdUtiliNavigation.Email, "Rendez-vous annulé - Bookify", presHtml, true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur Email: {ex.Message}");
                }
            }
            return result;
        }
    }
}
