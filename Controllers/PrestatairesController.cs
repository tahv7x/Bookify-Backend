using Bookify_API.DTOs;
using Bookify_API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bookify_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PrestatairesController : BaseController
    {
        private readonly BookifyDbContext context;

        public PrestatairesController(BookifyDbContext context)
        {
            this.context = context;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAll([FromQuery] string? q = null, [FromQuery] int? categoryId = null, [FromQuery] string? category = null, [FromQuery] string? city = null, [FromQuery] int? minRating = null)
        {
            var query = context.Prestataires
                .Include(p => p.IdUtiliNavigation)
                .Include(p => p.IdCategorieNavigation)
                .AsQueryable();

            if (!string.IsNullOrEmpty(q))
            {
                var lowerQ = q.ToLower();
                query = query.Where(p => 
                    (p.IdUtiliNavigation.NomComplet != null && p.IdUtiliNavigation.NomComplet.ToLower().Contains(lowerQ)) || 
                    (p.Speciallite != null && p.Speciallite.ToLower().Contains(lowerQ)) || 
                    p.Services.Any(s => s.Nom != null && s.Nom.ToLower().Contains(lowerQ)));
            }

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(p => p.IdCategorie == categoryId.Value);
            }
            else if (!string.IsNullOrEmpty(category) && category != "Tous" && category != "Toutes")
            {
                var lowerCategory = category.ToLower();
                query = query.Where(p => p.IdCategorieNavigation != null && p.IdCategorieNavigation.Nom.ToLower() == lowerCategory);
            }

            if (!string.IsNullOrEmpty(city) && city != "Toutes")
            {
                query = query.Where(p => p.IdUtiliNavigation.Adresse != null && p.IdUtiliNavigation.Adresse.Contains(city));
            }

            if (minRating.HasValue && minRating.Value > 0)
            {
                query = query.Where(p => p.Note >= minRating.Value);
            }

            var prestataires = await query
                .Select(p => new
                {
                    id = p.IdPres,
                    nom = p.IdUtiliNavigation.NomComplet,
                    location = p.IdUtiliNavigation.Adresse,
                    specialite = p.Speciallite,
                    rating = context.Avis.Where(a => a.IdPrestataire == p.IdPres).Any()
                        ? Math.Round(context.Avis.Where(a => a.IdPrestataire == p.IdPres).Average(a => (double)a.Note), 1)
                        : (double?)p.Note ?? 0,
                    description = p.Bio,
                    avatar = p.IdUtiliNavigation.Avatar,
                    categorie = p.IdCategorieNavigation != null ? p.IdCategorieNavigation.Nom : null,
                    idCategorie = p.IdCategorie,
                    latitude = p.Latitude,
                    longitude = p.Longitude,
                    enLocal = p.EnLocal,
                    aDomicile = p.ADomicile
                }).ToListAsync();
            return Ok(prestataires);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var prestataire = await context.Prestataires
                .Include(p => p.IdUtiliNavigation)
                .Include(p => p.Services)
                .Include(p => p.IdCategorieNavigation)
                .Where(p => p.IdPres == id)
                .Select(p => new
                {
                    id = p.IdPres,
                    idUtilisateur = p.IdUtili,
                    nom = p.IdUtiliNavigation.NomComplet,
                    email = p.IdUtiliNavigation.Email,
                    telephone = p.IdUtiliNavigation.Telephone,
                    adresse = p.IdUtiliNavigation.Adresse,
                    avatar = p.IdUtiliNavigation.Avatar,
                    specialite = p.Speciallite,
                    categorie = p.IdCategorieNavigation != null ? p.IdCategorieNavigation.Nom : null,
                    idCategorie = p.IdCategorie,
                    bio = p.Bio,
                    note = p.Note,
                    latitude = p.Latitude,
                    longitude = p.Longitude,
                    enLocal = p.EnLocal,
                    aDomicile = p.ADomicile,
                    services = p.Services.Select(s => new
                    {
                        id = s.IdService,
                        name = s.Nom,
                        prix = s.Prix,
                        duree = s.Duree,
                        uniteDuree = s.UniteDuree,
                        imageUrls = s.ImageUrls
                    }).ToList()
                }).FirstOrDefaultAsync();
            if (prestataire == null) return NotFound();
            return Ok(prestataire);
        }

        [HttpGet("mine")]
        [Authorize(Roles = "PRESTATAIRE")]
        public async Task<IActionResult> GetMyProviderProfile()
        {
            var myUserId = GetUserId();
            var myPrestataire = await context.Prestataires.FirstOrDefaultAsync(p => p.IdUtili == myUserId);
            if (myPrestataire == null) return Unauthorized(new { message = "Profil prestataire introuvable." });
            return await Get(myPrestataire.IdPres);
        }

        [HttpPut("mine")]
        [Authorize(Roles = "PRESTATAIRE")]
        public async Task<IActionResult> UpdateMyProviderProfile([FromBody] ProviderUpdateDto dto)
        {
            var myUserId = GetUserId();
            var myPrestataire = await context.Prestataires.FirstOrDefaultAsync(p => p.IdUtili == myUserId);
            if (myPrestataire == null) return Unauthorized(new { message = "Profil prestataire introuvable." });

            var utilisateur = await context.Utilisateurs.FindAsync(myUserId);
            if (utilisateur != null)
            {
                if (!string.IsNullOrEmpty(dto.Telephone) && 
                    dto.Telephone != utilisateur.Telephone && 
                    await context.Utilisateurs.AnyAsync(u => u.Telephone == dto.Telephone && u.IdUtilisateur != myUserId))
                {
                    return BadRequest("Numéro de téléphone déjà utilisé par un autre utilisateur.");
                }
                utilisateur.NomComplet = dto.NomComplet;
                utilisateur.Telephone = dto.Telephone;
                utilisateur.Adresse = dto.Adresse;
            }

            myPrestataire.Speciallite = dto.Specialite;
            myPrestataire.Bio = dto.Bio;
            
            if (dto.IdCategorie.HasValue && dto.IdCategorie.Value > 0)
            {
                myPrestataire.IdCategorie = dto.IdCategorie.Value;
            }
            else if (!string.IsNullOrEmpty(dto.Categorie))
            {
                var search = dto.Categorie.Trim().ToLower();
                var allCats = await context.Categories.ToListAsync();
                var cat = allCats.FirstOrDefault(c => c.Nom.Trim().ToLower() == search);

                if (cat != null)
                {
                    myPrestataire.IdCategorie = cat.IdCategorie;
                }
                else
                {
                    myPrestataire.IdCategorie = null;
                }
            }
            else
            {
                myPrestataire.IdCategorie = null;
            }

            if (dto.Latitude.HasValue) myPrestataire.Latitude = dto.Latitude.Value;
            if (dto.Longitude.HasValue) myPrestataire.Longitude = dto.Longitude.Value;
            
            myPrestataire.EnLocal = dto.EnLocal;
            myPrestataire.ADomicile = dto.ADomicile;

            return await SaveAsyncChanges(context, new { message = "Profil mis à jour", user = new {
                nom = utilisateur?.NomComplet,
                email = utilisateur?.Email,
                avatar = utilisateur?.Avatar,
                specialite = myPrestataire.Speciallite,
                bio = myPrestataire.Bio,
                enLocal = myPrestataire.EnLocal,
                aDomicile = myPrestataire.ADomicile
            } });
        }

        [HttpGet("mine/clients")]
        [Authorize(Roles = "PRESTATAIRE")]
        public async Task<IActionResult> GetMyClients()
        {
            var myUserId = GetUserId();
            var myPrestataire = await context.Prestataires.FirstOrDefaultAsync(p => p.IdUtili == myUserId);
            if (myPrestataire == null) return Unauthorized(new { message = "Profil prestataire introuvable." });

            var clients = await context.RendezVous
                .Where(r => r.IdPres == myPrestataire.IdPres && r.IdUtiliNavigation != null)
                .GroupBy(r => r.IdUtili)
                .Select(g => new
                {
                    id = g.Key,
                    name = g.First().IdUtiliNavigation.NomComplet,
                    email = g.First().IdUtiliNavigation.Email,
                    phone = g.First().IdUtiliNavigation.Telephone,
                    city = g.First().IdUtiliNavigation.Adresse,
                    avatar = g.First().IdUtiliNavigation.Avatar,
                    initials = !string.IsNullOrEmpty(g.First().IdUtiliNavigation.NomComplet) ? g.First().IdUtiliNavigation.NomComplet.Substring(0, Math.Min(2, g.First().IdUtiliNavigation.NomComplet.Length)) : "CL",
                    rdvCount = g.Count(),
                    lastRdv = g.Max(r => r.DateDebut),
                    rating = 0.0
                })
                .ToListAsync();

            return Ok(clients);
        }

        [HttpGet("mine/disponibilites")]
        [Authorize(Roles = "PRESTATAIRE")]
        public async Task<IActionResult> GetMyDisponibilites()
        {
            var myUserId = GetUserId();
            var myPrestataire = await context.Prestataires.FirstOrDefaultAsync(p => p.IdUtili == myUserId);
            if (myPrestataire == null) return Unauthorized();

            var dispos = await context.Disponibilites
                .Where(d => d.IdPres == myPrestataire.IdPres)
                .Select(d => new
                {
                    jourSemaine = d.JourSemaine,
                    heureDebut = d.HeureDebut.HasValue ? d.HeureDebut.Value.ToString(@"hh\:mm") : null,
                    heureFin = d.HeureFin.HasValue ? d.HeureFin.Value.ToString(@"hh\:mm") : null,
                    disponible = d.Disponible
                })
                .ToListAsync();

            return Ok(dispos);
        }

        [HttpPut("mine/disponibilites")]
        [Authorize(Roles = "PRESTATAIRE")]
        public async Task<IActionResult> UpdateMyDisponibilites([FromBody] List<DispoDto> dtos)
        {
            var myUserId = GetUserId();
            var myPrestataire = await context.Prestataires.FirstOrDefaultAsync(p => p.IdUtili == myUserId);
            if (myPrestataire == null) return Unauthorized();

            var existing = await context.Disponibilites.Where(d => d.IdPres == myPrestataire.IdPres).ToListAsync();
            context.Disponibilites.RemoveRange(existing);

            foreach (var dto in dtos)
            {
                if (string.IsNullOrEmpty(dto.HeureDebut) || string.IsNullOrEmpty(dto.HeureFin))
                {
                    context.Disponibilites.Add(new Disponibilite
                    {
                        IdPres = myPrestataire.IdPres,
                        JourSemaine = dto.JourSemaine,
                        HeureDebut = null,
                        HeureFin = null,
                        Disponible = dto.Disponible
                    });
                }
                else if (TimeSpan.TryParse(dto.HeureDebut, out var start) && TimeSpan.TryParse(dto.HeureFin, out var end))
                {
                    context.Disponibilites.Add(new Disponibilite
                    {
                        IdPres = myPrestataire.IdPres,
                        JourSemaine = dto.JourSemaine,
                        HeureDebut = start,
                        HeureFin = end,
                        Disponible = dto.Disponible
                    });
                }
            }
            return await SaveAsyncChanges(context, new { message = "Disponibilités mises à jour" });
        }

        [HttpGet("random")]
        public async Task<IActionResult> GetRandomPrestataire()
        {
            var prestataire = await context.Prestataires
                .Include(p => p.IdUtiliNavigation)
                .Include(p => p.IdCategorieNavigation)
                .OrderBy(r => Guid.NewGuid())
                .Take(3)
                .Select(p => new
                {
                    id = p.IdPres,
                    nom = p.IdUtiliNavigation.NomComplet,
                    location = p.IdUtiliNavigation.Adresse,
                    specialite = p.Speciallite,
                    rating = context.Avis.Where(a => a.IdPrestataire == p.IdPres).Any()
                        ? Math.Round(context.Avis.Where(a => a.IdPrestataire == p.IdPres).Average(a => (double)a.Note), 1)
                        : (double?)p.Note ?? 0,
                    description = p.Bio,
                    avatar = p.IdUtiliNavigation.Avatar,
                    categorie = p.IdCategorieNavigation != null ? p.IdCategorieNavigation.Nom : null,
                    idCategorie = p.IdCategorie,
                    latitude = p.Latitude,
                    longitude = p.Longitude,
                    enLocal = p.EnLocal,
                    aDomicile = p.ADomicile,
                }).ToListAsync();
            return Ok(prestataire);
        }
    }
}
