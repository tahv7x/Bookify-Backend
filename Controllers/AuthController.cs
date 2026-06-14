using BCrypt.Net;
using Bookify_API.DTOs;
using Bookify_API.Models;
using Bookify_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Bookify_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : BaseController
    {
        private readonly BookifyDbContext context;
        private readonly IConfiguration configuration;
        private readonly EmailService emailService;

        public AuthController(BookifyDbContext context, IConfiguration config, EmailService em)
        {
            this.context = context;
            this.configuration = config;
            this.emailService = em;
        }

        // Generate JWT token
        public string GenerateJwtToken(Utilisateur user, IConfiguration config)
        {
            var jwtSettings = config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("id", user.IdUtilisateur.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(Convert.ToDouble(jwtSettings["ExpireMinutes"])),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (await context.Utilisateurs.AnyAsync(u => u.Email == dto.Email))
                return BadRequest("Email déjà utilisé");

            var user = new Utilisateur
            {
                NomComplet = dto.NomComplet,
                Email = dto.Email,
                Telephone = dto.Telephone,
                Adresse = dto.Adresse,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = dto.Role,
                IsBlocked = false
            };
            context.Utilisateurs.Add(user);

            if (dto.Role == "PRESTATAIRE")
            {
                var prestataire = new Prestataire
                {
                    IdUtiliNavigation = user,
                    Note = 0.0m,
                    Speciallite = null,
                    IdCategorie = null,
                    Bio = null,
                    Latitude = dto.Latitude,
                    Longitude = dto.Longitude,
                    EnLocal = dto.EnLocal ?? true,
                    ADomicile = dto.ADomicile ?? true
                };
                context.Prestataires.Add(prestataire);
            }

            return await SaveAsyncChanges(context, "Utilisateur créé avec succès");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await context.Utilisateurs.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null) return Unauthorized("Utilisateur non trouvé");

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized("Mot de passe incorrect");

            if (user.IsBlocked)
                return Unauthorized(new { message = "Votre compte a été bloqué. Contactez l'administrateur." });

            var token = GenerateJwtToken(user, configuration);

            return Ok(new
            {
                message = "Login réussi",
                token,
                user = new
                {
                    idUtilisateur = user.IdUtilisateur,
                    nom = user.NomComplet,
                    email = user.Email,
                    role = user.Role,
                    adresse = user.Adresse,
                    telephone = user.Telephone,
                    avatar = user.Avatar,
                    isBlocked = user.IsBlocked
                }
            });
        }

        [HttpPut("change-password/{id}")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(int id, ChangePasswordDto dto)
        {
            var userIdFromToken = User.FindFirst("id")?.Value;
            if (userIdFromToken == null || userIdFromToken != id.ToString())
                return Forbid();

            var user = await context.Utilisateurs.FindAsync(id);
            if (user == null) return NotFound("Utilisateur introuvable");

            if (!BCrypt.Net.BCrypt.Verify(dto.AncienMotDePasse, user.PasswordHash))
                return BadRequest(new { message = "Mot de passe actuel incorrect." });

            if (dto.NouveauMotDePasse.Length < 8)
                return BadRequest(new { message = "Le nouveau mot de passe doit contenir au moins 8 caractères." });

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NouveauMotDePasse);
            return await SaveAsyncChanges(context, new { message = "Mot de passe modifié avec succès." });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto dto)
        {
            var user = await context.Utilisateurs.FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null)
                return BadRequest("Si cet email existe, un code de réinitialisation vous a été envoyé.");

            var code = new Random().Next(100000, 999999).ToString();

            user.ResetPasswordCode = code;
            user.ResetCodeExpiry = DateTime.Now.AddMinutes(10);

            await context.SaveChangesAsync();

            // Build the branded Bookify HTML email
            var htmlBody = emailService.BuildResetCodeEmail(user.NomComplet, code);

            emailService.Send(
                user.Email,
                "Code de réinitialisation – Bookify",
                htmlBody,
                isHtml: true
            );

            return Ok("Code de vérification envoyé par Email");
        }

        [HttpPost("verify-reset-code")]
        public async Task<IActionResult> VerifyResetCode(VerifyCodeDto dto)
        {
            var user = await context.Utilisateurs.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null)
                return BadRequest("Utilisateur Introuvable");

            if (user.ResetPasswordCode != dto.Code || user.ResetCodeExpiry < DateTime.Now)
                return BadRequest("Code Invalide ou expiré");

            return Ok("Code Validé");
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
        {
            var user = await context.Utilisateurs
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null)
                return BadRequest("Utilisateur Introuvable");

            if (user.ResetPasswordCode != dto.Code || user.ResetCodeExpiry < DateTime.Now)
                return BadRequest("Code Invalide ou expiré");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.ResetPasswordCode = null;
            user.ResetCodeExpiry = null;

            return await SaveAsyncChanges(context, "Mot de passe modifié avec succès");
        }

        [HttpDelete("delete-account")]
        [Authorize]
        public async Task<IActionResult> DeleteAccount()
        {
            var userId = GetUserId();
            var user = await context.Utilisateurs.FindAsync(userId);
            if (user == null) return NotFound("Utilisateur introuvable");

            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                var prestataire = await context.Prestataires.FirstOrDefaultAsync(p => p.IdUtili == userId);

                // 1. Find all RendezVous IDs related to this user (as client or as provider)
                var rdvIds = await context.RendezVous
                    .Where(r => r.IdUtili == userId || (prestataire != null && r.IdPres == prestataire.IdPres))
                    .Select(r => r.IdRendezVous)
                    .ToListAsync();

                // 2. Delete Messages first (independent)
                var messages = await context.Messages.Where(m => m.IdEnvoyeur == userId || m.IdReceveur == userId).ToListAsync();
                context.Messages.RemoveRange(messages);

                // 3. Delete Favoris (independent)
                var favoris = await context.Favoris
                    .Where(f => f.IdUtilisateur == userId || (prestataire != null && f.IdPrestataire == prestataire.IdPres))
                    .ToListAsync();
                context.Favoris.RemoveRange(favoris);

                // 4. Delete Avis (linked to user, provider, or appointments)
                var avis = await context.Avis
                    .Where(a => a.IdUtilisateur == userId
                             || (prestataire != null && a.IdPrestataire == prestataire.IdPres)
                             || (a.IdRendezVous.HasValue && rdvIds.Contains(a.IdRendezVous.Value)))
                    .ToListAsync();
                context.Avis.RemoveRange(avis);

                // 5. Delete Notifications (linked to user or appointments)
                var notifications = await context.Notifications
                    .Where(n => n.UtilisateurId == userId || (n.RendezVousId.HasValue && rdvIds.Contains(n.RendezVousId.Value)))
                    .ToListAsync();
                context.Notifications.RemoveRange(notifications);

                // 6. Now we can safely delete RendezVous
                var rdvs = await context.RendezVous
                    .Where(r => r.IdUtili == userId || (prestataire != null && r.IdPres == prestataire.IdPres))
                    .ToListAsync();
                context.RendezVous.RemoveRange(rdvs);

                // 8. Delete Provider-specific tables
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

                // 9. Finally delete the user
                context.Utilisateurs.Remove(user);

                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { message = "Compte supprimé avec succès." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "Erreur lors de la suppression.", details = ex.InnerException?.Message ?? ex.Message });
            }
        }
    }
}
