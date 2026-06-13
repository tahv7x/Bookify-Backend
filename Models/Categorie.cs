using System;

namespace Bookify_API.Models;

public class Categorie
{
    public int IdCategorie { get; set; }
    public string Nom { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
