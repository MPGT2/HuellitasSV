// [HU-10] Michael Menendez: Estructura base - Contexto de base de datos (Entity Framework Core).
// Configura índices y la relación Mascota -> Refugio con borrado restrictivo.

namespace HuellitasSV.API.Data;

using Microsoft.EntityFrameworkCore;
using HuellitasSV.API.Models;

/// <summary>
/// Contexto de base de datos de HuellitasSV.
/// </summary>
public class ApplicationDbContext : DbContext
{
    /// <summary>
    /// Inicializa el contexto con las opciones de conexión.
    /// </summary>
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    /// <summary>Conjunto de mascotas (tabla mascota).</summary>
    public DbSet<Mascota> Mascota { get; set; }

    /// <summary>Conjunto de refugios (tabla refugio).</summary>
    public DbSet<Refugio> Refugio { get; set; }

    /// <summary>
    /// Modelado de índices y relaciones.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Mascota>(entity =>
        {
            // Índices para las consultas más frecuentes: estado y refugio.
            entity.HasIndex(m => m.Estado);
            entity.HasIndex(m => m.IdRefugio);

            // Una mascota pertenece a un refugio; no se permite borrar un refugio con mascotas.
            entity.HasOne(m => m.Refugio)
                .WithMany()
                .HasForeignKey(m => m.IdRefugio)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Refugio>(entity =>
        {
            // Facilita el filtrado por estado de aprobación.
            entity.HasIndex(r => r.EstadoAprobacion);
        });
    }
}