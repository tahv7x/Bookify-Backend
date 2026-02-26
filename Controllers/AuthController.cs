using Bookify_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using BCrypt.Net;
using Bookify_API.DTOs;
using Microsoft.AspNetCore.Identity;
using Bookify_API.Services;

namespace Bookify_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext context;
        private readonly IConfiguration configuration;
        private readonly EmailService emailService;

        public AuthController(AppDbContext context, IConfiguration config,EmailService em)
        {
            this.context = context;
            this.configuration = config;
            this.emailService = em;
        }

        private string GenerateJwtToken(Utilisateur user, IConfiguration config)
        {
            var jwtSettings = config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim("role", user.Role),
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
                Role = dto.Role
            };

            context.Utilisateurs.Add(user);
            await context.SaveChangesAsync();
            return Ok("Utilisateur créé avec succès");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await context.Utilisateurs.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null) return Unauthorized("Utilisateur non trouvé");

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized("Mot de passe incorrect");

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
                    role = user.Role
                }
            });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto dto)
        {
            var user = await context.Utilisateurs.FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null)
                return BadRequest("Email Non trouvé");
            var code = new Random().Next(100000, 999999).ToString();

            user.ResetPasswordCode = code;
            user.ResetCodeExpiry = DateTime.Now.AddMinutes(10);

            await context.SaveChangesAsync();

            emailService.Send(
                user.Email,
                "Code de réinitialisation du mot de passe",
                $"Votre code de vérification est : {code}"
            );
            return Ok("Code de verification envoyé Par Email ");
        }
        [HttpPost("verify-reset-code")]
        public async Task<IActionResult> VerifyResetCode(VerifyCodeDto dto)
        {
            var user = await context.Utilisateurs.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null)
                return BadRequest("Utilisateur Introuvable");
            if(user.ResetPasswordCode != dto.Code || user.ResetCodeExpiry < DateTime.Now)
            {
                return BadRequest("Code Invalide ou expire");
            }

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

            await context.SaveChangesAsync();

            return Ok("Mot de passe modifié avec succès");
        }

    }
}
