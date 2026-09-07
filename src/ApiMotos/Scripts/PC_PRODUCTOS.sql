IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PC_PRODUCTOS')
BEGIN
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
        [Activo] BIT NOT NULL DEFAULT 1
    );
    CREATE UNIQUE INDEX [UX_PC_PRODUCTOS_Codigo] ON [PC_PRODUCTOS]([Codigo]);
    CREATE INDEX [IX_PC_PRODUCTOS_MarcaId] ON [PC_PRODUCTOS]([MarcaId]);
    CREATE INDEX [IX_PC_PRODUCTOS_CategoriaId] ON [PC_PRODUCTOS]([CategoriaId]);
    PRINT 'Tabla PC_PRODUCTOS creada';
END
ELSE
    PRINT 'Tabla PC_PRODUCTOS ya existe';
GO
