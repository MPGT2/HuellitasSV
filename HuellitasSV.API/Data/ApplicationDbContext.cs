// [HU-10] Michael Menendez: Estructura base - Contexto de base de datos (Entity Framework Core).
// Configura índices y la relación Mascota -> Refugio con borrado restrictivo.
// [HU-7/8/14/15] Alfredo López: integra las entidades de adopción, notificaciones y reportes de rescate.
// [HU-01/02] Michael Menendez: integra Cuenta y el perfil Usuario vinculado a una cuenta.
// [HU-13] Oscar Ramírez: integra Anuncio (espacios publicitarios).

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

    /// <summary>Conjunto de cuentas de acceso (HU-01 y HU-02).</summary>
    public DbSet<Cuenta> Cuenta { get; set; } = null!;

    /// <summary>Conjunto de anuncios publicitarios (HU-13).</summary>
    public DbSet<Anuncio> Anuncios { get; set; } = null!;

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

        // ===== Entidades de las HU-1, HU-2, HU-7, HU-8, HU-13, HU-14 y HU-15 =====
        // Las claves se declaran explícitamente porque usan el formato "IdXxx",
        // que no coincide con la convención automática de EF Core ("Id" o "XxxId").
        modelBuilder.Entity<Cuenta>().HasKey(c => c.IdCuenta);
        modelBuilder.Entity<Usuario>().HasKey(u => u.IdUsuario);
        modelBuilder.Entity<NecesidadDonacion>().HasKey(n => n.IdNecesidad);
        modelBuilder.Entity<SolicitudAdopcion>().HasKey(s => s.IdSolicitud);
        modelBuilder.Entity<Notificacion>().HasKey(n => n.IdNotificacion);
        modelBuilder.Entity<ReporteAnimal>().HasKey(r => r.IdReporte);
        modelBuilder.Entity<Anuncio>().HasKey(a => a.IdAnuncio);

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasIndex(u => u.IdCuenta);
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

        // ===== Datos semilla =====
        // Contraseñas con hash PBKDF2 (ASP.NET Core Identity v3).
        // Admin: admin@huellitassv.org / Admin2026!
        modelBuilder.Entity<Cuenta>().HasData(
            new Cuenta { IdCuenta = 1, Correo = "admin@huellitassv.org", Contrasena = "AQAAAAIAAYagAAAAECJkUTNTpDEXyGK0S6lDi/pF5uh9pDEYK2H/U3+h/ciWOS/vclz8lBWHa+9Vy2kCAg==", Rol = "Admin", Estado = "aprobado" },
            new Cuenta { IdCuenta = 1001, Correo = "contacto@huellitassv.org", Contrasena = "AQAAAAIAAYagAAAAEGpRavkRHJ/aY735e72svpCHsB4bya3yjaaoTgFIkSImDf/aT31gaWTTjv2yG1cSQg==", Rol = "Refugio", Estado = "aprobado" },
            new Cuenta { IdCuenta = 1002, Correo = "adopciones@proteccionsv.org", Contrasena = "AQAAAAIAAYagAAAAEGpRavkRHJ/aY735e72svpCHsB4bya3yjaaoTgFIkSImDf/aT31gaWTTjv2yG1cSQg==", Rol = "Refugio", Estado = "aprobado" },
            new Cuenta { IdCuenta = 1003, Correo = "info@alberguesm.org", Contrasena = "AQAAAAIAAYagAAAAEGpRavkRHJ/aY735e72svpCHsB4bya3yjaaoTgFIkSImDf/aT31gaWTTjv2yG1cSQg==", Rol = "Refugio", Estado = "pendiente" },
            new Cuenta { IdCuenta = 9001, Correo = "losamigos.refugio@correo.com", Contrasena = "AQAAAAIAAYagAAAAEGpRavkRHJ/aY735e72svpCHsB4bya3yjaaoTgFIkSImDf/aT31gaWTTjv2yG1cSQg==", Rol = "Refugio", Estado = "aprobado" },
            new Cuenta { IdCuenta = 9002, Correo = "hogarsantaana@correo.com", Contrasena = "AQAAAAIAAYagAAAAEGpRavkRHJ/aY735e72svpCHsB4bya3yjaaoTgFIkSImDf/aT31gaWTTjv2yG1cSQg==", Rol = "Refugio", Estado = "aprobado" },
            new Cuenta { IdCuenta = 9003, Correo = "vidasonsonate@correo.com", Contrasena = "AQAAAAIAAYagAAAAEGpRavkRHJ/aY735e72svpCHsB4bya3yjaaoTgFIkSImDf/aT31gaWTTjv2yG1cSQg==", Rol = "Refugio", Estado = "pendiente" },
            new Cuenta { IdCuenta = 9004, Correo = "patitasusulutan@correo.com", Contrasena = "AQAAAAIAAYagAAAAEGpRavkRHJ/aY735e72svpCHsB4bya3yjaaoTgFIkSImDf/aT31gaWTTjv2yG1cSQg==", Rol = "Refugio", Estado = "pendiente" },
            new Cuenta { IdCuenta = 9005, Correo = "ayudaanimalchalate@correo.com", Contrasena = "AQAAAAIAAYagAAAAEGpRavkRHJ/aY735e72svpCHsB4bya3yjaaoTgFIkSImDf/aT31gaWTTjv2yG1cSQg==", Rol = "Refugio", Estado = "rechazado" },
            new Cuenta { IdCuenta = 9006, Correo = "marialopez@correo.com", Contrasena = "AQAAAAIAAYagAAAAEEwuG7WIzT50BNMbcOjCcrOXbVZYK+aPbwJW8gVrFg1VQ5QqPmopibuNtnP7+aCn5A==", Rol = "Usuario", Estado = "activo" },
            new Cuenta { IdCuenta = 9007, Correo = "carlosperez@correo.com", Contrasena = "AQAAAAIAAYagAAAAEEwuG7WIzT50BNMbcOjCcrOXbVZYK+aPbwJW8gVrFg1VQ5QqPmopibuNtnP7+aCn5A==", Rol = "Usuario", Estado = "inactivo" },
            new Cuenta { IdCuenta = 9008, Correo = "anagomez@correo.com", Contrasena = "AQAAAAIAAYagAAAAEEwuG7WIzT50BNMbcOjCcrOXbVZYK+aPbwJW8gVrFg1VQ5QqPmopibuNtnP7+aCn5A==", Rol = "Usuario", Estado = "bloqueado" });

        modelBuilder.Entity<Usuario>().HasData(
            new Usuario { IdUsuario = 9001, IdCuenta = 9006, Nombre = "María López" },
            new Usuario { IdUsuario = 9002, IdCuenta = 9007, Nombre = "Carlos Pérez" },
            new Usuario { IdUsuario = 9003, IdCuenta = 9008, Nombre = "Ana Gómez" });

        modelBuilder.Entity<Refugio>().HasData(
            new Refugio { IdRefugio = 1, IdCuenta = 1001, NombreOrganizacion = "Refugio Huellitas San Salvador", Departamento = "San Salvador", Municipio = "San Salvador", Contacto = "contacto@huellitassv.org", DocumentacionUrl = "https://huellitassv.org/docs", EstadoAprobacion = "aprobado" },
            new Refugio { IdRefugio = 2, IdCuenta = 1002, NombreOrganizacion = "Protección Animal Santa Tecla", Departamento = "La Libertad", Municipio = "Santa Tecla", Contacto = "adopciones@proteccionsv.org", EstadoAprobacion = "aprobado" },
            new Refugio { IdRefugio = 3, IdCuenta = 1003, NombreOrganizacion = "Albergue Canino San Miguel", Departamento = "San Miguel", Municipio = "San Miguel", Contacto = "info@alberguesm.org", DocumentacionUrl = "https://alberguesm.org/documentos", EstadoAprobacion = "pendiente" },
            new Refugio { IdRefugio = 9001, IdCuenta = 9001, NombreOrganizacion = "Refugio Los Amigos", Departamento = "San Salvador", Municipio = "San Salvador", Contacto = "7770-0001", DocumentacionUrl = "https://losamigos.org/docs", EstadoAprobacion = "aprobado" },
            new Refugio { IdRefugio = 9002, IdCuenta = 9002, NombreOrganizacion = "Hogar Animal Santa Ana", Departamento = "Santa Ana", Municipio = "Santa Ana", Contacto = "7770-0002", EstadoAprobacion = "aprobado" },
            new Refugio { IdRefugio = 9003, IdCuenta = 9003, NombreOrganizacion = "Vida Animal Sonsonate", Departamento = "Sonsonate", Municipio = "Sonsonate", Contacto = "7770-0003", EstadoAprobacion = "pendiente" },
            new Refugio { IdRefugio = 9004, IdCuenta = 9004, NombreOrganizacion = "Patitas de Usulután", Departamento = "Usulután", Municipio = "Usulután", Contacto = "7770-0004", DocumentacionUrl = "https://patitasusulutan.org/docs", EstadoAprobacion = "pendiente" },
            new Refugio { IdRefugio = 9005, IdCuenta = 9005, NombreOrganizacion = "Ayuda Animal Chalatenango", Departamento = "Chalatenango", Municipio = "Chalatenango", Contacto = "7770-0005", EstadoAprobacion = "rechazado" });

        modelBuilder.Entity<Mascota>().HasData(
            new Mascota { IdMascota = 1, IdRefugio = 1, Nombre = "Firulais", Especie = "perro", Tamano = "mediano", EdadMeses = 24, EstadoSalud = "sano", Estado = "disponible", FechaRegistro = new DateTime(2026, 1, 15, 10, 0, 0, DateTimeKind.Utc) },
            new Mascota { IdMascota = 2, IdRefugio = 1, Nombre = "Michi", Especie = "gato", Tamano = "pequeño", EdadMeses = 12, EstadoSalud = "sano", Estado = "disponible", FechaRegistro = new DateTime(2026, 2, 20, 10, 0, 0, DateTimeKind.Utc) },
            new Mascota { IdMascota = 3, IdRefugio = 1, Nombre = "Rex", Especie = "perro", Tamano = "grande", EdadMeses = 36, EstadoSalud = "en_tratamiento", Estado = "disponible", FechaRegistro = new DateTime(2026, 3, 10, 10, 0, 0, DateTimeKind.Utc) },
            new Mascota { IdMascota = 4, IdRefugio = 2, Nombre = "Luna", Especie = "perro", Tamano = "mediano", EdadMeses = 18, EstadoSalud = "sano", Estado = "adoptada", FechaRegistro = new DateTime(2026, 1, 5, 10, 0, 0, DateTimeKind.Utc) },
            new Mascota { IdMascota = 5, IdRefugio = 2, Nombre = "Simba", Especie = "gato", Tamano = "pequeño", EdadMeses = 6, EstadoSalud = "sano", Estado = "disponible", FechaRegistro = new DateTime(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc) },
            new Mascota { IdMascota = 6, IdRefugio = 3, Nombre = "Rocky", Especie = "perro", Tamano = "grande", EdadMeses = 48, EstadoSalud = "crónico", Estado = "en_tratamiento", FechaRegistro = new DateTime(2026, 2, 28, 10, 0, 0, DateTimeKind.Utc) },
            new Mascota { IdMascota = 9001, IdRefugio = 1, Nombre = "Rocky II", Especie = "perro", Tamano = "grande", EdadMeses = 48, EstadoSalud = "sano", Estado = "disponible", FechaRegistro = new DateTime(2026, 5, 1, 10, 0, 0, DateTimeKind.Utc) },
            new Mascota { IdMascota = 9002, IdRefugio = 1, Nombre = "Mia", Especie = "gato", Tamano = "pequeño", EdadMeses = 8, EstadoSalud = "sano", Estado = "disponible", FechaRegistro = new DateTime(2026, 5, 20, 10, 0, 0, DateTimeKind.Utc) },
            new Mascota { IdMascota = 9003, IdRefugio = 1, Nombre = "Bobby", Especie = "perro", Tamano = "mediano", EdadMeses = 30, EstadoSalud = "en_tratamiento", Estado = "reservada", FechaRegistro = new DateTime(2026, 6, 10, 10, 0, 0, DateTimeKind.Utc) },
            new Mascota { IdMascota = 9004, IdRefugio = 2, Nombre = "Coco", Especie = "gato", Tamano = "mediano", EdadMeses = 18, EstadoSalud = "crónico", Estado = "disponible", FechaRegistro = new DateTime(2026, 6, 25, 10, 0, 0, DateTimeKind.Utc) },
            new Mascota { IdMascota = 9005, IdRefugio = 2, Nombre = "Thor", Especie = "perro", Tamano = "grande", EdadMeses = 60, EstadoSalud = "sano", Estado = "adoptada", FechaRegistro = new DateTime(2026, 7, 5, 10, 0, 0, DateTimeKind.Utc) },
            new Mascota { IdMascota = 9006, IdRefugio = 9001, Nombre = "Nina", Especie = "gato", Tamano = "pequeño", EdadMeses = 4, EstadoSalud = "sano", Estado = "disponible", FechaRegistro = new DateTime(2026, 7, 20, 10, 0, 0, DateTimeKind.Utc), ImagenUrl = "https://placekitten.com/400/300" },
            new Mascota { IdMascota = 9007, IdRefugio = 9001, Nombre = "Simón", Especie = "otro", Tamano = "pequeño", EdadMeses = 10, EstadoSalud = "discapacidad", Estado = "disponible", FechaRegistro = new DateTime(2026, 8, 2, 10, 0, 0, DateTimeKind.Utc) },
            new Mascota { IdMascota = 9008, IdRefugio = 9002, Nombre = "Duke", Especie = "perro", Tamano = "grande", EdadMeses = 24, EstadoSalud = "sano", Estado = "disponible", FechaRegistro = new DateTime(2026, 8, 15, 10, 0, 0, DateTimeKind.Utc), ImagenUrl = "https://placedog.net/500/400" },
            new Mascota { IdMascota = 9009, IdRefugio = 9002, Nombre = "Pelusa", Especie = "gato", Tamano = "mediano", EdadMeses = 14, EstadoSalud = "sano", Estado = "en_tratamiento", FechaRegistro = new DateTime(2026, 8, 28, 10, 0, 0, DateTimeKind.Utc) },
            new Mascota { IdMascota = 9010, IdRefugio = 9002, Nombre = "Zeus", Especie = "perro", Tamano = "mediano", EdadMeses = 36, EstadoSalud = "sano", Estado = "fallecida", FechaRegistro = new DateTime(2026, 9, 10, 10, 0, 0, DateTimeKind.Utc) });
    }
}