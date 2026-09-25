// [HU-10] Michael Menendez: Estructura base - Contexto de base de datos (Entity Framework Core).
// Configura índices y la relación Mascota -> Refugio con borrado restrictivo.
// [HU-7/8/14/15] Alfredo López: integra las entidades de adopción, notificaciones y reportes de rescate.

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
    public DbSet<Mascota> Mascota { get; set; } = null!;

    /// <summary>Conjunto de refugios (tabla refugio).</summary>
    public DbSet<Refugio> Refugio { get; set; } = null!;

    /// <summary>Conjunto de usuarios registrados (HU-7 y HU-14).</summary>
    public DbSet<Usuario> Usuarios { get; set; } = null!;

    /// <summary>Conjunto de necesidades de donación publicadas por los refugios.</summary>
    public DbSet<NecesidadDonacion> NecesidadesDonacion { get; set; } = null!;

    /// <summary>Conjunto de solicitudes de adopción (HU-7 y HU-8).</summary>
    public DbSet<SolicitudAdopcion> SolicitudesAdopcion { get; set; } = null!;

    /// <summary>Conjunto de notificaciones a refugios y usuarios.</summary>
    public DbSet<Notificacion> Notificaciones { get; set; } = null!;

    /// <summary>Conjunto de reportes de animales callejeros (HU-14 y HU-15).</summary>
    public DbSet<ReporteAnimal> ReportesAnimales { get; set; } = null!;

    /// <summary>
    /// Modelado de índices y relaciones.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ===== Configuración de la estructura base (Michael Menendez) =====
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

        // ===== Entidades de las HU-7, HU-8, HU-14 y HU-15 (Alfredo López) =====
        // Las claves se declaran explícitamente porque usan el formato "IdXxx",
        // que no coincide con la convención automática de EF Core ("Id" o "XxxId").
        modelBuilder.Entity<Usuario>().HasKey(u => u.IdUsuario);
        modelBuilder.Entity<NecesidadDonacion>().HasKey(n => n.IdNecesidad);
        modelBuilder.Entity<SolicitudAdopcion>().HasKey(s => s.IdSolicitud);
        modelBuilder.Entity<Notificacion>().HasKey(n => n.IdNotificacion);
        modelBuilder.Entity<ReporteAnimal>().HasKey(r => r.IdReporte);

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasIndex(u => u.Correo).IsUnique();
        });

        modelBuilder.Entity<NecesidadDonacion>(entity =>
        {
            entity.Property(n => n.CantidadRequerida).HasPrecision(18, 2);
            entity.Property(n => n.CantidadCubierta).HasPrecision(18, 2);

            entity.HasOne(n => n.Refugio)
                .WithMany()
                .HasForeignKey(n => n.IdRefugio)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SolicitudAdopcion>(entity =>
        {
            // El estado se almacena como texto ("Pendiente", "Aprobada", "Rechazada").
            entity.Property(s => s.Estado)
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.HasOne(s => s.Mascota)
                .WithMany()
                .HasForeignKey(s => s.IdMascota)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(s => s.Usuario)
                .WithMany()
                .HasForeignKey(s => s.IdUsuario)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Notificacion>(entity =>
        {
            entity.Property(n => n.Mensaje).HasMaxLength(500);

            entity.HasOne(n => n.Refugio)
                .WithMany()
                .HasForeignKey(n => n.IdRefugio)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(n => n.Usuario)
                .WithMany()
                .HasForeignKey(n => n.IdUsuario)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ReporteAnimal>(entity =>
        {
            // El estado se almacena como texto ("Pendiente", "Atendido").
            entity.Property(r => r.Estado)
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.HasOne(r => r.Usuario)
                .WithMany()
                .HasForeignKey(r => r.IdUsuario)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(r => r.Refugio)
                .WithMany()
                .HasForeignKey(r => r.IdRefugio)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
