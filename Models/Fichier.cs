using System;
using System.Collections.Generic;

namespace Bookify_API.Models;

public partial class Fichier
{
    public int Idfichier { get; set; }

    public int? IdRendezVous { get; set; }

    public int? IdUtilisateur { get; set; }

    public string? NomFichier { get; set; }

    public string? Url { get; set; }

    public string? TypeMime { get; set; }

    public DateTime? DateUpload { get; set; }

    public virtual RendezVou? IdRendezVousNavigation { get; set; }

    public virtual Utilisateur? IdUtilisateurNavigation { get; set; }
}
