using Bookify_API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Bookify_API.DTOs;

namespace Bookify_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MessageController : BaseController 
    {
        private readonly BookifyDbContext context;
        
        public MessageController(BookifyDbContext context)
        {
            this.context = context;
        }

        [HttpGet("contacts")]
        public async Task<IActionResult> GetContacts()
        {
            var myId = GetUserId();

            var contactIds = await context.Messages
                .Where(m => m.IdEnvoyeur == myId || m.IdReceveur == myId)
                .Select(m => m.IdEnvoyeur == myId ? m.IdReceveur : m.IdEnvoyeur)
                .Distinct()
                .ToListAsync();

            var adminIds = await context.Utilisateurs
                .Where(u => u.Role == "ADMIN")
                .Select(u => u.IdUtilisateur)
                .ToListAsync();

            contactIds.AddRange(adminIds);

            var providerIds = await context.RendezVous
                .Where(r => r.IdUtili == myId)
                .Select(r => r.IdPres)
                .Distinct()
                .ToListAsync();
            
            if (providerIds.Count > 0)
            {
                var providerUserIds = await context.Prestataires
                    .Where(p => providerIds.Contains(p.IdPres))
                    .Select(p => p.IdUtili)
                    .ToListAsync();
                contactIds.AddRange(providerUserIds);
            }

            // If the user is a provider, add clients they have rendezvous with
            var myPrestataire = await context.Prestataires.FirstOrDefaultAsync(p => p.IdUtili == myId);
            if (myPrestataire != null)
            {
                var clientUserIds = await context.RendezVous
                    .Where(r => r.IdPres == myPrestataire.IdPres)
                    .Select(r => r.IdUtili)
                    .Distinct()
                    .ToListAsync();
                contactIds.AddRange(clientUserIds);
            }

            contactIds = contactIds.Distinct().Where(id => id != myId).ToList();

            var contacts = new List<ContactDto>();

            foreach (var cid in contactIds)
            {
                var user = await context.Utilisateurs
                    .Include(u => u.Prestataires)
                    .FirstOrDefaultAsync(u => u.IdUtilisateur == cid);
                
                if (user == null) continue;

                var lastMessage = await context.Messages
                    .Where(m => (m.IdEnvoyeur == myId && m.IdReceveur == cid) || (m.IdEnvoyeur == cid && m.IdReceveur == myId))
                    .OrderByDescending(m => m.EnvoieA)
                    .FirstOrDefaultAsync();

                var unreadCount = await context.Messages
                    .CountAsync(m => m.IdEnvoyeur == cid && m.IdReceveur == myId && !m.Lu);

                contacts.Add(new ContactDto
                {
                    id = user.IdUtilisateur,
                    providerId = user.Role == "PRESTATAIRE" && user.Prestataires.Count > 0 ? user.Prestataires.First().IdPres : null,
                    name = user.Role == "ADMIN" ? "Support Bookify" : user.NomComplet,
                    specialty = user.Role == "ADMIN" ? "Administration" : (user.Role == "PRESTATAIRE" && user.Prestataires.Count > 0 ? user.Prestataires.First().Speciallite : "Client"),
                    avatar = user.Avatar,
                    unread = unreadCount,
                    isOnline = true, 
                    lastMessage = lastMessage != null ? lastMessage.Contenu : (user.Role == "ADMIN" ? "Comment pouvons-nous vous aider ?" : "Nouvelle discussion"),
                    time = lastMessage != null && lastMessage.EnvoieA.HasValue ? lastMessage.EnvoieA.Value.ToString("HH:mm") : ""
                });
            }

            return Ok(contacts.OrderByDescending(c => c.unread > 0 ? 1 : 0).ThenByDescending(c => c.time));
        }

        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var myId = GetUserId();
            var count = await context.Messages
                .CountAsync(m => m.IdReceveur == myId && !m.Lu);
            return Ok(new { unreadCount = count });
        }

        [HttpGet("{otherUserId}")]
        public async Task<IActionResult> GetConversation(int otherUserId)
        {
            var myId = GetUserId();

            var messages = await context.Messages
                .Where(m =>
                    (m.IdEnvoyeur == myId && m.IdReceveur == otherUserId) ||
                    (m.IdEnvoyeur == otherUserId && m.IdReceveur == myId))
                .OrderBy(m => m.EnvoieA)
                .Select(m => new
                {
                    id       = m.IdMessage,
                    senderId = m.IdEnvoyeur,
                    content  = m.Contenu,
                    sentAt   = m.EnvoieA,
                    lu       = m.Lu
                })
                .ToListAsync();

            var nonLus = await context.Messages 
                .Where(m => 
                    m.IdEnvoyeur == otherUserId 
                    && m.IdReceveur == myId 
                    && !m.Lu
                )
                .ToListAsync();

            if(nonLus.Count != 0){
                nonLus.ForEach(m => m.Lu = true);
                await context.SaveChangesAsync();
            }

            return Ok(messages);
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody]SendMessageDto dto)
        {
            if(string.IsNullOrWhiteSpace(dto.Content))
                return BadRequest(new {message = "Contenu Vide"});
            
            var myId = GetUserId();
            var msg = new Message
            {
                IdEnvoyeur = myId,
                IdReceveur = dto.ReceiverId,
                Contenu = dto.Content.Trim(),
                Lu = false
            };
            
            context.Messages.Add(msg);

            // Fetch sender to customize notification
            var sender = await context.Utilisateurs.FindAsync(myId);
            var senderName = sender?.NomComplet ?? "Un utilisateur";

            // Create notification for the receiver
            var notification = new Notification
            {
                UtilisateurId = dto.ReceiverId,
                Title = "Nouveau Message",
                Message = $"Vous avez reçu un nouveau message de {senderName}",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };
            context.Notifications.Add(notification);

            await context.SaveChangesAsync();
            
            return Ok(new {id = msg.IdMessage, sentAt = msg.EnvoieA});
        }
    }
}