using System;
using System.Collections.Generic;

namespace Bookify_API.Models;

public partial class RendezVou
{
    public int IdRendezVous { get; set; }


    public int IdUtili { get; set; }

    public int IdPres { get; set; }

    public int IdSer { get; set; }

    public DateTime DateDebut { get; set; }

    public DateTime? DateFin { get; set; }

    public string? Statut { get; set; }

    public string? Lieu { get; set; }

    public DateTime? DateCreation { get; set; }

    public virtual Prestataire IdPresNavigation { get; set; } = null!;

    public virtual Service IdSerNavigation { get; set; } = null!;

    public virtual Utilisateur IdUtiliNavigation { get; set; } = null!;
}
