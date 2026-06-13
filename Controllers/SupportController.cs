using Bookify_API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Bookify_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SupportController : BaseController
    {
        private readonly BookifyDbContext _context;

        public SupportController(BookifyDbContext context)
        {
            _context = context;
        }

        [HttpGet("tickets")]
        public async Task<IActionResult> GetTickets()
        {
            var userId = GetUserId();
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            IQueryable<SupportTicket> query = _context.SupportTickets
                .Include(t => t.Utilisateur)
                .Include(t => t.SupportMessages)
                .OrderByDescending(t => t.DateCreation);

            if (userRole != "ADMIN")
            {
                query = query.Where(t => t.IdUtilisateur == userId);
            }

            var tickets = await query.Select(t => new
            {
                t.IdTicket,
                UserName = t.Utilisateur.NomComplet,
                UserEmail = t.Utilisateur.Email,
                Subject = t.Sujet,
                Preview = t.SupportMessages.OrderBy(m => m.DateEnvoie).FirstOrDefault() != null 
                    ? t.SupportMessages.OrderBy(m => m.DateEnvoie).First().Contenu.Substring(0, Math.Min(t.SupportMessages.OrderBy(m => m.DateEnvoie).First().Contenu.Length, 100))
                    : "",
                Status = t.Statut,
                Date = t.DateCreation,
                Messages = t.SupportMessages.OrderBy(m => m.DateEnvoie).Select(m => new
                {
                    from = m.IdEnvoyeur == t.IdUtilisateur ? "user" : "admin",
                    text = m.Contenu,
                    time = m.DateEnvoie != null ? m.DateEnvoie.Value.ToString("HH:mm") : ""
                })
            }).ToListAsync();

            return Ok(tickets);
        }

        [HttpGet("tickets/{id}")]
        public async Task<IActionResult> GetTicket(int id)
        {
            var userId = GetUserId();
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            var ticket = await _context.SupportTickets
                .Include(t => t.Utilisateur)
                .Include(t => t.SupportMessages)
                .FirstOrDefaultAsync(t => t.IdTicket == id);

            if (ticket == null) return NotFound("Ticket non trouvé.");

            if (userRole != "ADMIN" && ticket.IdUtilisateur != userId)
                return Forbid();

            var result = new
            {
                ticket.IdTicket,
                UserName = ticket.Utilisateur.NomComplet,
                UserEmail = ticket.Utilisateur.Email,
                Subject = ticket.Sujet,
                Status = ticket.Statut,
                Date = ticket.DateCreation,
                Messages = ticket.SupportMessages.OrderBy(m => m.DateEnvoie).Select(m => new
                {
                    from = m.IdEnvoyeur == ticket.IdUtilisateur ? "user" : "admin",
                    text = m.Contenu,
                    time = m.DateEnvoie != null ? m.DateEnvoie.Value.ToString("HH:mm") : ""
                })
            };

            return Ok(result);
        }

        [HttpPost("tickets")]
        public async Task<IActionResult> CreateTicket([FromBody] CreateTicketDto dto)
        {
            var userId = GetUserId();

            var ticket = new SupportTicket
            {
                IdUtilisateur = userId,
                Sujet = dto.Subject,
                Statut = "Ouvert",
                DateCreation = DateTime.Now
            };

            _context.SupportTickets.Add(ticket);
            await _context.SaveChangesAsync();

            var message = new SupportMessage
            {
                IdTicket = ticket.IdTicket,
                IdEnvoyeur = userId,
                Contenu = dto.Message,
                DateEnvoie = DateTime.Now
            };

            _context.SupportMessages.Add(message);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Ticket créé.", ticketId = ticket.IdTicket });
        }

        [HttpPost("tickets/{id}/messages")]
        public async Task<IActionResult> AddMessage(int id, [FromBody] AddMessageDto dto)
        {
            var userId = GetUserId();
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            var ticket = await _context.SupportTickets.FindAsync(id);
            if (ticket == null) return NotFound("Ticket non trouvé.");

            if (userRole != "ADMIN" && ticket.IdUtilisateur != userId)
                return Forbid();

            var message = new SupportMessage
            {
                IdTicket = id,
                IdEnvoyeur = userId,
                Contenu = dto.Text,
                DateEnvoie = DateTime.Now
            };

            _context.SupportMessages.Add(message);
            
            // Si admin repond, le ticket passe 'En attente' ou reste 'Ouvert', si user repond, il passe 'Ouvert'
            if (userRole == "ADMIN")
            {
                if (ticket.Statut != "Résolu")
                {
                    ticket.Statut = "En attente";
                }

                var chatMessage = new Message
                {
                    IdEnvoyeur = userId,
                    IdReceveur = ticket.IdUtilisateur,
                    Contenu = $"Ticket {ticket.IdTicket} : {ticket.Sujet} \n\n {dto.Text}",
                    EnvoieA = DateTime.Now,
                    Lu = false
                };
                _context.Messages.Add(chatMessage);
                
                var notification = new Notification
                {
                    UtilisateurId = ticket.IdUtilisateur,
                    Title = "Support Bookify",
                    Message = $"Vous avez reçu une réponse pour le ticket #{ticket.IdTicket}",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Notifications.Add(notification);
            }
            else if (userRole != "ADMIN" && ticket.Statut == "En attente")
            {
                ticket.Statut = "Ouvert";
            }

            return await SaveAsyncChanges(_context, new { message = "Message ajouté." });
        }

        [HttpPut("tickets/{id}/status")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> UpdateTicketStatus(int id, [FromBody] UpdateStatusDto dto)
        {
            var ticket = await _context.SupportTickets.FindAsync(id);
            if (ticket == null) return NotFound("Ticket non trouvé.");

            ticket.Statut = dto.Status;
            return await SaveAsyncChanges(_context, new { message = "Statut mis à jour." });
        }
    }

    public class CreateTicketDto
    {
        public string Subject { get; set; } = null!;
        public string Message { get; set; } = null!;
    }

    public class AddMessageDto
    {
        public string Text { get; set; } = null!;
    }

    public class UpdateStatusDto
    {
        public string Status { get; set; } = null!;
    }
}
