using Bookify_API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bookify_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PrestatairesController : Controller
    {
        private readonly BookifyDbContext context;

        public PrestatairesController(BookifyDbContext context)
        {
            this.context = context;
        }
        [HttpGet("profile/{id}")]
        public async Task<IActionResult> getPrestataireProfile(int id)
        {
            var prestataire = await context.Prestataires
                .Include(p => p.IdUtiliNavigation)
                .Include(p => p.Services)
                .Where(p => p.IdPres == id)
                .Select(p => new
                {
                    id = p.IdPres,
                    nom = p.IdUtiliNavigation.NomComplet,
                    specialite = p.Speciallite,
                    bio = p.Bio,
                    note = p.Note,
                    services = p.Services.Select(s => new
                    {
                        id = s.IdService,
                        name = s.Nom,
                        prix = s.Prix,
                        duree = s.Duration
                    })
                }).FirstOrDefaultAsync();
            if (prestataire == null) return NotFound();
            return Ok(prestataire);
        }
        [HttpGet("random")]
        public async Task<IActionResult> GetRandomPrestataire()
        {
            var prestataire = await context.Prestataires
                .Include(p => p.IdUtiliNavigation)
                .OrderBy(r => Guid.NewGuid())
                .Take(3)
                .Select(p => new
                {
                    id = p.IdPres,
                    nom = p.IdUtiliNavigation.NomComplet,
                    location = p.IdUtiliNavigation.Adresse,
                    specialite = p.Speciallite,
                    rating = p.Note,
                    description = p.Bio,
                    avatar = p.IdUtiliNavigation.Avatar,
                }).ToListAsync();
            return Ok(prestataire);
        }
    }
}
