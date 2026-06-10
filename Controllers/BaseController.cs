using Bookify_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Bookify_API.Controllers
{
    [ApiController]
    public abstract class BaseController : ControllerBase
    {
        protected int GetUserId()
        {
            var claim = User.FindFirst("id") 
                     ?? User.FindFirst(ClaimTypes.NameIdentifier);

            if (claim == null)
                throw new UnauthorizedAccessException("Token invalide : claim 'id' introuvable.");

            return int.Parse(claim.Value);
        }

        protected async Task<IActionResult> SaveAsyncChanges(BookifyDbContext context, object? payload = null)
        {
            try
            {
                await context.SaveChangesAsync();
                if (payload == null) return Ok(new { message = "Succès" });
                return Ok(payload);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return Conflict(new { message = "Conflit de concurrence", detail = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                return BadRequest(new { message = "Erreur de mise à jour en base", detail = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur serveur", detail = ex.Message });
            }
        }
    }
}
