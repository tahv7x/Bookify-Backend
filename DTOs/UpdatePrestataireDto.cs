namespace Bookify_API.DTOs
{
    public class UpdatePrestataireDto
    {
        public string NomComplet { get; set; } = null!;
        public string Telephone { get; set; } = null!;
        public string Adresse { get; set; } = null!;
        public string? Specialite { get; set; }
        public string? Bio { get; set; }
        public string? Categorie { get; set; }
    }
}
