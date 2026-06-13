namespace Bookify_API.DTOs
{
    public class DispoDto
    {
        public string JourSemaine { get; set; } = string.Empty;
        public string? HeureDebut { get; set; }
        public string? HeureFin { get; set; }
        public bool Disponible { get; set; }
    }
}
