-- ═══════════════════════════════════════════════════════════════════════════════
-- SCRIPT DE CREACIÓN: Tablas de Reportes
-- RT_ReporteProgramado: Reportes programados para ejecución automática
-- RT_ReporteHistorial: Historial de reportes generados
-- ═══════════════════════════════════════════════════════════════════════════════

-- ═══════════════════════════════════════════════════════════════════════════════
-- TABLA: RT_ReporteProgramado
-- ═══════════════════════════════════════════════════════════════════════════════

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RT_ReporteProgramado]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[RT_ReporteProgramado] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [Nombre] NVARCHAR(200) NOT NULL,
        [TipoReporte] NVARCHAR(50) NOT NULL,
        [Entidad] NVARCHAR(100) NULL,
        [Formato] NVARCHAR(20) NOT NULL DEFAULT 'excel',
        [CronExpression] NVARCHAR(100) NULL,
        [Destinatarios] NVARCHAR(MAX) NULL,
        [Parametros] NVARCHAR(MAX) NULL,
        [Columnas] NVARCHAR(MAX) NULL,
        [Activo] BIT NOT NULL DEFAULT 1,
        [UltimaEjecucion] DATETIME2(7) NULL,
        [ProximaEjecucion] DATETIME2(7) NULL,
        [CreadoPor] INT NOT NULL,
        [FechaCreacion] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),

        CONSTRAINT [PK_RT_ReporteProgramado] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    PRINT 'Tabla RT_ReporteProgramado creada exitosamente';
END
ELSE
BEGIN
    PRINT 'Tabla RT_ReporteProgramado ya existe';
END
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- TABLA: RT_ReporteHistorial
-- ═══════════════════════════════════════════════════════════════════════════════

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RT_ReporteHistorial]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[RT_ReporteHistorial] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [ReporteProgramadoId] INT NULL,
        [TipoReporte] NVARCHAR(50) NOT NULL,
        [Entidad] NVARCHAR(100) NULL,
        [Formato] NVARCHAR(20) NOT NULL,
        [FechaGeneracion] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
        [GeneradoPor] INT NULL,
        [TamanioBytes] BIGINT NULL,
        [Registros] INT NOT NULL DEFAULT 0,
        [Estado] NVARCHAR(20) NOT NULL DEFAULT 'EnProceso',
        [ErrorMensaje] NVARCHAR(MAX) NULL,
        [DuracionMs] INT NULL,
        [Parametros] NVARCHAR(MAX) NULL,

        CONSTRAINT [PK_RT_ReporteHistorial] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_RT_ReporteHistorial_ReporteProgramado] FOREIGN KEY ([ReporteProgramadoId])
            REFERENCES [dbo].[RT_ReporteProgramado] ([Id]) ON DELETE SET NULL
    );

    PRINT 'Tabla RT_ReporteHistorial creada exitosamente';
END
ELSE
BEGIN
    PRINT 'Tabla RT_ReporteHistorial ya existe';
END
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- ÍNDICES PARA RT_ReporteProgramado
-- ═══════════════════════════════════════════════════════════════════════════════

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ReporteProgramado_Activo')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_ReporteProgramado_Activo]
    ON [dbo].[RT_ReporteProgramado] ([Activo] ASC)
    WHERE [Activo] = 1;

    PRINT 'Índice IX_ReporteProgramado_Activo creado';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ReporteProgramado_Entidad')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_ReporteProgramado_Entidad]
    ON [dbo].[RT_ReporteProgramado] ([Entidad] ASC)
    WHERE [Entidad] IS NOT NULL;

    PRINT 'Índice IX_ReporteProgramado_Entidad creado';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ReporteProgramado_CreadoPor')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_ReporteProgramado_CreadoPor]
    ON [dbo].[RT_ReporteProgramado] ([CreadoPor] ASC);

    PRINT 'Índice IX_ReporteProgramado_CreadoPor creado';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ReporteProgramado_ProximaEjecucion')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_ReporteProgramado_ProximaEjecucion]
    ON [dbo].[RT_ReporteProgramado] ([ProximaEjecucion] ASC)
    WHERE [ProximaEjecucion] IS NOT NULL AND [Activo] = 1;

    PRINT 'Índice IX_ReporteProgramado_ProximaEjecucion creado';
