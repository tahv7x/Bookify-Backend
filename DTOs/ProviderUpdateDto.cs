namespace Bookify_API.DTOs
{
    public class ProviderUpdateDto
    {
        public string NomComplet { get; set; } = null!;
        public string Telephone { get; set; } = null!;
        public string Adresse { get; set; } = null!;
        public string? Specialite { get; set; }
        public string? Bio { get; set; }
        public int? IdCategorie { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public bool EnLocal { get; set; }
        public bool ADomicile { get; set; }
    }
}
