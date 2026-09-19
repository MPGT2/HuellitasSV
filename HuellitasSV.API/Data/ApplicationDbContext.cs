using HuellitasSV.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HuellitasSV.API.Data;

/// <summary>
/// Contexto de base de datos de la aplicación HuellitasSV.
/// Administra el acceso a las entidades mediante Entity Framework Core.
/// </summary>
public class ApplicationDbContext : DbContext
{
    /// <summary>
    /// Inicializa una nueva instancia vacía de <see cref="ApplicationDbContext"/>.
    /// </summary>
    public ApplicationDbContext()
    {
    }

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="ApplicationDbContext"/> con las opciones configuradas.
    /// </summary>
    /// <param name="options">Opciones de configuración del contexto.</param>
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Conjunto de usuarios registrados en el sistema.
    /// </summary>
    public DbSet<Usuario> Usuarios { get; set; } = null!;

    /// <summary>
    /// Conjunto de refugios registrados en el sistema.
    /// </summary>
    public DbSet<Refugio> Refugios { get; set; } = null!;

    /// <summary>
    /// Conjunto de mascotas publicadas para adopción o rescate.
    /// </summary>
    public DbSet<Mascota> Mascotas { get; set; } = null!;

    /// <summary>
    /// Conjunto de necesidades de donación publicadas por los refugios.
    /// </summary>
    public DbSet<NecesidadDonacion> NecesidadesDonacion { get; set; } = null!;

    /// <summary>
    /// Configura el modelo de datos, relaciones y restricciones de precisión para SQL Server.
    /// </summary>
    /// <param name="modelBuilder">Constructor del modelo de Entity Framework.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasIndex(u => u.Correo).IsUnique();
        });

        modelBuilder.Entity<Refugio>(entity =>
        {
            entity.HasOne(r => r.Usuario)
                .WithOne(u => u.Refugio)
                .HasForeignKey<Refugio>(r => r.IdUsuario)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Mascota>(entity =>
        {
            entity.HasOne(m => m.Refugio)
                .WithMany(r => r.Mascotas)
                .HasForeignKey(m => m.IdRefugio)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<NecesidadDonacion>(entity =>
        {
            entity.Property(n => n.CantidadRequerida).HasPrecision(18, 2);
            entity.Property(n => n.CantidadCubierta).HasPrecision(18, 2);

            entity.HasOne(n => n.Refugio)
                .WithMany(r => r.NecesidadesDonacion)
                .HasForeignKey(n => n.IdRefugio)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
