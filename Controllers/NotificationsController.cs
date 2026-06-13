using Bookify_API.DTOs;
using Bookify_API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bookify_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : BaseController
    {
        private readonly BookifyDbContext context;
        public NotificationsController(BookifyDbContext context)
        {
            this.context = context;
        }
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> getNotifications()
        {
            var tokenId = int.Parse(User.FindFirst("id")?.Value);

            var notifications = await context.Notifications
                .Where(n => n.UtilisateurId == tokenId)
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new NotificationDto
                {
                    Id = n.Id,
                    Title = n.Title,
                    Message = n.Message,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt,
                    RendezVousId = n.RendezVousId
                })
                .ToListAsync();
            return Ok(notifications);
        }

        [HttpPut("{id}/read")]
        [Authorize]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var tokenID = int.Parse(User.FindFirst("id")?.Value);
            var notifications = await context.Notifications.FirstOrDefaultAsync(
                n => n.Id == id && n.UtilisateurId == tokenID
            );

            if (notifications == null) return NotFound(new {message = "Notification introuvable" });

            notifications.IsRead = true;
            return await SaveAsyncChanges(context, new { message = "Notification marquée comme lue" });
        }
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var tokenId = int.Parse(User.FindFirst("id")!.Value);

            var notification = await context.Notifications.FirstOrDefaultAsync(
                n => n.Id == id && n.UtilisateurId == tokenId
            );

            if (notification == null) return NotFound(new { message = "Notification introuvable" });

            context.Notifications.Remove(notification);
            return await SaveAsyncChanges(context, new { message = "Notification supprimée avec succès" });
        }
    }
}
