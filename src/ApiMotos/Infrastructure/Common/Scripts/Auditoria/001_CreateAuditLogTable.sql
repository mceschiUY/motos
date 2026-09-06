-- ═══════════════════════════════════════════════════════════════════════════════
-- SCRIPT DE CREACIÓN: RT_AuditLog
-- Tabla para registrar todas las acciones del sistema (auditoría)
-- ═══════════════════════════════════════════════════════════════════════════════

-- Verificar si la tabla existe
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RT_AuditLog]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[RT_AuditLog] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [Timestamp] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
        [UserId] INT NULL,
        [UserName] NVARCHAR(100) NULL,
        [Action] NVARCHAR(50) NOT NULL,
        [EntityType] NVARCHAR(100) NOT NULL,
        [EntityId] NVARCHAR(50) NULL,
        [OldValues] NVARCHAR(MAX) NULL,
        [NewValues] NVARCHAR(MAX) NULL,
        [IpAddress] NVARCHAR(50) NULL,
        [UserAgent] NVARCHAR(500) NULL,
        [RequestPath] NVARCHAR(500) NULL,
        [DurationMs] INT NULL,
        [Success] BIT NOT NULL DEFAULT 1,
        [ErrorMessage] NVARCHAR(MAX) NULL,

        CONSTRAINT [PK_RT_AuditLog] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    PRINT 'Tabla RT_AuditLog creada exitosamente';
END
ELSE
BEGIN
    PRINT 'Tabla RT_AuditLog ya existe';
END
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- ÍNDICES
-- ═══════════════════════════════════════════════════════════════════════════════

-- Índice por Timestamp (consultas por rango de fecha)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AuditLog_Timestamp')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_AuditLog_Timestamp]
    ON [dbo].[RT_AuditLog] ([Timestamp] DESC);

    PRINT 'Índice IX_AuditLog_Timestamp creado';
END
GO

-- Índice por UserId (consultas por usuario)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AuditLog_UserId')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_AuditLog_UserId]
    ON [dbo].[RT_AuditLog] ([UserId] ASC)
    WHERE [UserId] IS NOT NULL;

    PRINT 'Índice IX_AuditLog_UserId creado';
END
GO

-- Índice por EntityType + EntityId (historial de una entidad)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AuditLog_EntityType_EntityId')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_AuditLog_EntityType_EntityId]
    ON [dbo].[RT_AuditLog] ([EntityType] ASC, [EntityId] ASC);

    PRINT 'Índice IX_AuditLog_EntityType_EntityId creado';
END
GO

-- Índice por Action (consultas por tipo de acción)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AuditLog_Action')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_AuditLog_Action]
    ON [dbo].[RT_AuditLog] ([Action] ASC);

    PRINT 'Índice IX_AuditLog_Action creado';
END
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- CAPABILITIES DE AUDITORÍA
-- ═══════════════════════════════════════════════════════════════════════════════

-- Insertar capabilities de auditoría si no existen
IF NOT EXISTS (SELECT 1 FROM [dbo].[PC_CAPABILITIES] WHERE [Nombre] = 'AuditoriaModule.Ver')
BEGIN
    INSERT INTO [dbo].[PC_CAPABILITIES] ([Nombre], [Descripcion], [Modulo], [Activo])
    VALUES
        ('AuditoriaModule.Ver', 'Ver registros de auditoría', 'Auditoria', 1),
        ('AuditoriaModule.Exportar', 'Exportar registros de auditoría', 'Auditoria', 1),
        ('AuditoriaModule.Limpiar', 'Eliminar registros antiguos de auditoría', 'Auditoria', 1);

    PRINT 'Capabilities de Auditoria creadas';
END
GO

PRINT '═══════════════════════════════════════════════════════════════════════════════';
PRINT 'Script de Auditoría ejecutado exitosamente';
PRINT '═══════════════════════════════════════════════════════════════════════════════';
