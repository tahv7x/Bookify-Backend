namespace Bookify_API.DTOs
{
    public class CategoryCreateDto
    {
        public string Nom { get; set; } = null!;
        public string? Description { get; set; }
    }

    public class CategoryUpdateDto
    {
        public string? Nom { get; set; }
        public string? Description { get; set; }
        public bool? IsActive { get; set; }
    }
}
