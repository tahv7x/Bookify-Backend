using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Bookify_API.Models;

[Table("service")]
[Index("IdPres", Name = "idPres")]
public partial class Service
{
    [Key]
    [Column("idService")]
    public int IdService { get; set; }

    [Column("idPres")]
    public int IdPres { get; set; }

    [Column("nom")]
    [StringLength(255)]
    public string? Nom { get; set; }

    [Column("duration")]
    public int Duration { get; set; }

    [Column("prix")]
    [Precision(10, 2)]
    public decimal Prix { get; set; }

    [ForeignKey("IdPres")]
    [InverseProperty("Services")]
    public virtual Prestataire IdPresNavigation { get; set; } = null!;

    [InverseProperty("IdSerNavigation")]
    public virtual ICollection<RendezVou> RendezVous { get; set; } = new List<RendezVou>();
}
