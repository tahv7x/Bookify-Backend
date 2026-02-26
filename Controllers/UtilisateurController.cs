using Bookify_API.DTOs;
using Bookify_API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Bookify_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UtilisateurController : ControllerBase
    {
        private readonly AppDbContext context;

        public UtilisateurController(AppDbContext context)
        {
            this.context = context;
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
            var user = context.Utilisateurs.Find(id);
            if (user == null) return NotFound();

            user.NomComplet = miseUtilisateur.NomComplet;
            user.Telephone = miseUtilisateur.Telephone;
            user.Adresse = miseUtilisateur.Adresse;
            user.Role = miseUtilisateur.Role;
            context.SaveChanges();

            return Ok(user);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var user = context.Utilisateurs.Find(id);
            if(user == null) return NotFound();

            context.Utilisateurs.Remove(user);
            context.SaveChanges();

            return Ok("Utilisateur Supprimé");
        }
    }
}
