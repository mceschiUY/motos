-- =============================================================================
-- TABLA: Cfg_ConfiguracionSitio
-- Configuracion general del sitio (nombre, logo, contacto, redes sociales, etc.)
-- =============================================================================

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

    -- Indices
    CREATE UNIQUE INDEX IX_Cfg_ConfiguracionSitio_Clave ON Cfg_ConfiguracionSitio(Clave);
    CREATE INDEX IX_Cfg_ConfiguracionSitio_Grupo ON Cfg_ConfiguracionSitio(Grupo);
    CREATE INDEX IX_Cfg_ConfiguracionSitio_Activo ON Cfg_ConfiguracionSitio(Activo);

    PRINT 'Tabla Cfg_ConfiguracionSitio creada exitosamente';
END
GO

-- =============================================================================
-- DATOS INICIALES (SEED)
-- =============================================================================

-- Solo insertar si la tabla esta vacia
IF NOT EXISTS (SELECT 1 FROM Cfg_ConfiguracionSitio)
BEGIN
    -- FechaCreacion explícita: la tabla puede haberla creado EF (sin DEFAULT GETDATE)
    INSERT INTO Cfg_ConfiguracionSitio (Clave, Valor, Tipo, Grupo, Orden, Descripcion, FechaCreacion) VALUES
    -- General
    ('sitio.nombre', 'SiteMotos', 'string', 'general', 1, 'Nombre del sitio que se muestra en el header', GETDATE()),
    ('sitio.subtitulo', 'Panel de Control', 'string', 'general', 2, 'Subtitulo del sitio', GETDATE()),
    ('sitio.logo_url', '', 'image', 'general', 3, 'URL o ID del documento del logo', GETDATE()),
    ('sitio.favicon_url', '', 'image', 'general', 4, 'URL del favicon', GETDATE()),

    -- Contacto
    ('empresa.nombre', 'Mi Empresa', 'string', 'contacto', 1, 'Nombre legal de la empresa', GETDATE()),
    ('empresa.email', 'contacto@miempresa.com', 'email', 'contacto', 2, 'Email principal de contacto', GETDATE()),
    ('empresa.telefono', '', 'string', 'contacto', 3, 'Telefono de contacto', GETDATE()),
    ('empresa.direccion', '', 'string', 'contacto', 4, 'Direccion fisica', GETDATE()),
    ('empresa.horario', '', 'string', 'contacto', 5, 'Horario de atencion', GETDATE()),

    -- Redes Sociales
    ('social.facebook', '', 'url', 'redes', 1, 'URL de la pagina de Facebook', GETDATE()),
    ('social.instagram', '', 'url', 'redes', 2, 'URL del perfil de Instagram', GETDATE()),
    ('social.linkedin', '', 'url', 'redes', 3, 'URL de la pagina de LinkedIn', GETDATE()),
    ('social.twitter', '', 'url', 'redes', 4, 'URL del perfil de Twitter/X', GETDATE()),
    ('social.whatsapp', '', 'string', 'redes', 5, 'Numero de WhatsApp con codigo de pais', GETDATE()),
    ('social.youtube', '', 'url', 'redes', 6, 'URL del canal de YouTube', GETDATE()),

    -- Legal
    ('legal.copyright', '2024 Mi Empresa. Todos los derechos reservados.', 'string', 'legal', 1, 'Texto de copyright para el footer', GETDATE());

    PRINT 'Datos iniciales insertados exitosamente';
END
GO
