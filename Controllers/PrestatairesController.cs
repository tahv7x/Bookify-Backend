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
        public async Task<IActionResult> GetAll()
        {
            var prestataires = await context.Prestataires
                .Include(p => p.IdUtiliNavigation)
                .Include(p => p.Prestatairephotos)
                .Select(p => new
                {
                    id = p.IdPres,
                    nom = p.IdUtiliNavigation.NomComplet,
                    location = p.IdUtiliNavigation.Adresse,
                    specialite = p.Speciallite,
                    rating = p.Note,
                    description = p.Bio,
                    avatar = p.IdUtiliNavigation.Avatar,
                    categorie = p.Categorie,
                    photos = p.Prestatairephotos.Select(ph => ph.Url).ToList()
                }).ToListAsync();
            return Ok(prestataires);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var prestataire = await context.Prestataires
                .Include(p => p.IdUtiliNavigation)
                .Include(p => p.Services)
                .Include(p => p.Prestatairephotos)
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
                    categorie = p.Categorie,
                    bio = p.Bio,
                    note = p.Note,
                    photos = p.Prestatairephotos.Select(ph => ph.Url).ToList(),
                    services = p.Services.Select(s => new
                    {
                        id = s.IdService,
                        name = s.Nom,
                        prix = s.Prix,
                        duree = s.Duree,
                        uniteDuree = s.UniteDuree
                    }).ToList()
                }).FirstOrDefaultAsync();
            if (prestataire == null) return NotFound();
            return Ok(prestataire);
        }

        [HttpGet("profile/{id}")]
        public async Task<IActionResult> GetProfile(int id)
        {
            return await Get(id);
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
                utilisateur.NomComplet = dto.NomComplet;
                utilisateur.Telephone = dto.Telephone;
                utilisateur.Adresse = dto.Adresse;
            }

            myPrestataire.Speciallite = dto.Specialite;
            myPrestataire.Bio = dto.Bio;
            myPrestataire.Categorie = dto.Categorie;

            return await SaveAsyncChanges(context, new { message = "Profil mis à jour", user = new {
                id = utilisateur?.IdUtilisateur,
                nom = utilisateur?.NomComplet,
                email = utilisateur?.Email,
                role = utilisateur?.Role,
                telephone = utilisateur?.Telephone,
                avatar = utilisateur?.Avatar
            }});
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
                    heureDebut = d.HeureDebut.ToString(@"hh\:mm"),
                    heureFin = d.HeureFin.ToString(@"hh\:mm"),
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
                if (TimeSpan.TryParse(dto.HeureDebut, out var start) && TimeSpan.TryParse(dto.HeureFin, out var end))
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

    public class ProviderUpdateDto
    {
        public string NomComplet { get; set; }
        public string Telephone { get; set; }
        public string Adresse { get; set; }
        public string Specialite { get; set; }
        public string Bio { get; set; }
        public string Categorie { get; set; }
    }

    public class DispoDto
    {
        public string JourSemaine { get; set; }
        public string HeureDebut { get; set; }
        public string HeureFin { get; set; }
        public bool Disponible { get; set; }
    }
}
