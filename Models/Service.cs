using System;
using System.Collections.Generic;

namespace Bookify_API.Models;

public partial class Service
{
    public int IdService { get; set; }

    public int IdPres { get; set; }

    public string? Nom { get; set; }

    public string? Description { get; set; }

    public decimal? Prix { get; set; }

    public int Duree { get; set; } = 1;

    public string UniteDuree { get; set; } = "HEURE";

    public virtual Prestataire IdPresNavigation { get; set; } = null!;

    public virtual ICollection<RendezVou> RendezVous { get; set; } = new List<RendezVou>();
}
