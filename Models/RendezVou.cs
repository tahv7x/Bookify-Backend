using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Bookify_API.Models;

[Table("rendez_vous")]
[Index("IdPres", Name = "idPres")]
[Index("IdSer", Name = "idSer")]
[Index("IdUtili", Name = "idUtili")]
public partial class RendezVou
{
    [Key]
    [Column("idRendez_vous")]
    public int IdRendezVous { get; set; }

    [Column("idUtili")]
    public int IdUtili { get; set; }

    [Column("idPres")]
    public int IdPres { get; set; }

    [Column("idSer")]
    public int IdSer { get; set; }

    [Column("date_debut", TypeName = "datetime")]
    public DateTime DateDebut { get; set; }

    [Column("date_fin", TypeName = "datetime")]
    public DateTime DateFin { get; set; }

    [Column("statut", TypeName = "enum('EN_ATTENTE','ACCEPTE','REFUSE','ANNULE','TERMINE')")]
    public string? Statut { get; set; }

    [Column("date_creation", TypeName = "datetime")]
    public DateTime? DateCreation { get; set; }

    [InverseProperty("IdRendezVousNavigation")]
    public virtual ICollection<Fichier> Fichiers { get; set; } = new List<Fichier>();

    [ForeignKey("IdPres")]
    [InverseProperty("RendezVous")]
    public virtual Prestataire IdPresNavigation { get; set; } = null!;

    [ForeignKey("IdSer")]
    [InverseProperty("RendezVous")]
    public virtual Service IdSerNavigation { get; set; } = null!;

    [ForeignKey("IdUtili")]
    [InverseProperty("RendezVous")]
    public virtual Utilisateur IdUtiliNavigation { get; set; } = null!;
}
