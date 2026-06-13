namespace Bookify_API.DTOs
{
    public class UserResponseDto
    {
        public int IdUtilisateur { get; set; }
        public string NomComplet { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Telephone { get; set; }
        public string? Adresse { get; set; }
        public string? Avatar { get; set; }
        public string? Role { get; set; }
        public DateTime? CreerA { get; set; }
        public bool IsBlocked { get; set; }
    }
}
