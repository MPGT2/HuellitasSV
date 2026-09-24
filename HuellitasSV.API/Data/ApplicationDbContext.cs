// [HU-10] Michael Menendez: Estructura base - Contexto de base de datos (Entity Framework Core).
// Configura Ã­ndices y la relaciÃ³n Mascota -> Refugio con borrado restrictivo.

namespace HuellitasSV.API.Data;

using Microsoft.EntityFrameworkCore;
using HuellitasSV.API.Models;

/// <summary>
/// Contexto de base de datos de HuellitasSV.
/// </summary>
public class ApplicationDbContext : DbContext
{
    /// <summary>
    /// Inicializa el contexto con las opciones de conexiÃ³n.
    /// </summary>
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    /// <summary>Conjunto de mascotas (tabla mascota).</summary>
    public DbSet<Mascota> Mascota { get; set; }

    /// <summary>Conjunto de refugios (tabla refugio).</summary>
    public DbSet<Refugio> Refugio { get; set; }

    /// <summary>Conjunto de cuentas de acceso (tabla cuenta).</summary>
    public DbSet<Cuenta> Cuenta { get; set; }

    /// <summary>Conjunto de perfiles de usuarios clientes (tabla usuario).</summary>
    public DbSet<Usuario> Usuario { get; set; }

    /// <summary>
    /// Modelado de Ã­ndices, relaciones y datos semilla.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Mascota>(entity =>
        {
            // Ãndices para las consultas mÃ¡s frecuentes: estado y refugio.
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
            // Facilita el filtrado por estado de aprobaciÃ³n.
            entity.HasIndex(r => r.EstadoAprobacion);
        });

        modelBuilder.Entity<Cuenta>(entity =>
        {
            // El correo identifica de forma Ãºnica a cada cuenta (HU-02).
            entity.HasIndex(c => c.Correo).IsUnique();
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            // Facilita la bÃºsqueda del perfil a partir de la cuenta (inicio de sesiÃ³n HU-01).
            entity.HasIndex(u => u.IdCuenta);
        });

