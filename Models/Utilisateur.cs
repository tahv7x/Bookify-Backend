using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Bookify_API.Models;

[Table("utilisateur")]
[Index("Email", Name = "email", IsUnique = true)]
public partial class Utilisateur
{
    [Key]
    [Column("idUtilisateur")]
    public int IdUtilisateur { get; set; }

    [Column("nomComplet")]
    [StringLength(100)]
    public string NomComplet { get; set; } = null!;

    [Column("email")]
    [StringLength(150)]
    public string Email { get; set; } = null!;

    [Column("telephone")]
    [StringLength(20)]
    public string? Telephone { get; set; }

    [Column("adresse")]
    [StringLength(50)]
    public string? Adresse { get; set; }

    [Column("passwordHash")]
    [StringLength(255)]
    public string PasswordHash { get; set; } = null!;

    [Column("role", TypeName = "enum('CLIENT','PRESTATAIRE','ADMIN')")]
    public string Role { get; set; } = null!;

    [Column("creerA", TypeName = "datetime")]
    public DateTime? CreerA { get; set; }

    [Column("resetCode")]
    [StringLength(6)]
    public string? ResetPasswordCode { get; set; }

    [Column("resetCodeExpiry",TypeName = "datetime")]
    public DateTime? ResetCodeExpiry { get; set; }


    [InverseProperty("IdUtilisateurNavigation")]
    public virtual ICollection<Fichier> Fichiers { get; set; } = new List<Fichier>();

    [InverseProperty("IdUtiliNavigation")]
    public virtual ICollection<Prestataire> Prestataires { get; set; } = new List<Prestataire>();

    [InverseProperty("IdUtiliNavigation")]
    public virtual ICollection<RendezVou> RendezVous { get; set; } = new List<RendezVou>();
}
