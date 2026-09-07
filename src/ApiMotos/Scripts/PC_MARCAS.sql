IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PC_MARCAS')
BEGIN
    CREATE TABLE [PC_MARCAS] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [Nombre] NVARCHAR(120) NOT NULL,
        [Pais] NVARCHAR(80) NULL,
        [Activo] BIT NOT NULL DEFAULT 1
    );
    CREATE UNIQUE INDEX [UX_PC_MARCAS_Nombre] ON [PC_MARCAS]([Nombre]);
    PRINT 'Tabla PC_MARCAS creada';
END
ELSE
    PRINT 'Tabla PC_MARCAS ya existe';
GO
