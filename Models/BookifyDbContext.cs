using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace Bookify_API.Models;

public partial class BookifyDbContext : DbContext
{
    public BookifyDbContext()
    {
    }

    public BookifyDbContext(DbContextOptions<BookifyDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Prestataire> Prestataires { get; set; }

    public virtual DbSet<Prestatairephoto> Prestatairephotos { get; set; }

    public virtual DbSet<RendezVou> RendezVous { get; set; }

    public virtual DbSet<Service> Services { get; set; }

    public virtual DbSet<Utilisateur> Utilisateurs { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Message> Messages { get; set; }

    public virtual DbSet<Disponibilite> Disponibilites { get; set; }

    public virtual DbSet<Favori> Favoris { get; set; }

    public virtual DbSet<Avis> Avis { get; set; }

    public virtual DbSet<Categorie> Categories { get; set; }

    public virtual DbSet<SupportTicket> SupportTickets { get; set; }

    public virtual DbSet<SupportMessage> SupportMessages { get; set; }

    public virtual DbSet<Faq> Faqs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // The connection string is managed via Program.cs and appsettings.json / .env
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Favori>(entity =>
        {
            entity.HasKey(e => e.IdFavori).HasName("PRIMARY");
            entity.ToTable("favoris");
            entity.HasIndex(e => e.IdUtilisateur, "idUtilisateur_favoris");
            entity.HasIndex(e => e.IdPrestataire, "idPrestataire_favoris");

            entity.Property(e => e.IdFavori).HasColumnName("idFavori");
            entity.Property(e => e.IdUtilisateur).HasColumnName("idUtilisateur");
            entity.Property(e => e.IdPrestataire).HasColumnName("idPrestataire");
            entity.Property(e => e.DateAjout).HasColumnName("dateAjout").HasColumnType("datetime");

            entity.HasOne(d => d.Utilisateur).WithMany()
                .HasForeignKey(d => d.IdUtilisateur)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_favoris_utilisateur");

            entity.HasOne(d => d.Prestataire).WithMany()
                .HasForeignKey(d => d.IdPrestataire)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_favoris_prestataire");
        });

        modelBuilder.Entity<Prestataire>(entity =>
        {
            entity.HasKey(e => e.IdPres).HasName("PRIMARY");

            entity.ToTable("prestataire");

            entity.HasIndex(e => e.IdUtili, "idUtili");

            entity.Property(e => e.IdPres).HasColumnName("idPres");
            entity.Property(e => e.Bio)
                .HasColumnType("text")
                .HasColumnName("bio");
            entity.Property(e => e.IdCategorie).HasColumnName("idCategorie");
            entity.HasOne(e => e.IdCategorieNavigation)
                .WithMany()
                .HasForeignKey(e => e.IdCategorie)
                .OnDelete(DeleteBehavior.SetNull);
            entity.Property(e => e.IdUtili).HasColumnName("idUtili");
            entity.Property(e => e.Note)
                .HasPrecision(2, 1)
                .HasDefaultValueSql("'0.0'")
                .HasColumnName("note");
            entity.Property(e => e.Speciallite)
                .HasMaxLength(255)
                .HasColumnName("speciallite");
            entity.Property(e => e.Latitude).HasColumnName("latitude");
            entity.Property(e => e.Longitude).HasColumnName("longitude");

            entity.Property(e => e.EnLocal)
                .HasColumnType("tinyint(1)")
                .HasColumnName("enLocal");

            entity.Property(e => e.ADomicile)
                .HasColumnType("tinyint(1)")
                .HasColumnName("aDomicile");

            entity.HasOne(d => d.IdUtiliNavigation).WithMany(p => p.Prestataires)
                .HasForeignKey(d => d.IdUtili)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("prestataire_ibfk_1");
        });

        modelBuilder.Entity<Prestatairephoto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("prestatairephotos");

            entity.HasIndex(e => e.PrestataireId, "prestataireId");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("createdAt");
            entity.Property(e => e.PrestataireId).HasColumnName("prestataireId");
            entity.Property(e => e.Url)
                .HasMaxLength(255)
                .HasColumnName("url");

            entity.HasOne(d => d.Prestataire).WithMany(p => p.Prestatairephotos)
                .HasForeignKey(d => d.PrestataireId)
                .HasConstraintName("prestatairephotos_ibfk_1");
        });

