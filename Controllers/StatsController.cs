using Bookify_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bookify_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatsController : BaseController
    {
        private readonly BookifyDbContext context;
        public StatsController(BookifyDbContext context)
        {
            this.context = context;
        }
        [HttpGet("{prestataireId}")]
        public async Task<IActionResult> GetStats(int prestataireId)
        {
            var now = DateTime.Now;

            // Basic stats
            var startOfMonth = new DateTime(now.Year, now.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1);

            var revenus = await context.RendezVous
                .Include(r => r.IdSerNavigation)
                .Where(r => r.IdPres == prestataireId &&
                    r.DateCreation.HasValue &&
                    r.DateCreation.Value >= startOfMonth &&
                    r.DateCreation.Value < endOfMonth &&
                    r.Statut == "TERMINE"
                    )
                .SumAsync(r => (decimal?)r.IdSerNavigation.Prix) ?? 0;

            var today = DateTime.Today;
            var rdvToday = await context.RendezVous
                .Where(r =>
                    r.IdPres == prestataireId &&
                    r.DateDebut.Date == today
                ).CountAsync();

            // AreaData (Last 6 months revenues/appointments)
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

            // BarData (Appointments per day of current week)
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

            // DonutData (Appointments by status)
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
                    string color = s.Status == "TERMINE" ? "#1e3a8a" : s.Status == "CONFIRME" ? "#93c5fd" : s.Status == "ANNULE" ? "#ef4444" : "#fcd34d";
                    string name = s.Status ?? "INCONNU";
                    donutData.Add(new { name = name, value = percentage, color = color });
                }
            }
            else
            {
                donutData.Add(new { name = "Aucun", value = 100, color = "#e5e7eb" });
            }

            return Ok(new
            {
                revenus,
                rdvToday,
                areaData,
                barData,
                donutData
            });
        }
        [HttpGet("{prestataireId}/upcoming")]
        public async Task<IActionResult> GetUpcoming(int prestataireId)
        {
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
    }
}
