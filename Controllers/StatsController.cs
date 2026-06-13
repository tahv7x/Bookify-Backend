using Bookify_API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bookify_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "PRESTATAIRE")]
    public class StatsController : BaseController
    {
        private readonly BookifyDbContext context;
        public StatsController(BookifyDbContext context)
        {
            this.context = context;
        }
    
        private async Task<int?> GetAuthorizedPrestataireId(int prestataireId)
        {
            var tokenId = GetUserId();
            var prestataire = await context.Prestataires
                .FirstOrDefaultAsync(p => p.IdPres == prestataireId && p.IdUtili == tokenId);
            return prestataire?.IdPres;
        }

        [HttpGet("{prestataireId}")]
        public async Task<IActionResult> GetStats(int prestataireId)
        {
            if (await GetAuthorizedPrestataireId(prestataireId) == null)
                return Forbid();

            var now = DateTime.Now;

            // Basic stats
            var startOfMonth = new DateTime(now.Year, now.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1);

            var revenus = await context.RendezVous
                .Include(r => r.IdSerNavigation)
                .Where(r => r.IdPres == prestataireId &&
                    r.DateDebut >= startOfMonth &&
                    r.DateDebut < endOfMonth &&
                    r.Statut == "ACCEPTE"
                    )
                .SumAsync(r => (decimal?)r.IdSerNavigation.Prix) ?? 0;

            var rdvThisMonth = await context.RendezVous
                .Where(r => r.IdPres == prestataireId &&
                    r.DateDebut >= startOfMonth &&
                    r.DateDebut < endOfMonth
                ).CountAsync();

            var noteMoyenne = await context.Avis
                .Where(a => a.IdPrestataire == prestataireId)
                .AverageAsync(a => (double?)a.Note) ?? 0;

            var today = DateTime.Today;

            var areaData = new List<object>();
            for (int i = 5; i >= 0; i--)
            {
                var monthStart = new DateTime(now.Year, now.Month, 1).AddMonths(-i);
                var monthEnd = monthStart.AddMonths(1);
                var monthLabel = monthStart.ToString("MMM").ToUpperInvariant();

                var monthAppointments = await context.RendezVous
                    .Where(r => r.IdPres == prestataireId && r.DateDebut >= monthStart && r.DateDebut < monthEnd)
                    .CountAsync();

                var monthRevenues = await context.RendezVous
                    .Include(r => r.IdSerNavigation)
                    .Where(r => r.IdPres == prestataireId && r.DateDebut >= monthStart && r.DateDebut < monthEnd && r.Statut == "TERMINE")
                    .SumAsync(r => (decimal?)r.IdSerNavigation.Prix) ?? 0;

                areaData.Add(new { month = monthLabel, v1 = monthAppointments, v2 = (int)(monthRevenues / 100) }); // Scaling down revenues for chart visual
            }

            var startOfWeek = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday); // Assuming Monday start
            if (today.DayOfWeek == DayOfWeek.Sunday) startOfWeek = today.AddDays(-6);
            var barData = new List<object>();
            var days = new[] { "L", "M", "M", "J", "V", "S", "D" };
            for (int i = 0; i < 7; i++)
            {
                var dayDate = startOfWeek.AddDays(i);
                var nextDay = dayDate.AddDays(1);
                var count = await context.RendezVous
                    .Where(r => r.IdPres == prestataireId && r.DateDebut >= dayDate && r.DateDebut < nextDay)
                    .CountAsync();
                barData.Add(new { day = days[i], v = count });
            }

            var statuses = await context.RendezVous
                .Where(r => r.IdPres == prestataireId)
                .GroupBy(r => r.Statut)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            var total = statuses.Sum(s => s.Count);
            var donutData = new List<object>();
            if (total > 0)
            {
                foreach (var s in statuses)
                {
                    var percentage = (int)Math.Round((double)s.Count / total * 100);
                    string color = s.Status == "TERMINE" ? "#1e3a8a" : s.Status == "ACCEPTE" ? "#93c5fd" : s.Status == "ANNULE" ? "#ef4444" : s.Status == "REFUSE" ? "#f97316" : "#fcd34d";
                    string name = s.Status ?? "INCONNU";
                    donutData.Add(new { name = name, value = percentage, color = color });
                }
            }
            else
            {
                donutData.Add(new { name = "Aucun", value = 100, color = "#e5e7eb" });
            }

            var totalClients = await context.RendezVous
                .Where(r => r.IdPres == prestataireId)
                .Select(r => r.IdUtili)
                .Distinct()
                .CountAsync();

            var rdvEnAttente = await context.RendezVous
                .Where(r => r.IdPres == prestataireId && r.Statut == "EN_ATTENTE")
                .CountAsync();

            // RDV days this month for calendar highlighting (with status for color coding)
            var rdvDaysThisMonth = await context.RendezVous
                .Where(r => r.IdPres == prestataireId && r.DateDebut >= startOfMonth && r.DateDebut < endOfMonth)
                .GroupBy(r => r.DateDebut.Day)
                .Select(g => new {
                    day = g.Key,
                    // Priority: ACCEPTE > EN_ATTENTE > ANNULE > others
                    statut = g.Any(r => r.Statut == "ACCEPTE") ? "ACCEPTE"
                           : g.Any(r => r.Statut == "EN_ATTENTE") ? "EN_ATTENTE"
                           : g.Any(r => r.Statut == "ANNULE") ? "ANNULE"
                           : g.First().Statut
                })
                .ToListAsync();

            // Top services by booking count
            var topServices = await context.RendezVous
                .Include(r => r.IdSerNavigation)
                .Where(r => r.IdPres == prestataireId)
                .GroupBy(r => new { r.IdSer, r.IdSerNavigation.Nom })
                .Select(g => new {
                    name = g.Key.Nom,
                    count = g.Count(),
                    revenue = g.Sum(r => (decimal?)r.IdSerNavigation.Prix) ?? 0
                })
                .OrderByDescending(g => g.count)
                .Take(5)
                .ToListAsync();

            // Clients this month
            var clientsThisMonth = await context.RendezVous
                .Where(r => r.IdPres == prestataireId && r.DateDebut >= startOfMonth && r.DateDebut < endOfMonth)
                .Select(r => r.IdUtili)
                .Distinct()
                .CountAsync();

            var noteMoyenneRounded = Math.Round(noteMoyenne, 1);
            return Ok(new
            {
                revenus,
                rdvThisMonth,
                noteMoyenne = noteMoyenneRounded,
                totalClients,
                clientsThisMonth,
                rdvEnAttente,
                rdvDaysThisMonth,
                topServices,
                areaData,
                barData,
                donutData
            });
        }
        [HttpGet("{prestataireId}/upcoming")]
        public async Task<IActionResult> GetUpcoming(int prestataireId)
        {
            if (await GetAuthorizedPrestataireId(prestataireId) == null)
                return Forbid();

            var now = DateTime.Now;

            var rdvs = await context.RendezVous
                .Include(r => r.IdUtiliNavigation)
                .Where(r => r.IdPres == prestataireId && r.DateDebut >= now)
                .OrderBy(r => r.DateDebut)
                .Take(3)
                .Select(r => new
                {
                    client = r.IdUtiliNavigation.NomComplet,
                    time = r.DateDebut.ToString("HH:mm"),
                    statut = r.Statut
                })
                .ToListAsync();
            return Ok(rdvs);
        }
        [HttpGet("{prestataireId}/latest")]
        public async Task<IActionResult> GetLatest(int prestataireId)
        {
            if (await GetAuthorizedPrestataireId(prestataireId) == null)
                return Forbid();

            var now = DateTime.Now;

            var latest = await context.RendezVous
                .Include(r => r.IdUtiliNavigation)
                .Where(r => r.IdPres == prestataireId )
                .OrderByDescending(r => r.DateCreation)
                .Select(r => new {
                    client = r.IdUtiliNavigation.NomComplet,
                    time = r.DateDebut.ToString("HH:mm"),
                    statut = r.Statut
                })
                .FirstOrDefaultAsync();

            return Ok(latest);
        }
        [HttpGet("top-prestataires")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTopPrestataires()
        {
            var top = await context.Prestataires
                .Include(p=> p.IdUtiliNavigation)
                .OrderByDescending(p => p.Note)
                .Take(2)
                .Select(p => new
                {
                    nom = p.IdUtiliNavigation.NomComplet,
                    specialite = p.Speciallite,
                    note = p.Note
                }).ToListAsync();
            return Ok(top);
        }

        [HttpGet("{prestataireId}/activity-feed")]
        public async Task<IActionResult> GetActivityFeed(int prestataireId, [FromQuery] int limit = 50)
        {
            if (await GetAuthorizedPrestataireId(prestataireId) == null)
                return Forbid();

            var prestataire = await context.Prestataires
                .Include(p => p.IdUtiliNavigation)
                .FirstOrDefaultAsync(p => p.IdPres == prestataireId);

            if (prestataire == null) return NotFound();

            var providerUserId = prestataire.IdUtili;
            var now = DateTime.Now;
            var activities = new List<object>();

            // 1. New Messages (received by provider)
            var messages = await context.Messages
                .Include(m => m.IdEnvoyeurNavigation)
                .Where(m => m.IdReceveur == providerUserId)
                .OrderByDescending(m => m.EnvoieA)
                .Take(20)
                .Select(m => new
                {
                    id = m.IdMessage,
                    type = "message",
                    title = "Nouveau message",
                    message = $"De {m.IdEnvoyeurNavigation.NomComplet}: {m.Contenu.Substring(0, Math.Min(100, m.Contenu.Length))}{(m.Contenu.Length > 100 ? "..." : "")}",
                    isRead = m.Lu,
                    createdAt = m.EnvoieA,
                    senderId = m.IdEnvoyeur,
                    senderName = m.IdEnvoyeurNavigation.NomComplet,
                    senderAvatar = m.IdEnvoyeurNavigation.Avatar
                })
                .ToListAsync();
            activities.AddRange(messages);

            // 2. New Reviews (Avis)
            var avis = await context.Avis
                .Include(a => a.Utilisateur)
                .Include(a => a.RendezVous)
                .ThenInclude(r => r.IdSerNavigation)
                .Where(a => a.IdPrestataire == prestataireId)
                .OrderByDescending(a => a.DateCreation)
                .Take(20)
                .Select(a => new
                {
                    id = a.IdAvis,
                    type = "avis",
                    title = "Nouvel avis reçu",
                    message = $"{a.Utilisateur.NomComplet} a laissé une note de {a.Note}/5: \"{a.Commentaire.Substring(0, Math.Min(100, a.Commentaire.Length))}{(a.Commentaire.Length > 100 ? "..." : "")}\"",
                    isRead = false,
                    createdAt = a.DateCreation,
                    rating = a.Note,
                    clientName = a.Utilisateur.NomComplet,
                    clientAvatar = a.Utilisateur.Avatar,
                    serviceName = a.RendezVous != null && a.RendezVous.IdSerNavigation != null ? a.RendezVous.IdSerNavigation.Nom : "Service"
                })
                .ToListAsync();
            activities.AddRange(avis);

            // 3. New Appointments (RendezVous) - all statuses
            var rdvs = await context.RendezVous
                .Include(r => r.IdUtiliNavigation)
                .Include(r => r.IdSerNavigation)
                .Where(r => r.IdPres == prestataireId)
                .OrderByDescending(r => r.DateCreation)
                .Take(20)
                .Select(r => new
                {
                    id = r.IdRendezVous,
                    type = "rendezvous",
                    title = GetRdvTitle(r.Statut),
                    message = $"{r.IdUtiliNavigation.NomComplet} a demandé un rendez-vous pour {r.IdSerNavigation.Nom} le {r.DateDebut:dd/MM/yyyy} à {r.DateDebut:HH:mm}",
                    isRead = false,
                    createdAt = r.DateCreation,
                    statut = r.Statut,
                    clientName = r.IdUtiliNavigation.NomComplet,
                    clientAvatar = r.IdUtiliNavigation.Avatar,
                    serviceName = r.IdSerNavigation.Nom,
                    servicePrice = r.IdSerNavigation.Prix,
                    dateDebut = r.DateDebut,
                    dateFin = r.DateFin,
                    lieu = r.Lieu
                })
                .ToListAsync();
            activities.AddRange(rdvs);

            // 4. Existing Notifications
            var notifications = await context.Notifications
                .Where(n => n.UtilisateurId == providerUserId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(20)
                .Select(n => new
                {
                    id = n.Id,
                    type = "notification",
                    title = n.Title,
                    message = n.Message,
                    isRead = n.IsRead,
                    createdAt = n.CreatedAt,
                    rendezVousId = n.RendezVousId
                })
                .ToListAsync();
            activities.AddRange(notifications);

            // Sort all activities by date descending and take limit
            var sortedActivities = activities
                .OrderByDescending(a => ((dynamic)a).createdAt)
                .Take(limit)
                .ToList();

            return Ok(sortedActivities);
        }

        private string GetRdvTitle(string? statut)
        {
            return statut switch
            {
                "EN_ATTENTE" => "Nouvelle demande de rendez-vous",
                "ACCEPTE" => "Rendez-vous confirmé",
                "REFUSE" => "Rendez-vous refusé",
                "ANNULE" => "Rendez-vous annulé",
                "TERMINE" => "Rendez-vous terminé",
                _ => "Nouveau rendez-vous"
            };
        }
    }
}