// Cuentas semilla alineadas con los refugios semilla (id_cuenta 1001-1003)
        // y datos de prueba para validar el flujo completo de HU-02 y HU-24
        // (aprobado, pendiente y rechazado) y de HU-01 (activo, inactivo y bloqueado).
        // Las contraseÃ±as se almacenan con hash PBKDF2 (no en texto plano).
        modelBuilder.Entity<Cuenta>().HasData(
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
            new Cuenta { IdCuenta = 9008, Correo = "anagomez@correo.com", Contrasena = "AQAAAAIAAYagAAAAEEwuG7WIzT50BNMbcOjCcrOXbVZYK+aPbwJW8gVrFg1VQ5QqPmopibuNtnP7+aCn5A==", Rol = "Usuario", Estado = "bloqueado" }
        );

        // Perfiles de usuario semilla alineados con las cuentas 9006-9008 para probar
        // los tres escenarios de inicio de sesiÃ³n de la HU-01 (activo, inactivo y bloqueado).
        modelBuilder.Entity<Usuario>().HasData(
            new Usuario { IdUsuario = 9001, IdCuenta = 9006, Nombre = "MarÃ­a LÃ³pez" },
            new Usuario { IdUsuario = 9002, IdCuenta = 9007, Nombre = "Carlos PÃ©rez" },
            new Usuario { IdUsuario = 9003, IdCuenta = 9008, Nombre = "Ana GÃ³mez" }
        );

        // Datos de prueba de refugios para HU-24: cubren los tres estados de aprobaciÃ³n
        // y varios departamentos/municipios para el filtro de ubicaciÃ³n de la HU-06.
        modelBuilder.Entity<Refugio>().HasData(
            new Refugio { IdRefugio = 9001, IdCuenta = 9001, NombreOrganizacion = "Refugio Los Amigos", Departamento = "San Salvador", Municipio = "San Salvador", Contacto = "7770-0001", DocumentacionUrl = "https://losamigos.org/docs", EstadoAprobacion = "aprobado" },
            new Refugio { IdRefugio = 9002, IdCuenta = 9002, NombreOrganizacion = "Hogar Animal Santa Ana", Departamento = "Santa Ana", Municipio = "Santa Ana", Contacto = "7770-0002", DocumentacionUrl = null, EstadoAprobacion = "aprobado" },
            new Refugio { IdRefugio = 9003, IdCuenta = 9003, NombreOrganizacion = "Vida Animal Sonsonate", Departamento = "Sonsonate", Municipio = "Sonsonate", Contacto = "7770-0003", DocumentacionUrl = null, EstadoAprobacion = "pendiente" },
            new Refugio { IdRefugio = 9004, IdCuenta = 9004, NombreOrganizacion = "Patitas de UsulutÃ¡n", Departamento = "UsulutÃ¡n", Municipio = "UsulutÃ¡n", Contacto = "7770-0004", DocumentacionUrl = "https://patitasusulutan.org/docs", EstadoAprobacion = "pendiente" },
            new Refugio { IdRefugio = 9005, IdCuenta = 9005, NombreOrganizacion = "Ayuda Animal Chalatenango", Departamento = "Chalatenango", Municipio = "Chalatenango", Contacto = "7770-0005", DocumentacionUrl = null, EstadoAprobacion = "rechazado" }
        );

        // Datos de prueba de mascotas para HU-04, HU-05, HU-06 y HU-09: cubren las tres
        // especies, los tres tamaÃ±os, los cuatro estados de salud, los cinco estados de
        // mascota, un rango amplio de edades y algunas imÃ¡genes externas (ImagenUrl).
        modelBuilder.Entity<Mascota>().HasData(
            new Mascota { IdMascota = 9001, IdRefugio = 1, Nombre = "Rocky II", Especie = "perro", Tamano = "grande", EdadMeses = 48, EstadoSalud = "sano", Estado = "disponible", FechaRegistro = new DateTime(2026, 5, 1, 10, 0, 0, DateTimeKind.Utc) },
            new Mascota { IdMascota = 9002, IdRefugio = 1, Nombre = "Mia", Especie = "gato", Tamano = "pequeÃ±o", EdadMeses = 8, EstadoSalud = "sano", Estado = "disponible", FechaRegistro = new DateTime(2026, 5, 20, 10, 0, 0, DateTimeKind.Utc) },
            new Mascota { IdMascota = 9003, IdRefugio = 1, Nombre = "Bobby", Especie = "perro", Tamano = "mediano", EdadMeses = 30, EstadoSalud = "en_tratamiento", Estado = "reservada", FechaRegistro = new DateTime(2026, 6, 10, 10, 0, 0, DateTimeKind.Utc) },
            new Mascota { IdMascota = 9004, IdRefugio = 2, Nombre = "Coco", Especie = "gato", Tamano = "mediano", EdadMeses = 18, EstadoSalud = "crÃ³nico", Estado = "disponible", FechaRegistro = new DateTime(2026, 6, 25, 10, 0, 0, DateTimeKind.Utc) },
            new Mascota { IdMascota = 9005, IdRefugio = 2, Nombre = "Thor", Especie = "perro", Tamano = "grande", EdadMeses = 60, EstadoSalud = "sano", Estado = "adoptada", FechaRegistro = new DateTime(2026, 7, 5, 10, 0, 0, DateTimeKind.Utc) },
            new Mascota { IdMascota = 9006, IdRefugio = 9001, Nombre = "Nina", Especie = "gato", Tamano = "pequeÃ±o", EdadMeses = 4, EstadoSalud = "sano", Estado = "disponible", FechaRegistro = new DateTime(2026, 7, 20, 10, 0, 0, DateTimeKind.Utc), ImagenUrl = "https://placekitten.com/400/300" },
            new Mascota { IdMascota = 9007, IdRefugio = 9001, Nombre = "SimÃ³n", Especie = "otro", Tamano = "pequeÃ±o", EdadMeses = 10, EstadoSalud = "discapacidad", Estado = "disponible", FechaRegistro = new DateTime(2026, 8, 2, 10, 0, 0, DateTimeKind.Utc) },
            new Mascota { IdMascota = 9008, IdRefugio = 9002, Nombre = "Duke", Especie = "perro", Tamano = "grande", EdadMeses = 24, EstadoSalud = "sano", Estado = "disponible", FechaRegistro = new DateTime(2026, 8, 15, 10, 0, 0, DateTimeKind.Utc), ImagenUrl = "https://placedog.net/500/400" },
            new Mascota { IdMascota = 9009, IdRefugio = 9002, Nombre = "Pelusa", Especie = "gato", Tamano = "mediano", EdadMeses = 14, EstadoSalud = "sano", Estado = "en_tratamiento", FechaRegistro = new DateTime(2026, 8, 28, 10, 0, 0, DateTimeKind.Utc) },
            new Mascota { IdMascota = 9010, IdRefugio = 9002, Nombre = "Zeus", Especie = "perro", Tamano = "mediano", EdadMeses = 36, EstadoSalud = "sano", Estado = "fallecida", FechaRegistro = new DateTime(2026, 9, 10, 10, 0, 0, DateTimeKind.Utc) }
        );
    }
}
