using Bookify_API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bookify_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FavorisController : BaseController
    {
        private readonly BookifyDbContext context;

        public FavorisController(BookifyDbContext context)
        {
            this.context = context;
        }

        [HttpPost("toggle/{idPres}")]
        public async Task<IActionResult> ToggleFavori(int idPres)
        {
            try {
                var userId = GetUserId();
                var favori = await context.Favoris
                    .FirstOrDefaultAsync(f => f.IdUtilisateur == userId && f.IdPrestataire == idPres);

                if (favori == null)
                {
                    favori = new Favori
                    {
                        IdUtilisateur = userId,
                        IdPrestataire = idPres,
                        DateAjout = DateTime.Now
                    };
                    context.Favoris.Add(favori);
                    await context.SaveChangesAsync();
                    return Ok(new { isFavorited = true });
                }
                else
                {
                    context.Favoris.Remove(favori);
                    await context.SaveChangesAsync();
                    return Ok(new { isFavorited = false });
                }
            } catch (UnauthorizedAccessException) {
                return Unauthorized();
            }
        }

        [HttpGet("check/{idPres}")]
        public async Task<IActionResult> CheckFavori(int idPres)
        {
            try {
                var userId = GetUserId();
                var isFavorited = await context.Favoris
                    .AnyAsync(f => f.IdUtilisateur == userId && f.IdPrestataire == idPres);
                return Ok(new { isFavorited });
            } catch (UnauthorizedAccessException) {
                return Unauthorized();
            }
        }

        [HttpGet("my-favorites")]
        public async Task<IActionResult> GetMyFavorites()
        {
            try {
                var userId = GetUserId();

            var today = DateTime.Now.DayOfWeek switch
            {
                DayOfWeek.Monday => "Lun",
                DayOfWeek.Tuesday => "Mar",
                DayOfWeek.Wednesday => "Mer",
                DayOfWeek.Thursday => "Jeu",
                DayOfWeek.Friday => "Ven",
                DayOfWeek.Saturday => "Sam",
                DayOfWeek.Sunday => "Dim",
                _ => "Lun"
            };

            var favorites = await context.Favoris
                .Include(f => f.Prestataire)
                    .ThenInclude(p => p.IdUtiliNavigation)
                .Where(f => f.IdUtilisateur == userId)
                .OrderByDescending(f => f.DateAjout)
                .Select(f => new
                {
                    id = f.Prestataire.IdPres,
                    nom = f.Prestataire.IdUtiliNavigation.NomComplet,
                    location = f.Prestataire.IdUtiliNavigation.Adresse,
                    specialite = f.Prestataire.Speciallite,
                    categorie = f.Prestataire.Categorie,
                    rating = f.Prestataire.Note,
                    description = f.Prestataire.Bio,
                    avatar = f.Prestataire.IdUtiliNavigation.Avatar,
                    availableToday = context.Disponibilites.Any(d => d.IdPres == f.Prestataire.IdPres && d.JourSemaine == today && d.Disponible)
                })
                .ToListAsync();

            return Ok(favorites);
            } catch (UnauthorizedAccessException) {
                return Unauthorized();
            }
        }
    }
}
