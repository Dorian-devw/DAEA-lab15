using System;
using System.Collections.Generic;
using _13Serpentarios.Entidades.Models;
using Microsoft.EntityFrameworkCore;

namespace _13Serpentarios.Datos.Models;

public partial class SerpentarioContext : DbContext
{
    public SerpentarioContext(DbContextOptions<SerpentarioContext> options)
        : base(options)
    {
    }

    public virtual DbSet<EspeciesSerpiente> EspeciesSerpientes { get; set; }

    public virtual DbSet<LotesVeneno> LotesVenenos { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EspeciesSerpiente>(entity =>
        {
            entity.HasKey(e => e.EspecieId);

            entity.ToTable("EspeciesSerpiente");

            entity.HasIndex(e => e.NombreCientifico, "UQ_EspeciesSerpiente_NombreCientifico").IsUnique();

            entity.Property(e => e.Activa).HasDefaultValue(true, "DF_EspeciesSerpiente_Activa");
            entity.Property(e => e.Familia).HasMaxLength(60);
            entity.Property(e => e.FechaRegistro)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())", "DF_EspeciesSerpiente_FechaRegistro");
            entity.Property(e => e.NombreCientifico).HasMaxLength(120);
            entity.Property(e => e.NombreComun).HasMaxLength(100);
            entity.Property(e => e.RegionOrigen).HasMaxLength(120);
            entity.Property(e => e.TipoVenenoDominante).HasMaxLength(60);
        });

        modelBuilder.Entity<LotesVeneno>(entity =>
        {
            entity.HasKey(e => e.LoteId);

            entity.ToTable("LotesVeneno");

            entity.HasIndex(e => e.CodigoLote, "UQ_LotesVeneno_CodigoLote").IsUnique();

            entity.Property(e => e.CantidadMiligramos).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.CodigoLote).HasMaxLength(30);
            entity.Property(e => e.EstadoCalidad).HasMaxLength(30);
            entity.Property(e => e.Observaciones).HasMaxLength(300);
            entity.Property(e => e.Presentacion).HasMaxLength(40);
            entity.Property(e => e.PurezaPorcentaje).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.TemperaturaAlmacenamientoC).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.UsoDestino).HasMaxLength(120);

            entity.HasOne(d => d.Especie).WithMany(p => p.LotesVenenos)
                .HasForeignKey(d => d.EspecieId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LotesVeneno_EspeciesSerpiente");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.UsuarioId);

            entity.ToTable("Usuarios");

            entity.HasIndex(e => e.Correo, "UQ_Usuarios_Correo").IsUnique();

            entity.Property(e => e.Correo).HasMaxLength(150);
            entity.Property(e => e.ClaveHash).HasMaxLength(255);
            entity.Property(e => e.Rol).HasMaxLength(50);
            entity.Property(e => e.FechaRegistro)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())", "DF_Usuarios_FechaRegistro");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
