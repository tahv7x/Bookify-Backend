using Bookify_API.DTOs;
using Bookify_API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Bookify_API.Services;
namespace Bookify_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UtilisateurController : ControllerBase
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
        public IActionResult GetALL()
        {
            var users = context.Utilisateurs.ToList();
            return Ok(users);
        }

        [HttpGet("{id}")]
        [Authorize]
        public IActionResult GetById(int id)
        {
            var user = context.Utilisateurs.Find(id);
            if (user == null) return NotFound();
            return Ok(user);
        }


        [HttpPut("{id}")]
        [Authorize]
        public IActionResult Update(int id, UpdateUtilisateurDto miseUtilisateur)
        {
            var userIdFromToken = User.FindFirst("id")?.Value;
            var userRole = User.FindFirst("role")?.Value;

            if (userIdFromToken == null)
            {
                return Unauthorized();
            }

            if (userRole != "ADMIN" && userIdFromToken != id.ToString())
            {
                return Forbid();
            }

            var user = context.Utilisateurs.Find(id);
            if (user == null) return NotFound();

            user.NomComplet = miseUtilisateur.NomComplet;
            user.Telephone = miseUtilisateur.Telephone;
            user.Adresse = miseUtilisateur.Adresse;

            if (userRole == "ADMIN" && !string.IsNullOrEmpty(miseUtilisateur.Role))
                user.Role = miseUtilisateur.Role;
            context.SaveChanges();
            return Ok(new
            {
                message = "Profil mis a jour",
                user
            });
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var user = context.Utilisateurs.Find(id);
            if (user == null) return NotFound();

            context.Utilisateurs.Remove(user);
            context.SaveChanges();

            return Ok("Utilisateur Supprimé");
        }
        //Upload Avatar
        [HttpPost("{id}/avatar")]
        [Authorize]
        public async Task<IActionResult> UploadeAvatar(int id, IFormFile file)
        {
            var tokenId = User.FindFirst("id")?.Value;
            if (tokenId == null || tokenId != id.ToString()) return Forbid();

            var user = await context.Utilisateurs.FindAsync(id);
            if (user == null) return NotFound(new { message = "Utilisateur Introuvable" });

            if (file == null || file.Length == 0) return BadRequest(new { message = "Aucun FIchier" });
            try
            {
                if(!string.IsNullOrEmpty(user.Avatar)) 
                    await cloudinaryService.DeleteImageAsync(user.Avatar);

                var avatarUrl = await cloudinaryService.UploadImageAsync(file);
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
