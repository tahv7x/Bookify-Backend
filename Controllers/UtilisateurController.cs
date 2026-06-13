using Bookify_API.DTOs;
using Bookify_API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Bookify_API.Services;
namespace Bookify_API.Controllers
{
    [Route("api/[controller]")]
    public class UtilisateurController : BaseController
    {
        private readonly BookifyDbContext context;
        private readonly CloudinaryService cloudinaryService;
        public UtilisateurController(BookifyDbContext context, CloudinaryService cloudinaryService)
        {
            this.context = context;
            this.cloudinaryService = cloudinaryService;
        }

        [HttpGet]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> GetALL()
        {
            var users = await context.Utilisateurs
                .Select(u => new UserResponseDto
                {
                    IdUtilisateur = u.IdUtilisateur,
                    NomComplet = u.NomComplet,
                    Email = u.Email,
                    Telephone = u.Telephone,
                    Adresse = u.Adresse,
                    Avatar = u.Avatar,
                    Role = u.Role,
                    CreerA = u.CreerA,
                    IsBlocked = u.IsBlocked
                })
                .ToListAsync();
            return Ok(users);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await context.Utilisateurs
                .Where(u => u.IdUtilisateur == id)
                .Select(u => new UserResponseDto
                {
                    IdUtilisateur = u.IdUtilisateur,
                    NomComplet = u.NomComplet,
                    Email = u.Email,
                    Telephone = u.Telephone,
                    Adresse = u.Adresse,
                    Avatar = u.Avatar,
                    Role = u.Role,
                    CreerA = u.CreerA,
                    IsBlocked = u.IsBlocked
                })
                .FirstOrDefaultAsync();

            if (user == null) return NotFound();
            return Ok(user);
        }


        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, UpdateUtilisateurDto miseUtilisateur)
        {
            var userIdFromToken = User.FindFirst("id")?.Value;
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            if (userIdFromToken == null)
            {
                return Unauthorized();
            }

            if (userRole != "ADMIN" && userIdFromToken != id.ToString())
            {
                return Forbid();
            }

            var user = await context.Utilisateurs.FindAsync(id);
            if (user == null) return NotFound();

            user.NomComplet = miseUtilisateur.NomComplet;
            user.Telephone = miseUtilisateur.Telephone;
            user.Adresse = miseUtilisateur.Adresse;

            if (userRole == "ADMIN" && !string.IsNullOrEmpty(miseUtilisateur.Role))
                user.Role = miseUtilisateur.Role;

            return await SaveAsyncChanges(context, new
            {
                message = "Profil mis a jour",
                user = new UserResponseDto
                {
                    IdUtilisateur = user.IdUtilisateur,
                    NomComplet = user.NomComplet,
                    Email = user.Email,
                    Telephone = user.Telephone,
                    Adresse = user.Adresse,
                    Avatar = user.Avatar,
                    Role = user.Role,
                    CreerA = user.CreerA
                }
            });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Delete(int id)
        {
            var userIdFromToken = User.FindFirst("id")?.Value;
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            if (userIdFromToken == null)
                return Unauthorized();

            // Only the user themselves or an ADMIN can delete
            if (userRole != "ADMIN" && userIdFromToken != id.ToString())
                return Forbid();

            var user = await context.Utilisateurs.FindAsync(id);
            if (user == null) return NotFound();

            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                // Find provider profile (if any)
                var prestataire = await context.Prestataires.FirstOrDefaultAsync(p => p.IdUtili == id);

                // 1. Find all RendezVous IDs related to this user
                var rdvIds = await context.RendezVous
                    .Where(r => r.IdUtili == id || (prestataire != null && r.IdPres == prestataire.IdPres))
                    .Select(r => r.IdRendezVous)
                    .ToListAsync();

                // 2. Delete dependent records that reference the user
                var messages = await context.Messages.Where(m => m.IdEnvoyeur == id || m.IdReceveur == id).ToListAsync();
                context.Messages.RemoveRange(messages);

                var favoris = await context.Favoris
                    .Where(f => f.IdUtilisateur == id || (prestataire != null && f.IdPrestataire == prestataire.IdPres))
                    .ToListAsync();
                context.Favoris.RemoveRange(favoris);

                var avis = await context.Avis
                    .Where(a => a.IdUtilisateur == id
                             || (prestataire != null && a.IdPrestataire == prestataire.IdPres)
                             || (a.IdRendezVous.HasValue && rdvIds.Contains(a.IdRendezVous.Value)))
                    .ToListAsync();
                context.Avis.RemoveRange(avis);

                var notifications = await context.Notifications
                    .Where(n => n.UtilisateurId == id || (n.RendezVousId.HasValue && rdvIds.Contains(n.RendezVousId.Value)))
                    .ToListAsync();
                context.Notifications.RemoveRange(notifications);

                // 3. Delete RendezVous
                var rdvs = await context.RendezVous
                    .Where(r => r.IdUtili == id || (prestataire != null && r.IdPres == prestataire.IdPres))
                    .ToListAsync();
                context.RendezVous.RemoveRange(rdvs);

                // 4. Delete provider-related data
                if (prestataire != null)
                {
                    var services = await context.Services.Where(s => s.IdPres == prestataire.IdPres).ToListAsync();
                    context.Services.RemoveRange(services);

                    var disponibilites = await context.Disponibilites.Where(d => d.IdPres == prestataire.IdPres).ToListAsync();
                    context.Disponibilites.RemoveRange(disponibilites);

                    var photos = await context.Prestatairephotos.Where(p => p.PrestataireId == prestataire.IdPres).ToListAsync();
                    context.Prestatairephotos.RemoveRange(photos);

                    context.Prestataires.Remove(prestataire);
                }

                // 5. Finally delete the user
                context.Utilisateurs.Remove(user);

                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { message = "Utilisateur supprimé avec succès." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "Erreur lors de la suppression.", details = ex.InnerException?.Message ?? ex.Message });
            }
        }

        [HttpPost("{id}/avatar")]
        [Authorize]
        public async Task<IActionResult> UploadAvatar(int id, IFormFile File)
        {
            var tokenId = User.FindFirst("id")?.Value;
            if (tokenId == null || tokenId != id.ToString()) return Forbid();

            var user = await context.Utilisateurs.FindAsync(id);
            if (user == null) return NotFound(new { message = "Utilisateur Introuvable" });

            if (File == null || File.Length == 0) return BadRequest(new { message = "Aucun Fichier" });
            try
            {
                if (!string.IsNullOrEmpty(user.Avatar))
                    await cloudinaryService.DeleteImageAsync(user.Avatar);

                var avatarUrl = await cloudinaryService.UploadImageAsync(File);
                user.Avatar = avatarUrl;

                await context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Avatar mis a jour",
                    avatarUrl = avatarUrl
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpPut("{id}/toggle-block")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> ToggleBlock(int id)
        {
            var user = await context.Utilisateurs.FindAsync(id);
            if (user == null) return NotFound(new { message = "Utilisateur introuvable." });
            if (user.Role == "ADMIN") return BadRequest(new { message = "Impossible de bloquer un administrateur." });

            user.IsBlocked = !user.IsBlocked;
            return await SaveAsyncChanges(context, new
            {
                message = user.IsBlocked ? "Utilisateur bloqué." : "Utilisateur réactivé.",
                isBlocked = user.IsBlocked
            });
        }

        [HttpDelete("{id}/avatar")]
        [Authorize]
        public async Task<IActionResult> DeleteAvatar(int id)
        {
            var tokenId = User.FindFirst("id")?.Value;
            if (tokenId == null || tokenId != id.ToString()) return Forbid();

            var user = await context.Utilisateurs.FindAsync(id);
            if (user == null) return NotFound(new { message = "Utilisateur Introuvable" });

            if (string.IsNullOrEmpty(user.Avatar)) return BadRequest(new { message = "Aucun avatar a supprimer" });
            await cloudinaryService.DeleteImageAsync(user.Avatar);
            user.Avatar = null;
            await context.SaveChangesAsync();

            return Ok(new
            {
                message = "Avatar supprimé"
            });
        }
    }
}
