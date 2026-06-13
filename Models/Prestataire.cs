using System;
using System.Collections.Generic;

namespace Bookify_API.Models;

public partial class Prestataire
{
    public int IdPres { get; set; }

    public int IdUtili { get; set; }

    public string? Speciallite { get; set; }

    public string? Bio { get; set; }

    public int? IdCategorie {get;set;}

    public decimal? Note { get; set; }

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    public bool EnLocal { get; set; }

    public bool ADomicile { get; set; }

    public virtual Categorie? IdCategorieNavigation { get; set; }

    public virtual Utilisateur IdUtiliNavigation { get; set; } = null!;

    public virtual ICollection<Prestatairephoto> Prestatairephotos { get; set; } = new List<Prestatairephoto>();

    public virtual ICollection<RendezVou> RendezVous { get; set; } = new List<RendezVou>();

    public virtual ICollection<Service> Services { get; set; } = new List<Service>();
}