END
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- ÍNDICES PARA RT_ReporteHistorial
-- ═══════════════════════════════════════════════════════════════════════════════

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ReporteHistorial_FechaGeneracion')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_ReporteHistorial_FechaGeneracion]
    ON [dbo].[RT_ReporteHistorial] ([FechaGeneracion] DESC);

    PRINT 'Índice IX_ReporteHistorial_FechaGeneracion creado';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ReporteHistorial_GeneradoPor')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_ReporteHistorial_GeneradoPor]
    ON [dbo].[RT_ReporteHistorial] ([GeneradoPor] ASC)
    WHERE [GeneradoPor] IS NOT NULL;

    PRINT 'Índice IX_ReporteHistorial_GeneradoPor creado';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ReporteHistorial_Entidad')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_ReporteHistorial_Entidad]
    ON [dbo].[RT_ReporteHistorial] ([Entidad] ASC)
    WHERE [Entidad] IS NOT NULL;

    PRINT 'Índice IX_ReporteHistorial_Entidad creado';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ReporteHistorial_Estado')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_ReporteHistorial_Estado]
    ON [dbo].[RT_ReporteHistorial] ([Estado] ASC);

    PRINT 'Índice IX_ReporteHistorial_Estado creado';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ReporteHistorial_ReporteProgramadoId')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_ReporteHistorial_ReporteProgramadoId]
    ON [dbo].[RT_ReporteHistorial] ([ReporteProgramadoId] ASC)
    WHERE [ReporteProgramadoId] IS NOT NULL;

    PRINT 'Índice IX_ReporteHistorial_ReporteProgramadoId creado';
END
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- CAPABILITIES DE REPORTES
-- ═══════════════════════════════════════════════════════════════════════════════

IF NOT EXISTS (SELECT 1 FROM [dbo].[PC_CAPABILITIES] WHERE [Nombre] = 'ReportesModule.Ver')
BEGIN
    INSERT INTO [dbo].[PC_CAPABILITIES] ([Nombre], [Descripcion], [Modulo], [Activo])
    VALUES
        ('ReportesModule.Ver', 'Ver módulo de reportes', 'Reportes', 1),
        ('ReportesModule.Exportar', 'Exportar datos a Excel/CSV', 'Reportes', 1),
        ('ReportesModule.CrearProgramado', 'Crear reportes programados', 'Reportes', 1),
        ('ReportesModule.EditarProgramado', 'Editar reportes programados', 'Reportes', 1),
        ('ReportesModule.EliminarProgramado', 'Eliminar reportes programados', 'Reportes', 1),
        ('ReportesModule.VerHistorial', 'Ver historial de reportes', 'Reportes', 1),
        ('ReportesModule.VerEstadisticas', 'Ver estadísticas de reportes', 'Reportes', 1),
        ('ReportesModule.LimpiarHistorial', 'Limpiar historial de reportes', 'Reportes', 1);

    PRINT 'Capabilities de Reportes creadas';
END
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- DATOS DE EJEMPLO (OPCIONAL - COMENTAR EN PRODUCCIÓN)
-- ═══════════════════════════════════════════════════════════════════════════════

-- Ejemplo de reporte programado
-- INSERT INTO [dbo].[RT_ReporteProgramado]
--     ([Nombre], [TipoReporte], [Entidad], [Formato], [CronExpression], [Destinatarios], [CreadoPor])
-- VALUES
--     ('Reporte Semanal de Documentos', 'Listado', 'Documentos', 'excel', '0 8 * * 1', '["admin@empresa.com"]', 1);

PRINT '═══════════════════════════════════════════════════════════════════════════════';
PRINT 'Script de Reportes ejecutado exitosamente';
PRINT '═══════════════════════════════════════════════════════════════════════════════';
