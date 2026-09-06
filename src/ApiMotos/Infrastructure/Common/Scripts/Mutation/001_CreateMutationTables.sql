-- =============================================
-- Mutation Module - Database Schema
-- ZAS Code Generation System
-- =============================================

-- Tabla principal de mutaciones
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'RT_Mutacion')
BEGIN
    CREATE TABLE RT_Mutacion (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        SolicitudOriginal NVARCHAR(2000) NOT NULL,
        ResumenTecnico NVARCHAR(MAX) NULL,
        Estado INT NOT NULL DEFAULT 0,  -- 0=Pendiente, 1=Analizada, 2=Previsualizada, 3=Ejecutada, 4=Fallida, 5=Revertida
        FechaSolicitud DATETIME2 NOT NULL,
        FechaAnalisis DATETIME2 NULL,
        FechaEjecucion DATETIME2 NULL,
        UsuarioId INT NOT NULL,
        UsuarioNombre NVARCHAR(200) NULL,
        PreviewHtml NVARCHAR(MAX) NULL,
        ArchitectureCompliance INT NULL,
        RiskAnalysis NVARCHAR(MAX) NULL,
        ErrorMessage NVARCHAR(MAX) NULL,
        ContextoJson NVARCHAR(MAX) NULL,
        RespuestaClaudeJson NVARCHAR(MAX) NULL
    );

    -- Índices
    CREATE INDEX IX_Mutacion_Estado ON RT_Mutacion(Estado);
    CREATE INDEX IX_Mutacion_UsuarioId ON RT_Mutacion(UsuarioId);
    CREATE INDEX IX_Mutacion_FechaSolicitud ON RT_Mutacion(FechaSolicitud);

    PRINT 'Tabla RT_Mutacion creada exitosamente';
END
ELSE
BEGIN
    PRINT 'Tabla RT_Mutacion ya existe';
END
GO

-- Tabla de impactos estructurales
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'RT_MutacionImpacto')
BEGIN
    CREATE TABLE RT_MutacionImpacto (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        MutacionId INT NOT NULL,
        Capa INT NOT NULL,  -- 0=Domain, 1=Application, 2=Infrastructure, 3=Presentation, 4=Database
        Tipo INT NOT NULL,  -- 0=Crear, 1=Modificar, 2=Eliminar
        Descripcion NVARCHAR(500) NOT NULL,
        RutaArchivo NVARCHAR(500) NULL,
        CodigoGenerado NVARCHAR(MAX) NULL,
        Lenguaje NVARCHAR(50) NULL,
        Orden INT NOT NULL DEFAULT 0,

        CONSTRAINT FK_MutacionImpacto_Mutacion FOREIGN KEY (MutacionId)
            REFERENCES RT_Mutacion(Id) ON DELETE CASCADE
    );

    -- Índices
    CREATE INDEX IX_MutacionImpacto_MutacionId ON RT_MutacionImpacto(MutacionId);

    PRINT 'Tabla RT_MutacionImpacto creada exitosamente';
END
ELSE
BEGIN
    PRINT 'Tabla RT_MutacionImpacto ya existe';
END
GO

-- Tabla de archivos afectados (para rollback)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'RT_MutacionArchivo')
BEGIN
    CREATE TABLE RT_MutacionArchivo (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        MutacionId INT NOT NULL,
        RutaCompleta NVARCHAR(1000) NOT NULL,
        RutaRelativa NVARCHAR(500) NOT NULL,
        Operacion INT NOT NULL,  -- 0=Crear, 1=Modificar, 2=Eliminar
        ContenidoOriginal NVARCHAR(MAX) NULL,
        ContenidoNuevo NVARCHAR(MAX) NULL,
        HashContenido NVARCHAR(100) NULL,
        FechaOperacion DATETIME2 NOT NULL,
        Restaurado BIT NOT NULL DEFAULT 0,
        FechaRestauracion DATETIME2 NULL,

        CONSTRAINT FK_MutacionArchivo_Mutacion FOREIGN KEY (MutacionId)
            REFERENCES RT_Mutacion(Id) ON DELETE CASCADE
    );

    -- Índices
    CREATE INDEX IX_MutacionArchivo_MutacionId ON RT_MutacionArchivo(MutacionId);

    PRINT 'Tabla RT_MutacionArchivo creada exitosamente';
END
ELSE
BEGIN
    PRINT 'Tabla RT_MutacionArchivo ya existe';
END
GO

PRINT 'Script de Mutation completado';