        modelBuilder.Entity<RendezVou>(entity =>
        {
            entity.HasKey(e => e.IdRendezVous).HasName("PRIMARY");

            entity.ToTable("rendez_vous");

            entity.HasIndex(e => e.IdPres, "idPres");

            entity.HasIndex(e => e.IdSer, "idSer");

            entity.HasIndex(e => e.IdUtili, "idUtili");

            entity.Property(e => e.IdRendezVous).HasColumnName("idRendez_vous");
            entity.Property(e => e.DateCreation)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("date_creation");
            entity.Property(e => e.DateDebut)
                .HasColumnType("datetime")
                .HasColumnName("date_debut");
            entity.Property(e => e.DateFin)
                .HasColumnType("datetime")
                .HasColumnName("date_fin");
            entity.Property(e => e.IdPres).HasColumnName("idPres");
            entity.Property(e => e.IdSer).HasColumnName("idSer");
            entity.Property(e => e.IdUtili).HasColumnName("idUtili");
            entity.Property(e => e.Statut)
                .HasDefaultValueSql("'EN_ATTENTE'")
                .HasColumnType("enum('EN_ATTENTE','ACCEPTE','REFUSE','ANNULE','TERMINE','A_REPLANIFIER')")
                .HasColumnName("statut");

            entity.HasOne(d => d.IdPresNavigation).WithMany(p => p.RendezVous)
                .HasForeignKey(d => d.IdPres)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("rendez_vous_ibfk_2");

            entity.HasOne(d => d.IdSerNavigation).WithMany(p => p.RendezVous)
                .HasForeignKey(d => d.IdSer)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("rendez_vous_ibfk_3");

            entity.HasOne(d => d.IdUtiliNavigation).WithMany(p => p.RendezVous)
                .HasForeignKey(d => d.IdUtili)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("rendez_vous_ibfk_1");
        });

        modelBuilder.Entity<Service>(entity =>
        {
            entity.HasKey(e => e.IdService).HasName("PRIMARY");

            entity.ToTable("service");

            entity.HasIndex(e => e.IdPres, "idPres");

            entity.Property(e => e.IdService).HasColumnName("idService");
            entity.Property(e => e.Duree).HasColumnName("duration");
            entity.Property(e => e.IdPres).HasColumnName("idPres");
            entity.Property(e => e.Nom)
                .HasMaxLength(255)
                .HasColumnName("nom");
            entity.Property(e => e.Prix)
                .HasPrecision(10, 2)
                .HasColumnName("prix");

            entity.HasOne(d => d.IdPresNavigation).WithMany(p => p.Services)
                .HasForeignKey(d => d.IdPres)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("service_ibfk_1");
        });

