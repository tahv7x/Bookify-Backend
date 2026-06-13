namespace Bookify_API.DTOs
{
    public class CreateAvisDto
    {
        public int IdPrestataire { get; set; }
        public int? IdRendezVous { get; set; }
        public int Note { get; set; }
        public string Commentaire { get; set; } = null!;
    }
}
