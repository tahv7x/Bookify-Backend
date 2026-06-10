using System;

namespace Bookify_API.Models;

public partial class Disponibilite
{
    public int IdDispo { get; set; }

    public int IdPres { get; set; }

    public string JourSemaine { get; set; } = null!;

    public TimeSpan HeureDebut { get; set; }

    public TimeSpan HeureFin { get; set; }

    public bool Disponible { get; set; }

    public virtual Prestataire IdPresNavigation { get; set; } = null!;
}