        modelBuilder.Entity<Utilisateur>(entity =>
        {
            entity.HasKey(e => e.IdUtilisateur).HasName("PRIMARY");

            entity.ToTable("utilisateur");

            entity.HasIndex(e => e.Email, "email").IsUnique();

            entity.Property(e => e.IdUtilisateur).HasColumnName("idUtilisateur");
            entity.Property(e => e.Adresse)
                .HasMaxLength(50)
                .HasColumnName("adresse");
            entity.Property(e => e.Avatar)
                .HasMaxLength(255)
                .HasColumnName("avatar");
            entity.Property(e => e.CreerA)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("creerA");
            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .HasColumnName("email");
            entity.Property(e => e.NomComplet)
                .HasMaxLength(100)
                .HasColumnName("nomComplet");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("passwordHash");
            entity.Property(e => e.ResetCodeExpiry)
                .HasColumnType("datetime")
                .HasColumnName("resetCodeExpiry");
            entity.Property(e => e.ResetPasswordCode)
                .HasMaxLength(6)
                .HasColumnName("resetPasswordCode");
            entity.Property(e => e.Role)
                .HasDefaultValueSql("'CLIENT'")
                .HasColumnType("enum('CLIENT','PRESTATAIRE','ADMIN')")
                .HasColumnName("role");
            entity.Property(e => e.Telephone)
                .HasMaxLength(20)
                .HasColumnName("telephone");
            entity.Property(e => e.IsBlocked)
                .HasColumnType("tinyint(1)")
                .HasColumnName("isBlocked");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
            entity.ToTable("notifications");
            entity.Property(e => e.Id).HasColumnName("Id");
            entity.Property(e => e.UtilisateurId).HasColumnName("UtilisateurId");
            entity.Property(e => e.Title).HasMaxLength(255).HasColumnName("Title");
            entity.Property(e => e.Message).HasColumnType("text").HasColumnName("Message");
            entity.Property(e => e.IsRead).HasDefaultValue(false).HasColumnName("IsRead");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnType("datetime").HasColumnName("CreatedAt");

            entity.HasOne(d => d.Utilisateur).WithMany()
                .HasForeignKey(d => d.UtilisateurId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("notifications_ibfk_1");

            entity.Property(e => e.RendezVousId).HasColumnName("RendezVousId");
            entity.HasOne(d => d.RendezVous).WithMany()
                .HasForeignKey(d => d.RendezVousId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.IdMessage).HasName("PRIMARY");
            entity.ToTable("message");

            entity.Property(e => e.IdMessage).HasColumnName("idMessage");
            entity.Property(e => e.IdEnvoyeur).HasColumnName("idEnvoyeur");
            entity.Property(e => e.IdReceveur).HasColumnName("idReceveur");
            entity.Property(e => e.Contenu)
                .HasColumnType("text")
                .HasColumnName("contenu");
            entity.Property(e => e.EnvoieA)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("envoieA");
            entity.Property(e => e.Lu)
                .HasDefaultValue(false)
                .HasColumnName("lu");

            entity.HasOne(d => d.IdEnvoyeurNavigation)
                .WithMany()
                .HasForeignKey(d => d.IdEnvoyeur)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("message_ibfk_1");

            entity.HasOne(d => d.IdReceveurNavigation)
                .WithMany()
                .HasForeignKey(d => d.IdReceveur)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("message_ibfk_2");
        });

        modelBuilder.Entity<Disponibilite>(entity =>
        {
            entity.HasKey(e => e.IdDispo).HasName("PRIMARY");
            entity.ToTable("disponibilite");

            entity.Property(e => e.IdDispo).HasColumnName("idDispo");
            entity.Property(e => e.IdPres).HasColumnName("idPres");
            entity.Property(e => e.JourSemaine)
                .HasColumnType("enum('Lun','Mar','Mer','Jeu','Ven','Sam','Dim')")
                .HasColumnName("jourSemaine");
            entity.Property(e => e.HeureDebut).HasColumnName("heureDebut");
            entity.Property(e => e.HeureFin).HasColumnName("heureFin");
            entity.Property(e => e.Disponible)
                .HasDefaultValue(true)
                .HasColumnName("disponible");

            entity.HasOne(d => d.IdPresNavigation)
                .WithMany()
                .HasForeignKey(d => d.IdPres)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("disponibilite_ibfk_1");
        });

        modelBuilder.Entity<Avis>(entity =>
        {
            entity.HasKey(e => e.IdAvis).HasName("PRIMARY");
            entity.ToTable("avis");

            entity.HasIndex(e => e.IdUtilisateur, "idUtilisateur_avis");
            entity.HasIndex(e => e.IdPrestataire, "idPrestataire_avis");
            entity.HasIndex(e => e.IdRendezVous, "idRendezVous_avis");

            entity.Property(e => e.IdAvis).HasColumnName("idAvis");
            entity.Property(e => e.IdUtilisateur).HasColumnName("idUtilisateur");
            entity.Property(e => e.IdPrestataire).HasColumnName("idPrestataire");
            entity.Property(e => e.IdRendezVous).HasColumnName("idRendezVous");
            entity.Property(e => e.Note).HasColumnName("note");
            entity.Property(e => e.Commentaire).HasColumnType("text").HasColumnName("commentaire");
            entity.Property(e => e.DateCreation).HasColumnName("dateCreation").HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.Utilisateur).WithMany()
                .HasForeignKey(d => d.IdUtilisateur)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_avis_utilisateur");

            entity.HasOne(d => d.Prestataire).WithMany()
                .HasForeignKey(d => d.IdPrestataire)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_avis_prestataire");

            entity.HasOne(d => d.RendezVous).WithMany()
                .HasForeignKey(d => d.IdRendezVous)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_avis_rendezvous");
        });

