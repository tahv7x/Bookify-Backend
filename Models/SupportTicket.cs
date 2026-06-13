using System;
using System.Collections.Generic;

namespace Bookify_API.Models;

public partial class SupportTicket
{
    public int IdTicket { get; set; }

    public int IdUtilisateur { get; set; }

    public string Sujet { get; set; } = null!;

    public string Statut { get; set; } = "Ouvert"; // "Ouvert", "En attente", "Résolu"

    public DateTime? DateCreation { get; set; }

    public virtual Utilisateur Utilisateur { get; set; } = null!;

    public virtual ICollection<SupportMessage> SupportMessages { get; set; } = new List<SupportMessage>();
}
