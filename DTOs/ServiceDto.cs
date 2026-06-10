using System.ComponentModel.DataAnnotations;

namespace Bookify_API.DTOs
{
    public class ServiceDto
    {
        public int IdService { get; set; }
        
        [Required]
        public string Nom { get; set; } = null!;
        
        [Required]
        public string Description { get; set; } = null!;
        
        public decimal Prix { get; set; }
        
        public int Duree { get; set; } = 1;
        
        public string UniteDuree { get; set; } = "HEURE";
    }
}
