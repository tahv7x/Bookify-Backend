namespace Bookify_API.DTOs
{
    public class ChangePasswordDto
    {
        public string AncienMotDePasse { get; set; } = string.Empty;
        public string NouveauMotDePasse { get; set; } = string.Empty;
    }
}
