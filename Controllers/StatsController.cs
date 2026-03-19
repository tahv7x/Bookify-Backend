using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bookify_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatsController : ControllerBase
    {
        private readonly AppDbContext context;
        public StatsController(AppDbContext context)
        {
            this.context = context;
        }
        [HttpGet("{prestataireId}")]
        public async Task<IActionResult> GetStats(int prestataireId)
        {
            var now = DateTime.Now;

            var start = new DateTime(now.Year, now.Month, 1);
            var end = start.AddMonths(1);

            var revenus = await context.RendezVous
                .Include(r => r.IdSerNavigation)
                .Where(r => r.IdPres == prestataireId &&
                    r.DateCreation.HasValue &&
                    r.DateCreation.Value >= start &&
                    r.DateCreation.Value < end &&
                    r.Statut == "TERMINE"
                    )
                .SumAsync(r => (decimal?)r.IdSerNavigation.Prix) ?? 0;

            var today = DateTime.Today;
            var rdvToday = await context.RendezVous
                .Where(r =>
                    r.IdPres == prestataireId &&
                    r.DateDebut.Date == today
                ).CountAsync();
            return Ok(new
            {
                revenus,
                rdvToday
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
                .Take(5)
                .Select(r => new
                {
                    client = r.IdUtiliNavigation.NomComplet,
                    time = r.DateDebut.ToString("HH:mm"),
                    statut = r.Statut
                })
                .ToListAsync();
            return Ok(rdvs);
        }
    }
}
