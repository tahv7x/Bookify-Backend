namespace Bookify_API.DTOs
{
    public class DisponibiliteDto
    {
        public string JourSemaine { get; set; } = string.Empty;
        public string HeureDebut { get; set; } = "09:00";
        public string HeureFin { get; set; } = "18:00";
        public bool Disponible { get; set; }
    }
}
