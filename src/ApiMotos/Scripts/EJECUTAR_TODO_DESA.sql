-- ═══════════════════════════════════════════════════════════════════════════════
-- SCRIPT CONSOLIDADO PARA BASE DE DATOS DESA
-- Ejecutar en SQL Server Management Studio
-- Todas las tablas tienen IF NOT EXISTS, seguro ejecutar múltiples veces
-- ═══════════════════════════════════════════════════════════════════════════════

USE DESA;
GO

PRINT '═══════════════════════════════════════════════════════════════════════════════';
PRINT 'Iniciando instalación de esquema ApiMotos en DESA...';
PRINT '═══════════════════════════════════════════════════════════════════════════════';
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- 1. MODULO: DOCUMENTOS
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
    CREATE INDEX IX_PC_DOCUMENTOS_RelacionId ON PC_DOCUMENTOS(relacionid);
    CREATE INDEX IX_PC_DOCUMENTOS_RelacionNombre ON PC_DOCUMENTOS(relacionnombre);
    PRINT '✓ Tabla PC_DOCUMENTOS creada';
END
ELSE
    PRINT '○ Tabla PC_DOCUMENTOS ya existe';
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- 2. MODULO: SEGURIDAD - Capabilities
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
    CREATE UNIQUE INDEX IX_Seg_Capabilities_Nombre ON Seg_Capabilities(Nombre);
    CREATE INDEX IX_Seg_Capabilities_Modulo ON Seg_Capabilities(Modulo);
    PRINT '✓ Tabla Seg_Capabilities creada';
END
ELSE
    PRINT '○ Tabla Seg_Capabilities ya existe';
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- 3. MODULO: SEGURIDAD - Roles
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
    CREATE UNIQUE INDEX IX_Seg_Roles_Nombre ON Seg_Roles(Nombre);
    PRINT '✓ Tabla Seg_Roles creada';
END
ELSE
    PRINT '○ Tabla Seg_Roles ya existe';
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- 4. MODULO: SEGURIDAD - RolCapabilities (N:M)
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
    PRINT '✓ Tabla Seg_RolCapabilities creada';
END
ELSE
    PRINT '○ Tabla Seg_RolCapabilities ya existe';
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- 5. MODULO: SEGURIDAD - Perfiles
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
    CREATE UNIQUE INDEX IX_Seg_Perfiles_Nombre ON Seg_Perfiles(Nombre);
    PRINT '✓ Tabla Seg_Perfiles creada';
END
ELSE
    PRINT '○ Tabla Seg_Perfiles ya existe';
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- 6. MODULO: SEGURIDAD - PerfilRoles (N:M)
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
    PRINT '✓ Tabla Seg_PerfilRoles creada';
END
ELSE
    PRINT '○ Tabla Seg_PerfilRoles ya existe';
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- 7. MODULO: SEGURIDAD - Usuarios
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
    CREATE UNIQUE INDEX IX_Seg_Usuarios_UserName ON Seg_Usuarios(UserName);
    CREATE UNIQUE INDEX IX_Seg_Usuarios_Email ON Seg_Usuarios(Email);
    CREATE INDEX IX_Seg_Usuarios_PerfilId ON Seg_Usuarios(PerfilId);
    PRINT '✓ Tabla Seg_Usuarios creada';
END
ELSE
    PRINT '○ Tabla Seg_Usuarios ya existe';
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- 8. MODULO: CONFIGURACION - ConfiguracionSitio
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
    PRINT '✓ Tabla Cfg_ConfiguracionSitio creada';
END
ELSE
    PRINT '○ Tabla Cfg_ConfiguracionSitio ya existe';
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- DATOS INICIALES: Perfil Administrador
-- ═══════════════════════════════════════════════════════════════════════════════

IF NOT EXISTS (SELECT 1 FROM Seg_Perfiles WHERE Nombre = 'Administrador')
BEGIN
    INSERT INTO Seg_Perfiles (Nombre, Descripcion, Activo, FechaCreacion)
    VALUES ('Administrador', 'Perfil con acceso total al sistema', 1, GETDATE());
    PRINT '✓ Perfil Administrador creado';
END
ELSE
    PRINT '○ Perfil Administrador ya existe';
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- DATOS INICIALES: Usuario admin (Password: Admin123!)
-- ═══════════════════════════════════════════════════════════════════════════════

IF NOT EXISTS (SELECT 1 FROM Seg_Usuarios WHERE UserName = 'admin')
BEGIN
    DECLARE @PerfilAdminId INT;
    SELECT @PerfilAdminId = Id FROM Seg_Perfiles WHERE Nombre = 'Administrador';

    -- Hash BCrypt para 'Admin123!'
    INSERT INTO Seg_Usuarios (UserName, Email, NombreCompleto, PasswordHash, PerfilId, Activo, FechaCreacion)
    VALUES (
        'admin',
        'admin@sistema.local',
        'Administrador del Sistema',
        '$2a$11$rBNr.IFSV8p8cPqNqe8tKu5yvHBN.G8xQVG5vS3iR4k1N5yZvW5Sm',
        @PerfilAdminId,
        1,
        GETDATE()
    );
    PRINT '✓ Usuario admin creado (Password: Admin123!)';
END
ELSE
    PRINT '○ Usuario admin ya existe';
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

    PRINT '✓ Configuraciones iniciales del sitio creadas';
END
ELSE
    PRINT '○ Configuraciones del sitio ya existen';
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- RESUMEN FINAL
-- ═══════════════════════════════════════════════════════════════════════════════

PRINT '';
PRINT '═══════════════════════════════════════════════════════════════════════════════';
PRINT 'INSTALACION COMPLETADA';
PRINT '═══════════════════════════════════════════════════════════════════════════════';
PRINT '';
PRINT 'Tablas del sistema:';
PRINT '  [1] PC_DOCUMENTOS         - Almacenamiento de documentos';
PRINT '  [2] Seg_Capabilities      - Permisos/Capacidades';
PRINT '  [3] Seg_Roles             - Roles del sistema';
PRINT '  [4] Seg_RolCapabilities   - Relacion Roles-Capacidades';
PRINT '  [5] Seg_Perfiles          - Perfiles de usuario';
PRINT '  [6] Seg_PerfilRoles       - Relacion Perfiles-Roles';
PRINT '  [7] Seg_Usuarios          - Usuarios del sistema';
PRINT '  [8] Cfg_ConfiguracionSitio - Configuracion del sitio';
PRINT '';
PRINT 'Usuario por defecto: admin / Admin123!';
PRINT '═══════════════════════════════════════════════════════════════════════════════';
GO
