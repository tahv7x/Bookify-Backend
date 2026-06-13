using System;

namespace Bookify_API.Models;

public partial class Avis
{
    public int IdAvis { get; set; }

    public int IdUtilisateur { get; set; }

    public int IdPrestataire { get; set; }

    public int? IdRendezVous { get; set; }

    public int Note { get; set; }

    public string Commentaire { get; set; } = null!;

    public DateTime DateCreation { get; set; }

    public virtual Utilisateur Utilisateur { get; set; } = null!;

    public virtual Prestataire Prestataire { get; set; } = null!;

    public virtual RendezVou? RendezVous { get; set; }
}
