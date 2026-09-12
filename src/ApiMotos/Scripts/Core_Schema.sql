-- ═══════════════════════════════════════════════════════════════════════════════
-- CORE SCHEMA - Tablas base para todos los universos
-- Ejecutar al crear una nueva base de datos de universo
-- ═══════════════════════════════════════════════════════════════════════════════

-- ═══════════════════════════════════════════════════════════════════════════════
-- MODULO: DOCUMENTOS
-- ═══════════════════════════════════════════════════════════════════════════════

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PC_DOCUMENTOS')
BEGIN
    CREATE TABLE PC_DOCUMENTOS (
        id INT IDENTITY(1,1) PRIMARY KEY,
        nombre NVARCHAR(255) NOT NULL,
        extension NVARCHAR(50) NOT NULL,
        contenido VARBINARY(MAX) NOT NULL,
        mimetype NVARCHAR(100) NOT NULL,
        fechacarga DATETIME2 NOT NULL DEFAULT GETDATE(),
        relacionid INT NOT NULL,
        relacionnombre NVARCHAR(100) NOT NULL
    );

    -- Indices
    CREATE INDEX IX_PC_DOCUMENTOS_RelacionId ON PC_DOCUMENTOS(relacionid);
    CREATE INDEX IX_PC_DOCUMENTOS_RelacionNombre ON PC_DOCUMENTOS(relacionnombre);

    PRINT 'Tabla PC_DOCUMENTOS creada correctamente';
END
ELSE
BEGIN
    PRINT 'Tabla PC_DOCUMENTOS ya existe';
END
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- MODULO: SEGURIDAD - Capabilities
-- ═══════════════════════════════════════════════════════════════════════════════

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Seg_Capabilities')
BEGIN
    CREATE TABLE Seg_Capabilities (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Nombre NVARCHAR(100) NOT NULL,
        Descripcion NVARCHAR(500) NULL,
        Modulo NVARCHAR(50) NOT NULL,
        Activo BIT NOT NULL DEFAULT 1
    );

    -- Indices
    CREATE UNIQUE INDEX IX_Seg_Capabilities_Nombre ON Seg_Capabilities(Nombre);
    CREATE INDEX IX_Seg_Capabilities_Modulo ON Seg_Capabilities(Modulo);

    PRINT 'Tabla Seg_Capabilities creada correctamente';
END
ELSE
BEGIN
    PRINT 'Tabla Seg_Capabilities ya existe';
END
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- MODULO: SEGURIDAD - Roles
-- ═══════════════════════════════════════════════════════════════════════════════

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Seg_Roles')
BEGIN
    CREATE TABLE Seg_Roles (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Nombre NVARCHAR(100) NOT NULL,
        Descripcion NVARCHAR(500) NULL,
        Activo BIT NOT NULL DEFAULT 1,
        FechaCreacion DATETIME2 NOT NULL DEFAULT GETDATE()
    );

    -- Indices
    CREATE UNIQUE INDEX IX_Seg_Roles_Nombre ON Seg_Roles(Nombre);

    PRINT 'Tabla Seg_Roles creada correctamente';
END
ELSE
BEGIN
    PRINT 'Tabla Seg_Roles ya existe';
END
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- MODULO: SEGURIDAD - RolCapabilities (N:M)
-- ═══════════════════════════════════════════════════════════════════════════════

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Seg_RolCapabilities')
BEGIN
    CREATE TABLE Seg_RolCapabilities (
        RolId INT NOT NULL,
        CapabilityId INT NOT NULL,
        FechaAsignacion DATETIME2 NOT NULL DEFAULT GETDATE(),

        CONSTRAINT PK_Seg_RolCapabilities PRIMARY KEY (RolId, CapabilityId),
        CONSTRAINT FK_Seg_RolCapabilities_Rol FOREIGN KEY (RolId)
            REFERENCES Seg_Roles(Id) ON DELETE CASCADE,
        CONSTRAINT FK_Seg_RolCapabilities_Capability FOREIGN KEY (CapabilityId)
            REFERENCES Seg_Capabilities(Id) ON DELETE CASCADE
    );

    PRINT 'Tabla Seg_RolCapabilities creada correctamente';
