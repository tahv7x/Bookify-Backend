using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Bookify_API.Models;

[Table("prestataire")]
[Index("IdUtili", Name = "idUtili")]
public partial class Prestataire
{
    [Key]
    [Column("idPres")]
    public int IdPres { get; set; }

    [Column("idUtili")]
    public int IdUtili { get; set; }

    [Column("speciallite")]
    [StringLength(100)]
    public string? Speciallite { get; set; }

    [Column("bio", TypeName = "text")]
    public string? Bio { get; set; }

    [ForeignKey("IdUtili")]
    [InverseProperty("Prestataires")]
    public virtual Utilisateur IdUtiliNavigation { get; set; } = null!;

    [InverseProperty("IdPresNavigation")]
    public virtual ICollection<RendezVou> RendezVous { get; set; } = new List<RendezVou>();

    [InverseProperty("IdPresNavigation")]
    public virtual ICollection<Service> Services { get; set; } = new List<Service>();
}
