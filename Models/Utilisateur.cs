using System;
using System.Collections.Generic;

namespace Bookify_API.Models;

public partial class Utilisateur
{
    public int IdUtilisateur { get; set; }

    public string NomComplet { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Telephone { get; set; }

    public string? Adresse { get; set; }

    public string? Avatar { get; set; }

    public string PasswordHash { get; set; } = null!;

    public string? Role { get; set; }

    public bool IsBlocked { get; set; }

    public string? ResetPasswordCode { get; set; }

    public DateTime? ResetCodeExpiry { get; set; }

    public DateTime? CreerA { get; set; }

    public virtual ICollection<Prestataire> Prestataires { get; set; } = new List<Prestataire>();

    public virtual ICollection<RendezVou> RendezVous { get; set; } = new List<RendezVou>();

    public virtual ICollection<SupportTicket> SupportTickets { get; set; } = new List<SupportTicket>();

    public virtual ICollection<SupportMessage> SupportMessages { get; set; } = new List<SupportMessage>();
}
