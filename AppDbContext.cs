using System;
using System.Collections.Generic;
using Bookify_API.Models;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace Bookify_API;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Fichier> Fichiers { get; set; }

    public virtual DbSet<Prestataire> Prestataires { get; set; }

    public virtual DbSet<RendezVou> RendezVous { get; set; }

    public virtual DbSet<Service> Services { get; set; }

    public virtual DbSet<Utilisateur> Utilisateurs { get; set; }

}
