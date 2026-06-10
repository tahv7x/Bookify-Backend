using System;

namespace Bookify_API.Models;

public partial class Message
{
    public int IdMessage { get; set; }

    public int IdEnvoyeur { get; set; }

    public int IdReceveur { get; set; }

    public string Contenu { get; set; } = null!;

    public DateTime? EnvoieA { get; set; }

    public bool Lu { get; set; }

    public virtual Utilisateur IdEnvoyeurNavigation { get; set; } = null!;
    public virtual Utilisateur IdReceveurNavigation { get; set; } = null!;
}