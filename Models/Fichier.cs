using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Bookify_API.Models;

[Table("fichier")]
[Index("IdRendezVous", Name = "idRendez_vous")]
[Index("IdUtilisateur", Name = "idUtilisateur")]
public partial class Fichier
{
    [Key]
    [Column("idfichier")]
    public int Idfichier { get; set; }

    [Column("idRendez_vous")]
    public int IdRendezVous { get; set; }

    [Column("idUtilisateur")]
    public int IdUtilisateur { get; set; }

    [Column("nom_fichier")]
    [StringLength(255)]
    public string NomFichier { get; set; } = null!;

    [Column("url", TypeName = "text")]
    public string Url { get; set; } = null!;

    [Column("type_mime")]
    [StringLength(100)]
    public string? TypeMime { get; set; }

    [Column("date_upload", TypeName = "datetime")]
    public DateTime? DateUpload { get; set; }

    [ForeignKey("IdRendezVous")]
    [InverseProperty("Fichiers")]
    public virtual RendezVou IdRendezVousNavigation { get; set; } = null!;

    [ForeignKey("IdUtilisateur")]
    [InverseProperty("Fichiers")]
    public virtual Utilisateur IdUtilisateurNavigation { get; set; } = null!;
}
