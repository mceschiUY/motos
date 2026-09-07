IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PC_VARIANTES')
BEGIN
    CREATE TABLE [PC_VARIANTES] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [ProductoId] INT NOT NULL,
        [TallaId] INT NULL,
        [ColorId] INT NULL,
        [Sku] NVARCHAR(60) NOT NULL,
        [CodigoBarras] NVARCHAR(40) NULL,
        [CostoEstandar] DECIMAL(18,4) NOT NULL DEFAULT 0,
        [PrecioLista] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [Activo] BIT NOT NULL DEFAULT 1
    );
    CREATE UNIQUE INDEX [UX_PC_VARIANTES_Combo] ON [PC_VARIANTES]([ProductoId],[TallaId],[ColorId]);
    CREATE UNIQUE INDEX [UX_PC_VARIANTES_Sku]   ON [PC_VARIANTES]([Sku]);
    CREATE INDEX [IX_PC_VARIANTES_ProductoId] ON [PC_VARIANTES]([ProductoId]);
    PRINT 'Tabla PC_VARIANTES creada';
END
ELSE
    PRINT 'Tabla PC_VARIANTES ya existe';
GO