END
ELSE
BEGIN
    PRINT 'Tabla Seg_RolCapabilities ya existe';
END
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- MODULO: SEGURIDAD - Perfiles
-- ═══════════════════════════════════════════════════════════════════════════════

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Seg_Perfiles')
BEGIN
    CREATE TABLE Seg_Perfiles (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Nombre NVARCHAR(100) NOT NULL,
        Descripcion NVARCHAR(500) NULL,
        Activo BIT NOT NULL DEFAULT 1,
        FechaCreacion DATETIME2 NOT NULL DEFAULT GETDATE()
    );

    -- Indices
    CREATE UNIQUE INDEX IX_Seg_Perfiles_Nombre ON Seg_Perfiles(Nombre);

    PRINT 'Tabla Seg_Perfiles creada correctamente';
END
ELSE
BEGIN
    PRINT 'Tabla Seg_Perfiles ya existe';
END
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- MODULO: SEGURIDAD - PerfilRoles (N:M)
-- ═══════════════════════════════════════════════════════════════════════════════

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Seg_PerfilRoles')
BEGIN
    CREATE TABLE Seg_PerfilRoles (
        PerfilId INT NOT NULL,
        RolId INT NOT NULL,
        FechaAsignacion DATETIME2 NOT NULL DEFAULT GETDATE(),

        CONSTRAINT PK_Seg_PerfilRoles PRIMARY KEY (PerfilId, RolId),
        CONSTRAINT FK_Seg_PerfilRoles_Perfil FOREIGN KEY (PerfilId)
            REFERENCES Seg_Perfiles(Id) ON DELETE CASCADE,
        CONSTRAINT FK_Seg_PerfilRoles_Rol FOREIGN KEY (RolId)
            REFERENCES Seg_Roles(Id) ON DELETE CASCADE
    );

    PRINT 'Tabla Seg_PerfilRoles creada correctamente';
END
ELSE
BEGIN
    PRINT 'Tabla Seg_PerfilRoles ya existe';
END
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- MODULO: SEGURIDAD - Usuarios
-- ═══════════════════════════════════════════════════════════════════════════════

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Seg_Usuarios')
BEGIN
    CREATE TABLE Seg_Usuarios (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        UserName NVARCHAR(50) NOT NULL,
        Email NVARCHAR(150) NOT NULL,
        NombreCompleto NVARCHAR(200) NOT NULL,
        PasswordHash NVARCHAR(500) NOT NULL,
        PerfilId INT NOT NULL,
        Activo BIT NOT NULL DEFAULT 1,
        FechaCreacion DATETIME2 NOT NULL DEFAULT GETDATE(),
        UltimoLogin DATETIME2 NULL,
        IntentosFallidos INT NOT NULL DEFAULT 0,
        BloqueadoHasta DATETIME2 NULL,

        CONSTRAINT FK_Seg_Usuarios_Perfil FOREIGN KEY (PerfilId)
            REFERENCES Seg_Perfiles(Id) ON DELETE NO ACTION
    );

    -- Indices
    CREATE UNIQUE INDEX IX_Seg_Usuarios_UserName ON Seg_Usuarios(UserName);
    CREATE UNIQUE INDEX IX_Seg_Usuarios_Email ON Seg_Usuarios(Email);
    CREATE INDEX IX_Seg_Usuarios_PerfilId ON Seg_Usuarios(PerfilId);

    PRINT 'Tabla Seg_Usuarios creada correctamente';
END
ELSE
BEGIN
    PRINT 'Tabla Seg_Usuarios ya existe';
