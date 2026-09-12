-- PC_PRODUCTOS — Producto (modelo/estilo). Crea la tabla si falta; si existe, agrega las
-- columnas faltantes (ALTER ADD, nunca DROP), como PC_CLIENTES.sql.
-- Etapa C (plan §3.7): columnas del catálogo premium, todas NULLABLE (Destacado, Novedad,
-- FichaTecnica, ImagenPrincipalId). En Development las agrega DbBootstrap (sync de columnas
-- del modelo EF); acá quedan para Docker (init-db.sh) y bases ya existentes.
-- ImagenPrincipalId apunta a PC_DOCUMENTOS.id, pero SIN FK: los documentos se borran solos
-- desde su propia pantalla y no queremos que eso bloquee el borrado de una foto.

IF OBJECT_ID('PC_PRODUCTOS', 'U') IS NULL
BEGIN
    PRINT 'Tabla PC_PRODUCTOS no existe. Creando...';
    CREATE TABLE [PC_PRODUCTOS] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [Codigo] NVARCHAR(60) NOT NULL,
        [Nombre] NVARCHAR(200) NOT NULL,
        [MarcaId] INT NOT NULL,
        [CategoriaId] INT NOT NULL,
        [Descripcion] NVARCHAR(1000) NULL,
        [Genero] NVARCHAR(20) NOT NULL DEFAULT 'unisex',
        [Temporada] NVARCHAR(40) NULL,
        [Material] NVARCHAR(120) NULL,
        [PesoGramos] INT NULL,
        [TipoCasco] NVARCHAR(20) NULL,
        [Homologacion] NVARCHAR(20) NULL,
        [HomologacionVigente] BIT NULL,
        [FechaVencHomologacion] DATE NULL,
        [Destacado] BIT NULL,
        [Novedad] BIT NULL,
        [FichaTecnica] NVARCHAR(MAX) NULL,
        [ImagenPrincipalId] INT NULL,
        [Activo] BIT NOT NULL DEFAULT 1
    );
    CREATE UNIQUE INDEX [UX_PC_PRODUCTOS_Codigo] ON [PC_PRODUCTOS]([Codigo]);
    CREATE INDEX [IX_PC_PRODUCTOS_MarcaId] ON [PC_PRODUCTOS]([MarcaId]);
    CREATE INDEX [IX_PC_PRODUCTOS_CategoriaId] ON [PC_PRODUCTOS]([CategoriaId]);
    PRINT 'Tabla PC_PRODUCTOS creada';
END
ELSE
BEGIN
    PRINT 'Tabla PC_PRODUCTOS ya existe. Verificando columnas...';
    -- Etapa C: catálogo premium (todas nullable)
    IF COL_LENGTH('PC_PRODUCTOS', 'Destacado') IS NULL
    BEGIN
        ALTER TABLE [PC_PRODUCTOS] ADD [Destacado] BIT NULL;
        PRINT 'Columna PC_PRODUCTOS.Destacado agregada';
    END
    IF COL_LENGTH('PC_PRODUCTOS', 'Novedad') IS NULL
    BEGIN
        ALTER TABLE [PC_PRODUCTOS] ADD [Novedad] BIT NULL;
        PRINT 'Columna PC_PRODUCTOS.Novedad agregada';
    END
    IF COL_LENGTH('PC_PRODUCTOS', 'FichaTecnica') IS NULL
    BEGIN
        ALTER TABLE [PC_PRODUCTOS] ADD [FichaTecnica] NVARCHAR(MAX) NULL;
        PRINT 'Columna PC_PRODUCTOS.FichaTecnica agregada';
    END
    IF COL_LENGTH('PC_PRODUCTOS', 'ImagenPrincipalId') IS NULL
    BEGIN
        ALTER TABLE [PC_PRODUCTOS] ADD [ImagenPrincipalId] INT NULL;
        PRINT 'Columna PC_PRODUCTOS.ImagenPrincipalId agregada';
    END
END
GO
