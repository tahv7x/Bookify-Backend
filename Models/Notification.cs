using Bookify_API.Controllers;

namespace Bookify_API.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public int UtilisateurId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Utilisateur Utilisateur { get; set; }
        public int? RendezVousId { get; set; }
        public RendezVou? RendezVous { get; set; }
    }
}