END
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- MODULO: CONFIGURACION - ConfiguracionSitio
-- ═══════════════════════════════════════════════════════════════════════════════

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Cfg_ConfiguracionSitio')
BEGIN
    CREATE TABLE Cfg_ConfiguracionSitio (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Clave NVARCHAR(100) NOT NULL,
        Valor NVARCHAR(MAX) NOT NULL DEFAULT '',
        Tipo NVARCHAR(50) NOT NULL DEFAULT 'string',
        Grupo NVARCHAR(100) NOT NULL DEFAULT 'general',
        Orden INT NOT NULL DEFAULT 0,
        Descripcion NVARCHAR(500) NULL,
        Activo BIT NOT NULL DEFAULT 1,
        FechaCreacion DATETIME2 NOT NULL DEFAULT GETDATE(),
        FechaActualizacion DATETIME2 NULL
    );

    CREATE UNIQUE INDEX IX_Cfg_ConfiguracionSitio_Clave ON Cfg_ConfiguracionSitio(Clave);
    CREATE INDEX IX_Cfg_ConfiguracionSitio_Grupo ON Cfg_ConfiguracionSitio(Grupo);
    CREATE INDEX IX_Cfg_ConfiguracionSitio_Activo ON Cfg_ConfiguracionSitio(Activo);

    PRINT 'Tabla Cfg_ConfiguracionSitio creada correctamente';
END
ELSE
BEGIN
    PRINT 'Tabla Cfg_ConfiguracionSitio ya existe';
END
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- MODULO: AUDITORIA - AuditLog
-- ═══════════════════════════════════════════════════════════════════════════════

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'RT_AuditLog')
BEGIN
    CREATE TABLE RT_AuditLog (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Timestamp DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UserId INT NULL,
        UserName NVARCHAR(100) NULL,
        Action NVARCHAR(50) NOT NULL,
        EntityType NVARCHAR(100) NOT NULL,
        EntityId NVARCHAR(50) NULL,
        OldValues NVARCHAR(MAX) NULL,
        NewValues NVARCHAR(MAX) NULL,
        IpAddress NVARCHAR(50) NULL,
        UserAgent NVARCHAR(500) NULL,
        RequestPath NVARCHAR(500) NULL,
        DurationMs INT NULL,
        Success BIT NOT NULL DEFAULT 1,
        ErrorMessage NVARCHAR(MAX) NULL
    );

    CREATE INDEX IX_AuditLog_Timestamp ON RT_AuditLog(Timestamp DESC);
    CREATE INDEX IX_AuditLog_UserId ON RT_AuditLog(UserId);
    CREATE INDEX IX_AuditLog_EntityType_EntityId ON RT_AuditLog(EntityType, EntityId);
    CREATE INDEX IX_AuditLog_Action ON RT_AuditLog(Action);

    PRINT 'Tabla RT_AuditLog creada correctamente';
END
ELSE
BEGIN
    PRINT 'Tabla RT_AuditLog ya existe';
END
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- DATOS INICIALES: Perfil y Usuario Administrador
-- ═══════════════════════════════════════════════════════════════════════════════

-- Crear perfil Administrador si no existe
IF NOT EXISTS (SELECT 1 FROM Seg_Perfiles WHERE Nombre = 'Administrador')
BEGIN
    INSERT INTO Seg_Perfiles (Nombre, Descripcion, Activo, FechaCreacion)
    VALUES ('Administrador', 'Perfil con acceso total al sistema', 1, GETDATE());

    PRINT 'Perfil Administrador creado';
END
GO

-- Crear usuario admin si no existe
-- Password por defecto: Admin123! (hash BCrypt)
IF NOT EXISTS (SELECT 1 FROM Seg_Usuarios WHERE UserName = 'admin')
BEGIN
    DECLARE @PerfilAdminId INT;
    SELECT @PerfilAdminId = Id FROM Seg_Perfiles WHERE Nombre = 'Administrador';

    INSERT INTO Seg_Usuarios (UserName, Email, NombreCompleto, PasswordHash, PerfilId, Activo, FechaCreacion)
    VALUES (
        'admin',
        'admin@sistema.local',
        'Administrador del Sistema',
        '$2b$11$Z6B84J7ZghRYPTn7zhsMGeDVQMBaPcyAADNnqxlZfabA8cp1fBeTa',
        @PerfilAdminId,
        1,
        GETDATE()
    );

    PRINT 'Usuario admin creado (cambiar password en produccion)';
END
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- DATOS INICIALES: Configuraciones del Sitio
-- ═══════════════════════════════════════════════════════════════════════════════

