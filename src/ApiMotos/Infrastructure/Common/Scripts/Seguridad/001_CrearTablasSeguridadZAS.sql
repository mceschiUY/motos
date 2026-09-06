-- ═══════════════════════════════════════════════════════════════════════════════
-- SISTEMA DE SEGURIDAD ZAS
-- Script de creación de tablas
-- Ejecutar en orden: Capabilities -> Roles -> RolCapabilities -> Perfiles -> PerfilRoles -> Usuarios
-- ═══════════════════════════════════════════════════════════════════════════════

-- ═══════════════════════════════════════════════════════════════════════════════
-- TABLA: Seg_Capabilities
-- Acciones atómicas del sistema (ej: "Lead.Crear", "Cliente.Ver")
-- ═══════════════════════════════════════════════════════════════════════════════
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Seg_Capabilities')
BEGIN
    CREATE TABLE Seg_Capabilities (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Nombre NVARCHAR(100) NOT NULL,
        Descripcion NVARCHAR(500) NULL,
        Modulo NVARCHAR(50) NOT NULL,
        Activo BIT NOT NULL DEFAULT 1,

        CONSTRAINT UQ_Seg_Capabilities_Nombre UNIQUE (Nombre)
    );

    CREATE INDEX IX_Seg_Capabilities_Modulo ON Seg_Capabilities(Modulo);

    PRINT 'Tabla Seg_Capabilities creada correctamente';
END
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- TABLA: Seg_Roles
-- Agrupación de Capabilities (ej: "Vendedor", "Administrador")
-- ═══════════════════════════════════════════════════════════════════════════════
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Seg_Roles')
BEGIN
    CREATE TABLE Seg_Roles (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Nombre NVARCHAR(100) NOT NULL,
        Descripcion NVARCHAR(500) NULL,
        Activo BIT NOT NULL DEFAULT 1,
        FechaCreacion DATETIME2 NOT NULL DEFAULT GETDATE(),

        CONSTRAINT UQ_Seg_Roles_Nombre UNIQUE (Nombre)
    );

    PRINT 'Tabla Seg_Roles creada correctamente';
END
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- TABLA: Seg_RolCapabilities
-- Relación N:M entre Roles y Capabilities
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
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- TABLA: Seg_Perfiles
-- Agrupación de Roles (ej: "Perfil Comercial", "Perfil Gerencia")
-- ═══════════════════════════════════════════════════════════════════════════════
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Seg_Perfiles')
BEGIN
    CREATE TABLE Seg_Perfiles (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Nombre NVARCHAR(100) NOT NULL,
        Descripcion NVARCHAR(500) NULL,
        Activo BIT NOT NULL DEFAULT 1,
        FechaCreacion DATETIME2 NOT NULL DEFAULT GETDATE(),

        CONSTRAINT UQ_Seg_Perfiles_Nombre UNIQUE (Nombre)
    );

    PRINT 'Tabla Seg_Perfiles creada correctamente';
END
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- TABLA: Seg_PerfilRoles
-- Relación N:M entre Perfiles y Roles
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
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- TABLA: Seg_Usuarios
-- Usuarios del sistema con Perfil asignado
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

        CONSTRAINT UQ_Seg_Usuarios_UserName UNIQUE (UserName),
        CONSTRAINT UQ_Seg_Usuarios_Email UNIQUE (Email),
        CONSTRAINT FK_Seg_Usuarios_Perfil FOREIGN KEY (PerfilId)
            REFERENCES Seg_Perfiles(Id) ON DELETE NO ACTION
    );

    CREATE INDEX IX_Seg_Usuarios_PerfilId ON Seg_Usuarios(PerfilId);

    PRINT 'Tabla Seg_Usuarios creada correctamente';
END
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- VISTA: vw_Seg_UsuarioCapabilities
-- Vista para obtener todas las capabilities de un usuario
-- ═══════════════════════════════════════════════════════════════════════════════
IF EXISTS (SELECT * FROM sys.views WHERE name = 'vw_Seg_UsuarioCapabilities')
    DROP VIEW vw_Seg_UsuarioCapabilities;
GO

CREATE VIEW vw_Seg_UsuarioCapabilities AS
SELECT DISTINCT
    u.Id AS UsuarioId,
    u.UserName,
    u.NombreCompleto,
    p.Id AS PerfilId,
    p.Nombre AS PerfilNombre,
    r.Id AS RolId,
    r.Nombre AS RolNombre,
    c.Id AS CapabilityId,
    c.Nombre AS CapabilityNombre,
    c.Modulo
FROM Seg_Usuarios u
INNER JOIN Seg_Perfiles p ON u.PerfilId = p.Id AND p.Activo = 1
INNER JOIN Seg_PerfilRoles pr ON p.Id = pr.PerfilId
INNER JOIN Seg_Roles r ON pr.RolId = r.Id AND r.Activo = 1
INNER JOIN Seg_RolCapabilities rc ON r.Id = rc.RolId
INNER JOIN Seg_Capabilities c ON rc.CapabilityId = c.Id AND c.Activo = 1
WHERE u.Activo = 1;
GO

PRINT 'Vista vw_Seg_UsuarioCapabilities creada correctamente';
GO

PRINT '═══════════════════════════════════════════════════════════════════════════════';
PRINT 'SISTEMA DE SEGURIDAD ZAS - Tablas creadas correctamente';
PRINT '═══════════════════════════════════════════════════════════════════════════════';
GO
