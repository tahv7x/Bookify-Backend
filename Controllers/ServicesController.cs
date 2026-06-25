using Bookify_API.DTOs;
using Bookify_API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Bookify_API.Services;

namespace Bookify_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServicesController : BaseController
    {
        private readonly BookifyDbContext context;
        private readonly CloudinaryService cloudinaryService;

        public ServicesController(BookifyDbContext context, CloudinaryService cloudinaryService)
        {
            this.context = context;
            this.cloudinaryService = cloudinaryService;
        }

        [HttpGet("explore")]
        public async Task<IActionResult> Explore([FromQuery] string? q = null, [FromQuery] int? categoryId = null, [FromQuery] string? category = null, [FromQuery] string? city = null, [FromQuery] int? minRating = null)
        {
            var query = context.Services
                .Include(s => s.IdPresNavigation)
                .ThenInclude(p => p.IdUtiliNavigation)
                .Include(s => s.IdPresNavigation)
                .ThenInclude(p => p.IdCategorieNavigation)
                .AsQueryable();

            if (!string.IsNullOrEmpty(q))
            {
                var lowerQ = q.ToLower();
                query = query.Where(s => 
                    (s.Nom != null && s.Nom.ToLower().Contains(lowerQ)) || 
                    (s.IdPresNavigation.IdUtiliNavigation.NomComplet != null && s.IdPresNavigation.IdUtiliNavigation.NomComplet.ToLower().Contains(lowerQ))
                );
            }

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(s => s.IdPresNavigation.IdCategorie == categoryId.Value);
            }
            else if (!string.IsNullOrEmpty(category) && category != "Tous" && category != "Toutes")
            {
                var lowerCategory = category.ToLower();
                query = query.Where(s => s.IdPresNavigation.IdCategorieNavigation != null && s.IdPresNavigation.IdCategorieNavigation.Nom.ToLower() == lowerCategory);
            }


            if (!string.IsNullOrEmpty(city) && city != "Toutes")
            {
                query = query.Where(s => s.IdPresNavigation.IdUtiliNavigation.Adresse != null && s.IdPresNavigation.IdUtiliNavigation.Adresse.Contains(city));
            }

            if (minRating.HasValue && minRating.Value > 0)
            {
                query = query.Where(s => s.IdPresNavigation.Note >= minRating.Value);
            }

            var services = await query.Select(s => new
            {
                idService = s.IdService,
                nom = s.Nom,
                prix = s.Prix,
                duree = s.Duree,
                uniteDuree = s.UniteDuree,
                imageUrls = s.ImageUrls,
                prestataire = new
                {
                    id = s.IdPres,
                    nom = s.IdPresNavigation.IdUtiliNavigation.NomComplet,
                    avatar = s.IdPresNavigation.IdUtiliNavigation.Avatar,
                    note = s.IdPresNavigation.Note,
                    adresse = s.IdPresNavigation.IdUtiliNavigation.Adresse,
                    enLocal = s.IdPresNavigation.EnLocal,
                    aDomicile = s.IdPresNavigation.ADomicile,
                    latitude = s.IdPresNavigation.Latitude,
                    longitude = s.IdPresNavigation.Longitude,
                    categorie = s.IdPresNavigation.IdCategorieNavigation != null ? s.IdPresNavigation.IdCategorieNavigation.Nom : null,
                    idCategorie = s.IdPresNavigation.IdCategorie
                }
            }).ToListAsync();

            return Ok(services);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetServiceById(int id)
        {
            var s = await context.Services
                .Include(s => s.IdPresNavigation)
                .ThenInclude(p => p.IdUtiliNavigation)
                .Include(s => s.IdPresNavigation)
                .ThenInclude(p => p.IdCategorieNavigation)
                .FirstOrDefaultAsync(s => s.IdService == id);

            if (s == null) return NotFound(new { message = "Service introuvable" });

            return Ok(new
            {
                idService = s.IdService,
                nom = s.Nom,
                description = s.Description,
                prix = s.Prix,
                duree = s.Duree,
                uniteDuree = s.UniteDuree,
                imageUrls = s.ImageUrls,
                prestataire = new
                {
                    id = s.IdPres,
                    idUtilisateur = s.IdPresNavigation.IdUtili,
                    nom = s.IdPresNavigation.IdUtiliNavigation.NomComplet,
                    avatar = s.IdPresNavigation.IdUtiliNavigation.Avatar,
                    note = s.IdPresNavigation.Note,
                    adresse = s.IdPresNavigation.IdUtiliNavigation.Adresse,
                    enLocal = s.IdPresNavigation.EnLocal,
                    aDomicile = s.IdPresNavigation.ADomicile,
                    categorie = s.IdPresNavigation.IdCategorieNavigation != null ? s.IdPresNavigation.IdCategorieNavigation.Nom : null,
                    idCategorie = s.IdPresNavigation.IdCategorie
                }
            });
        }

        [HttpGet("mine")]
        [Authorize(Roles = "PRESTATAIRE")]
        public async Task<IActionResult> GetMyServices()
        {
            var myUserId = GetUserId();
            var myPrestataire = await context.Prestataires.FirstOrDefaultAsync(p => p.IdUtili == myUserId);

            if (myPrestataire == null) return Unauthorized(new { message = "Profil prestataire introuvable." });

            var services = await context.Services
                .Where(s => s.IdPres == myPrestataire.IdPres)
                .Select(s => new ServiceDto
                {
                    IdService = s.IdService,
                    Nom = s.Nom ?? string.Empty,
                    Description = s.Description ?? string.Empty,
                    Prix = s.Prix ?? 0,
                    Duree = s.Duree,
                    UniteDuree = s.UniteDuree,
                    Images = string.IsNullOrEmpty(s.ImageUrls) ? new List<string>() : s.ImageUrls.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                })
                .ToListAsync();

            return Ok(services);
        }

        [HttpPost]
        [Authorize(Roles = "PRESTATAIRE")]
        public async Task<IActionResult> AddService([FromForm] ServiceUploadDto dto)
        {
            var myUserId = GetUserId();
            var myPrestataire = await context.Prestataires.FirstOrDefaultAsync(p => p.IdUtili == myUserId);

            if (myPrestataire == null) return Unauthorized(new { message = "Profil prestataire introuvable." });

            var imageUrls = new List<string>();
            if (dto.ImagesFiles != null && dto.ImagesFiles.Count > 0)
            {
                foreach (var file in dto.ImagesFiles.Take(4))
                {
                    var url = await cloudinaryService.UploadImageAsync(file);
                    if (!string.IsNullOrEmpty(url))
                    {
                        imageUrls.Add(url);
                    }
                }
            }

            var service = new Service
            {
                IdPres = myPrestataire.IdPres,
                Nom = dto.Nom,
                Description = dto.Description,
                Prix = dto.Prix,
                Duree = dto.Duree,
                UniteDuree = dto.UniteDuree,
                IsFullDay = dto.IsFullDay,
                ImageUrls = imageUrls.Count > 0 ? string.Join(",", imageUrls) : null
            };

            context.Services.Add(service);
            return await SaveAsyncChanges(context, () => new { message = "Service ajouté avec succès.", serviceId = service.IdService });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "PRESTATAIRE")]
        public async Task<IActionResult> UpdateService(int id, [FromForm] ServiceUploadDto dto)
        {
            var myUserId = GetUserId();
            var myPrestataire = await context.Prestataires.FirstOrDefaultAsync(p => p.IdUtili == myUserId);

            if (myPrestataire == null) return Unauthorized(new { message = "Profil prestataire introuvable." });

            var service = await context.Services.FindAsync(id);
            if (service == null) return NotFound(new { message = "Service introuvable." });

            if (service.IdPres != myPrestataire.IdPres) return Forbid();

            var imageUrls = new List<string>();
            int newFileIndex = 0;
            var fileArray = dto.ImagesFiles?.ToArray();

            if (dto.ExistingImages != null)
            {
                foreach (var img in dto.ExistingImages.Take(4))
                {
                    if (img == "__NEW__" && fileArray != null && newFileIndex < fileArray.Length)
                    {
                        var url = await cloudinaryService.UploadImageAsync(fileArray[newFileIndex]);
                        if (!string.IsNullOrEmpty(url)) imageUrls.Add(url);
                        newFileIndex++;
                    }
                    else if (img != "__NEW__")
                    {
                        imageUrls.Add(img);
                    }
                }
            }

            // Append any remaining new files just in case
            if (fileArray != null)
            {
                while (newFileIndex < fileArray.Length && imageUrls.Count < 4)
                {
                    var url = await cloudinaryService.UploadImageAsync(fileArray[newFileIndex]);
                    if (!string.IsNullOrEmpty(url)) imageUrls.Add(url);
                    newFileIndex++;
                }
            }

            service.Nom = dto.Nom;
            service.Description = dto.Description;
            service.Prix = dto.Prix;
            service.Duree = dto.Duree;
            service.UniteDuree = dto.UniteDuree;

            service.ImageUrls = imageUrls.Count > 0 ? string.Join(",", imageUrls) : null;

            return await SaveAsyncChanges(context, new { message = "Service modifié avec succès." });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "PRESTATAIRE")]
        public async Task<IActionResult> DeleteService(int id)
        {
            var myUserId = GetUserId();
            var myPrestataire = await context.Prestataires.FirstOrDefaultAsync(p => p.IdUtili == myUserId);

            if (myPrestataire == null) return Unauthorized(new { message = "Profil prestataire introuvable." });

            var service = await context.Services.FindAsync(id);
            if (service == null) return NotFound(new { message = "Service introuvable." });

            if (service.IdPres != myPrestataire.IdPres) return Forbid();

            context.Services.Remove(service);
            return await SaveAsyncChanges(context, new { message = "Service supprimé avec succès." });
        }
    }
}