IF NOT EXISTS (SELECT 1 FROM Cfg_ConfiguracionSitio WHERE Clave = 'sitio.nombre')
BEGIN
    INSERT INTO Cfg_ConfiguracionSitio (Clave, Valor, Tipo, Grupo, Orden, Descripcion) VALUES
    -- General
    ('sitio.nombre', 'SiteMotos', 'string', 'general', 1, 'Nombre del sitio que se muestra en el header'),
    ('sitio.subtitulo', 'Panel de Control', 'string', 'general', 2, 'Subtitulo del sitio'),
    ('sitio.logo_url', '', 'image', 'general', 3, 'URL o ID del documento del logo'),
    ('sitio.favicon_url', '', 'image', 'general', 4, 'URL del favicon'),
    -- Contacto
    ('empresa.nombre', 'Mi Empresa', 'string', 'contacto', 1, 'Nombre legal de la empresa'),
    ('empresa.email', 'contacto@miempresa.com', 'email', 'contacto', 2, 'Email principal de contacto'),
    ('empresa.telefono', '', 'string', 'contacto', 3, 'Telefono de contacto'),
    ('empresa.direccion', '', 'string', 'contacto', 4, 'Direccion fisica'),
    ('empresa.horario', '', 'string', 'contacto', 5, 'Horario de atencion'),
    -- Redes Sociales
    ('social.facebook', '', 'url', 'redes', 1, 'URL de la pagina de Facebook'),
    ('social.instagram', '', 'url', 'redes', 2, 'URL del perfil de Instagram'),
    ('social.linkedin', '', 'url', 'redes', 3, 'URL de la pagina de LinkedIn'),
    ('social.twitter', '', 'url', 'redes', 4, 'URL del perfil de Twitter/X'),
    ('social.whatsapp', '', 'string', 'redes', 5, 'Numero de WhatsApp con codigo de pais'),
    ('social.youtube', '', 'url', 'redes', 6, 'URL del canal de YouTube'),
    -- Legal
    ('legal.copyright', '2024 Mi Empresa. Todos los derechos reservados.', 'string', 'legal', 1, 'Texto de copyright para el footer');

    PRINT 'Configuraciones iniciales del sitio creadas';
END
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- RESUMEN
-- ═══════════════════════════════════════════════════════════════════════════════

PRINT '═══════════════════════════════════════════════════════════════════════════════';
PRINT 'CORE SCHEMA - Instalacion completada';
PRINT '═══════════════════════════════════════════════════════════════════════════════';
PRINT 'Tablas creadas:';
PRINT '  - PC_DOCUMENTOS (Modulo Documentos)';
PRINT '  - Seg_Capabilities (Modulo Seguridad)';
PRINT '  - Seg_Roles (Modulo Seguridad)';
PRINT '  - Seg_RolCapabilities (Modulo Seguridad)';
PRINT '  - Seg_Perfiles (Modulo Seguridad)';
PRINT '  - Seg_PerfilRoles (Modulo Seguridad)';
PRINT '  - Seg_Usuarios (Modulo Seguridad)';
PRINT '  - Cfg_ConfiguracionSitio (Modulo Configuracion)';
PRINT '  - RT_AuditLog (Modulo Auditoria)';
PRINT '═══════════════════════════════════════════════════════════════════════════════';
GO

-- =============================================================================
-- ALERTAS DEL NEGOCIO (plan Etapa D, 2026-09-12): idempotente por clave, para bases ya creadas.
-- Las leen el centro de control, la agenda, el panel del vendedor y el cliente 360.
-- =============================================================================
IF NOT EXISTS (SELECT 1 FROM Cfg_ConfiguracionSitio WHERE Clave = 'stock.umbral_bajo')
    INSERT INTO Cfg_ConfiguracionSitio (Clave, Valor, Tipo, Grupo, Orden, Descripcion) VALUES
    ('stock.umbral_bajo', '3', 'number', 'alertas', 1, 'Un SKU con menos unidades que este umbral se marca como stock bajo');
IF NOT EXISTS (SELECT 1 FROM Cfg_ConfiguracionSitio WHERE Clave = 'crm.dias_sin_visita')
    INSERT INTO Cfg_ConfiguracionSitio (Clave, Valor, Tipo, Grupo, Orden, Descripcion) VALUES
    ('crm.dias_sin_visita', '30', 'number', 'alertas', 2, 'Un cliente sin actividad hace mas de estos dias dispara la alerta de sin visitar');
GO
