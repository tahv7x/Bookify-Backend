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

    public virtual DbSet<Fichier> Fichiers { get; set; }

    public virtual DbSet<Prestataire> Prestataires { get; set; }

    public virtual DbSet<Prestatairephoto> Prestatairephotos { get; set; }

    public virtual DbSet<RendezVou> RendezVous { get; set; }

    public virtual DbSet<Service> Services { get; set; }

    public virtual DbSet<Utilisateur> Utilisateurs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySql("server=localhost;database=BookifyDB;user=root;password=2006", Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.44-mysql"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Fichier>(entity =>
        {
            entity.HasKey(e => e.Idfichier).HasName("PRIMARY");

            entity.ToTable("fichier");

            entity.HasIndex(e => e.IdRendezVous, "idRendez_vous");

            entity.HasIndex(e => e.IdUtilisateur, "idUtilisateur");

            entity.Property(e => e.Idfichier).HasColumnName("idfichier");
            entity.Property(e => e.DateUpload)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("date_upload");
            entity.Property(e => e.IdRendezVous).HasColumnName("idRendez_vous");
            entity.Property(e => e.IdUtilisateur).HasColumnName("idUtilisateur");
            entity.Property(e => e.NomFichier)
                .HasMaxLength(255)
                .HasColumnName("nom_fichier");
            entity.Property(e => e.TypeMime)
                .HasMaxLength(100)
                .HasColumnName("type_mime");
            entity.Property(e => e.Url)
                .HasColumnType("text")
                .HasColumnName("url");

            entity.HasOne(d => d.IdRendezVousNavigation).WithMany(p => p.Fichiers)
                .HasForeignKey(d => d.IdRendezVous)
                .HasConstraintName("fichier_ibfk_1");

            entity.HasOne(d => d.IdUtilisateurNavigation).WithMany(p => p.Fichiers)
                .HasForeignKey(d => d.IdUtilisateur)
                .HasConstraintName("fichier_ibfk_2");
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
            entity.Property(e => e.IdUtili).HasColumnName("idUtili");
            entity.Property(e => e.Note)
                .HasPrecision(2, 1)
                .HasDefaultValueSql("'0.0'")
                .HasColumnName("note");
            entity.Property(e => e.Speciallite)
                .HasMaxLength(100)
                .HasColumnName("speciallite");

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
                .HasColumnType("enum('EN_ATTENTE','ACCEPTE','REFUSE','ANNULE','TERMINE')")
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
            entity.Property(e => e.Duration).HasColumnName("duration");
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
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
