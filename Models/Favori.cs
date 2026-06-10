using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bookify_API.Models;

public partial class Favori
{
    [Key]
    public int IdFavori { get; set; }

    public int IdUtilisateur { get; set; }

    public int IdPrestataire { get; set; }

    public DateTime DateAjout { get; set; } = DateTime.Now;

    [ForeignKey("IdUtilisateur")]
    public virtual Utilisateur Utilisateur { get; set; } = null!;

    [ForeignKey("IdPrestataire")]
    public virtual Prestataire Prestataire { get; set; } = null!;
}