        modelBuilder.Entity<Categorie>(entity =>
        {
            entity.HasKey(e => e.IdCategorie).HasName("PRIMARY");
            entity.ToTable("categorie");
            entity.HasIndex(e => e.Nom).IsUnique();
            entity.Property(e => e.IdCategorie).HasColumnName("idCategorie");
            entity.Property(e => e.Nom).HasMaxLength(100).HasColumnName("nom");
            entity.Property(e => e.Description).HasColumnType("text").HasColumnName("description");
            entity.Property(e => e.IsActive).HasColumnType("tinyint(1)").HasColumnName("isActive");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnType("datetime").HasColumnName("createdAt");
        });

        modelBuilder.Entity<SupportTicket>(entity =>
        {
            entity.HasKey(e => e.IdTicket).HasName("PRIMARY");
            entity.ToTable("support_ticket");
            entity.Property(e => e.IdTicket).HasColumnName("idTicket");
            entity.Property(e => e.IdUtilisateur).HasColumnName("idUtilisateur");
            entity.Property(e => e.Sujet).HasMaxLength(255).HasColumnName("sujet");
            entity.Property(e => e.Statut).HasMaxLength(50).HasDefaultValue("Ouvert").HasColumnName("statut");
            entity.Property(e => e.DateCreation).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnType("datetime").HasColumnName("dateCreation");

            entity.HasOne(d => d.Utilisateur).WithMany(p => p.SupportTickets)
                .HasForeignKey(d => d.IdUtilisateur)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_support_ticket_utilisateur");
        });

        modelBuilder.Entity<SupportMessage>(entity =>
        {
            entity.HasKey(e => e.IdMessage).HasName("PRIMARY");
            entity.ToTable("support_message");
            entity.Property(e => e.IdMessage).HasColumnName("idMessage");
            entity.Property(e => e.IdTicket).HasColumnName("idTicket");
            entity.Property(e => e.IdEnvoyeur).HasColumnName("idEnvoyeur");
            entity.Property(e => e.Contenu).HasColumnType("text").HasColumnName("contenu");
            entity.Property(e => e.DateEnvoie).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnType("datetime").HasColumnName("dateEnvoie");

            entity.HasOne(d => d.Ticket).WithMany(p => p.SupportMessages)
                .HasForeignKey(d => d.IdTicket)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_support_message_ticket");

            entity.HasOne(d => d.Envoyeur).WithMany(p => p.SupportMessages)
                .HasForeignKey(d => d.IdEnvoyeur)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_support_message_envoyeur");
        });

        modelBuilder.Entity<Faq>(entity =>
        {
            entity.HasKey(e => e.IdFaq).HasName("PRIMARY");
            entity.ToTable("faq");
            entity.Property(e => e.IdFaq).HasColumnName("idFaq");
            entity.Property(e => e.Question).HasColumnType("text").HasColumnName("question");
            entity.Property(e => e.Reponse).HasColumnType("text").HasColumnName("reponse");
            entity.Property(e => e.DateCreation).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnType("datetime").HasColumnName("dateCreation");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
