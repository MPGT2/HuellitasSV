-- HU-10: calificacion por estrellas de los refugios.
--
-- Por que este script existe: la tabla calificacion se agrego al modelo de EF
-- (ApplicationDbContext.Calificacion) pero nunca llego a la base de datos, porque
-- las unicas migraciones aplicadas son Inicial, AgregaCuentaAdminYSeguridad y
-- AgregarUbicacionAnuncio. Sin esta tabla, GET /api/Mascotas responde 500 con
-- "Invalid object name 'calificacion'" al calcular el promedio de estrellas.
--
-- El script es idempotente: se puede ejecutar mas de una vez sin error.
-- Si prefieres gestionarlo con migraciones de EF, NO lo ejecutes: crea el
-- esquema con `dotnet ef migrations add AgregaCalificacion` en su lugar.

IF OBJECT_ID(N'dbo.calificacion', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.calificacion (
        id_calificacion BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_calificacion PRIMARY KEY,
        id_refugio      BIGINT        NOT NULL,
        id_usuario      BIGINT        NOT NULL,
        estrellas       INT           NOT NULL,
        comentario      NVARCHAR(500) NULL,
        fecha           DATETIME2     NOT NULL,
        CONSTRAINT CK_calificacion_estrellas CHECK (estrellas BETWEEN 1 AND 5),
        CONSTRAINT FK_calificacion_refugio FOREIGN KEY (id_refugio) REFERENCES dbo.refugio (id_refugio) ON DELETE CASCADE,
        CONSTRAINT FK_calificacion_usuario FOREIGN KEY (id_usuario) REFERENCES dbo.usuario (id_usuario)
    );

    CREATE UNIQUE INDEX IX_calificacion_id_refugio_id_usuario
        ON dbo.calificacion (id_refugio, id_usuario);
END
GO

-- Datos de prueba. El refugio 9002 (Hogar Animal Santa Ana) queda sin
-- calificaciones a proposito, para poder probar el mensaje "sin calificaciones".
IF NOT EXISTS (SELECT 1 FROM dbo.calificacion)
BEGIN
    INSERT INTO dbo.calificacion (id_refugio, id_usuario, estrellas, comentario, fecha) VALUES
        (1,    9001, 5, N'Excelente atención',       '2026-08-01T10:00:00'),
        (1,    9002, 4, N'Muy buen seguimiento',     '2026-08-05T10:00:00'),
        (1,    9003, 5, NULL,                        '2026-08-10T10:00:00'),
        (2,    9001, 4, N'Proceso claro',            '2026-08-12T10:00:00'),
        (9001, 9001, 3, NULL,                        '2026-08-15T10:00:00'),
        (9001, 9002, 4, N'Buen trato a los animales', '2026-08-20T10:00:00');
END
GO
