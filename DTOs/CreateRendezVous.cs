namespace Bookify_API.DTOs
{
    public class CreateRendezVous
        {
            public int idPres { get; set; }
            public int idServ { get; set; }
            public DateTime DateDebut { get; set; }
            public DateTime DateFin { get; set; }
            public string? Lieu { get; set; }
        }
}
