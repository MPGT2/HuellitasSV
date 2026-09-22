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
    /// Conjunto de solicitudes de adopción enviadas por los usuarios.
    /// </summary>
    public DbSet<SolicitudAdopcion> SolicitudesAdopcion { get; set; } = null!;

    /// <summary>
    /// Conjunto de notificaciones enviadas a refugios y usuarios.
    /// </summary>
    public DbSet<Notificacion> Notificaciones { get; set; } = null!;

    /// <summary>
    /// Conjunto de reportes de animales callejeros enviados por los usuarios.
    /// </summary>
    public DbSet<ReporteAnimal> ReportesAnimales { get; set; } = null!;

    /// <summary>
    /// Configura el modelo de datos, relaciones y restricciones de precisión para SQL Server.
    /// </summary>
    /// <param name="modelBuilder">Constructor del modelo de Entity Framework.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Las claves se declaran explícitamente porque nuestras propiedades usan el formato
        // "IdXxx", que no coincide con la convención automática de EF Core ("Id" o "XxxId").
        modelBuilder.Entity<Usuario>().HasKey(u => u.IdUsuario);
        modelBuilder.Entity<Refugio>().HasKey(r => r.IdRefugio);
        modelBuilder.Entity<Mascota>().HasKey(m => m.IdMascota);
        modelBuilder.Entity<NecesidadDonacion>().HasKey(n => n.IdNecesidad);
        modelBuilder.Entity<SolicitudAdopcion>().HasKey(s => s.IdSolicitud);
        modelBuilder.Entity<Notificacion>().HasKey(n => n.IdNotificacion);
        modelBuilder.Entity<ReporteAnimal>().HasKey(r => r.IdReporte);

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
            // El estado se almacena como texto ("Disponible", "EnProcesoAdopcion", "Adoptada").
            entity.Property(m => m.Estado)
                .HasConversion<string>()
                .HasMaxLength(30);

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
