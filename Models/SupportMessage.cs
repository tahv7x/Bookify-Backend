using System;

namespace Bookify_API.Models;

public partial class SupportMessage
{
    public int IdMessage { get; set; }

    public int IdTicket { get; set; }

    public int IdEnvoyeur { get; set; }

    public string Contenu { get; set; } = null!;

    public DateTime? DateEnvoie { get; set; }

    public virtual SupportTicket Ticket { get; set; } = null!;

    public virtual Utilisateur Envoyeur { get; set; } = null!;
}
