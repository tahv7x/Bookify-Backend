using Bookify_API.DTOs;
using Bookify_API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Bookify_API.Controllers
{
    [Route("api/admin")]
    [ApiController]
    [Authorize(Roles = "ADMIN")]
    public class AdminController : BaseController
    {
        private readonly BookifyDbContext _context;

        public AdminController(BookifyDbContext context)
        {
            _context = context;
        }

        // ── GET /api/admin/stats ──────────────────────────────────────────────
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var now = DateTime.Now;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1);
            var lastMonthStart = startOfMonth.AddMonths(-1);

            // Users
            var totalUsers = await _context.Utilisateurs.CountAsync(u => u.Role != "ADMIN");
            var totalClients = await _context.Utilisateurs.CountAsync(u => u.Role == "CLIENT");
            var totalPrestataires = await _context.Utilisateurs.CountAsync(u => u.Role == "PRESTATAIRE");
            var newUsersThisMonth = await _context.Utilisateurs.CountAsync(u => u.Role != "ADMIN" && u.CreerA >= startOfMonth && u.CreerA < endOfMonth);
            var newUsersLastMonth = await _context.Utilisateurs.CountAsync(u => u.Role != "ADMIN" && u.CreerA >= lastMonthStart && u.CreerA < startOfMonth);

            // Bookings
            var totalBookings = await _context.RendezVous.CountAsync();
            var bookingsThisMonth = await _context.RendezVous.CountAsync(r => r.DateCreation >= startOfMonth && r.DateCreation < endOfMonth);
            var bookingsLastMonth = await _context.RendezVous.CountAsync(r => r.DateCreation >= lastMonthStart && r.DateCreation < startOfMonth);
            var acceptedBookings = await _context.RendezVous.CountAsync(r => r.Statut == "ACCEPTE" || r.Statut == "TERMINE");
            var acceptanceRate = totalBookings > 0 ? Math.Round((double)acceptedBookings / totalBookings * 100, 1) : 0.0;

            // Revenue
            var totalRevenue = await _context.RendezVous
                .Include(r => r.IdSerNavigation)
                .Where(r => r.Statut == "TERMINE" && r.IdSerNavigation != null)
                .SumAsync(r => (decimal?)r.IdSerNavigation.Prix) ?? 0;

            var revenueThisMonth = await _context.RendezVous
                .Include(r => r.IdSerNavigation)
                .Where(r => r.Statut == "TERMINE" && r.DateCreation >= startOfMonth && r.DateCreation < endOfMonth && r.IdSerNavigation != null)
                .SumAsync(r => (decimal?)r.IdSerNavigation.Prix) ?? 0;

            var revenueLastMonth = await _context.RendezVous
                .Include(r => r.IdSerNavigation)
                .Where(r => r.Statut == "TERMINE" && r.DateCreation >= lastMonthStart && r.DateCreation < startOfMonth && r.IdSerNavigation != null)
                .SumAsync(r => (decimal?)r.IdSerNavigation.Prix) ?? 0;

            // Bookings by day (last 7 days)
            var bookingsByDay = new List<object>();
            for (int i = 6; i >= 0; i--)
            {
                var day = DateTime.Today.AddDays(-i);
                var nextDay = day.AddDays(1);
                var count = await _context.RendezVous.CountAsync(r => r.DateCreation >= day && r.DateCreation < nextDay);
                bookingsByDay.Add(new { name = day.ToString("ddd", new CultureInfo("fr-FR")), total = count });
            }

            // Bookings by status
            var statusGroups = await _context.RendezVous
                .GroupBy(r => r.Statut)
                .Select(g => new { status = g.Key, count = g.Count() })
                .ToListAsync();

            var totalForPct = statusGroups.Sum(s => s.count);
            var bookingsByStatus = statusGroups.Select(s => new
            {
                name = s.status switch
                {
                    "ACCEPTE" => "Accepté",
                    "EN_ATTENTE" => "En attente",
                    "ANNULE" => "Annulé",
                    "TERMINE" => "Terminé",
                    "REFUSE" => "Refusé",
                    _ => s.status ?? "Inconnu"
                },
                value = totalForPct > 0 ? (int)Math.Round((double)s.count / totalForPct * 100) : 0,
                color = s.status switch
                {
                    "ACCEPTE" => "#10b981",
                    "EN_ATTENTE" => "#f59e0b",
                    "ANNULE" => "#ef4444",
                    "TERMINE" => "#6366f1",
                    _ => "#94a3b8"
                }
            }).ToList();

            // Trends
            double usersTrend = newUsersLastMonth > 0 ? Math.Round((double)(newUsersThisMonth - newUsersLastMonth) / newUsersLastMonth * 100, 1) : 0;
            double bookingsTrend = bookingsLastMonth > 0 ? Math.Round((double)(bookingsThisMonth - bookingsLastMonth) / bookingsLastMonth * 100, 1) : 0;
            double revenueTrend = revenueLastMonth > 0 ? Math.Round((double)(revenueThisMonth - revenueLastMonth) / (double)revenueLastMonth * 100, 1) : 0;

            return Ok(new
            {
                totalUsers,
                totalClients,
                totalPrestataires,
                newUsersThisMonth,
                usersTrend,
                totalBookings,
                bookingsThisMonth,
                bookingsTrend,
                acceptanceRate,
                totalRevenue,
                revenueThisMonth,
                revenueTrend,
                bookingsByDay,
                bookingsByStatus
            });
        }

        // ── GET /api/admin/recent-activity ────────────────────────────────────
        [HttpGet("recent-activity")]
        public async Task<IActionResult> GetRecentActivity()
        {
            var recentUsers = await _context.Utilisateurs
                .Where(u => u.Role != "ADMIN")
                .OrderByDescending(u => u.CreerA)
                .Take(5)
                .Select(u => new
                {
                    type = "user",
                    text = u.Role == "CLIENT" ? "Nouveau client inscrit" : "Nouveau prestataire inscrit",
                    sub = u.NomComplet,
                    createdAt = u.CreerA ?? DateTime.Now
                })
                .ToListAsync();

            var recentBookings = await _context.RendezVous
                .Include(r => r.IdUtiliNavigation)
                .Include(r => r.IdSerNavigation)
                .OrderByDescending(r => r.DateCreation)
                .Take(5)
                .Select(r => new
                {
                    type = "booking",
                    text = r.Statut == "EN_ATTENTE" ? "Nouvelle réservation"
                                : r.Statut == "ACCEPTE" ? "Réservation confirmée"
                                : "Réservation mise à jour",
                    sub = r.IdUtiliNavigation.NomComplet + " · " + r.IdSerNavigation.Nom,
                    createdAt = r.DateCreation ?? DateTime.Now
                })
                .ToListAsync();

            var activities = recentUsers.Cast<object>()
                .Concat(recentBookings.Cast<object>())
                .OrderByDescending(a => (DateTime)((dynamic)a).createdAt)
                .Take(8)
                .Select(a => new
                {
                    ((dynamic)a).type,
                    ((dynamic)a).text,
                    ((dynamic)a).sub,
                    timeAgo = TimeAgo((DateTime)((dynamic)a).createdAt)
                })
                .ToList();

            return Ok(activities);
        }

        // ── GET /api/admin/categories ─────────────────────────────────────────
        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _context.Categories.OrderBy(c => c.Nom).ToListAsync();

            var result = new List<object>();
            foreach (var cat in categories)
            {
                var servicesCount = await _context.Prestataires.CountAsync(p => p.IdCategorie == cat.IdCategorie);
                result.Add(new
                {
                    idCategorie = cat.IdCategorie,
                    nom = cat.Nom,
                    description = cat.Description,
                    isActive = cat.IsActive,
                    createdAt = cat.CreatedAt,
                    servicesCount
                });
            }
            return Ok(result);
        }

        // ── POST /api/admin/categories ────────────────────────────────────────
        [HttpPost("categories")]
        public async Task<IActionResult> CreateCategory(CategoryCreateDto dto)
        {
            if (await _context.Categories.AnyAsync(c => c.Nom.ToLower() == dto.Nom.ToLower()))
                return BadRequest(new { message = "Cette catégorie existe déjà." });

            var cat = new Categorie
            {
                Nom = dto.Nom.Trim(),
                Description = dto.Description,
                IsActive = true,
                CreatedAt = DateTime.Now
            };
            _context.Categories.Add(cat);
            return await SaveAsyncChanges(_context, new { message = "Catégorie créée.", categorie = cat });
        }

        // ── PUT /api/admin/categories/{id} ────────────────────────────────────
        [HttpPut("categories/{id}")]
        public async Task<IActionResult> UpdateCategory(int id, CategoryUpdateDto dto)
        {
            var cat = await _context.Categories.FindAsync(id);
            if (cat == null) return NotFound(new { message = "Catégorie introuvable." });

            if (dto.Nom != null) cat.Nom = dto.Nom.Trim();
            if (dto.Description != null) cat.Description = dto.Description;
            if (dto.IsActive.HasValue) cat.IsActive = dto.IsActive.Value;

            return await SaveAsyncChanges(_context, new { message = "Catégorie mise à jour.", categorie = cat });
        }

        // ── DELETE /api/admin/categories/{id} ─────────────────────────────────
        [HttpDelete("categories/{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var cat = await _context.Categories.FindAsync(id);
            if (cat == null) return NotFound(new { message = "Catégorie introuvable." });

            _context.Categories.Remove(cat);
            return await SaveAsyncChanges(_context, new { message = "Catégorie supprimée." });
        }

        // ── Helpers ───────────────────────────────────────────────────────────
        private static string TimeAgo(DateTime dt)
        {
            var span = DateTime.Now - dt;
            if (span.TotalMinutes < 1) return "À l'instant";
            if (span.TotalMinutes < 60) return $"Il y a {(int)span.TotalMinutes} min";
            if (span.TotalHours < 24) return $"Il y a {(int)span.TotalHours}h";
            if (span.TotalDays < 7) return $"Il y a {(int)span.TotalDays} jour{((int)span.TotalDays > 1 ? "s" : "")}";
            return dt.ToString("dd MMM yyyy", new CultureInfo("fr-FR"));
        }
    }
}
