IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PC_COLORES')
BEGIN
    CREATE TABLE [PC_COLORES] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [Nombre] NVARCHAR(60) NOT NULL,
        [CodigoHex] NVARCHAR(7) NULL,
        [Activo] BIT NOT NULL DEFAULT 1
    );
    CREATE UNIQUE INDEX [UX_PC_COLORES_Nombre] ON [PC_COLORES]([Nombre]);
    PRINT 'Tabla PC_COLORES creada';
END
ELSE
    PRINT 'Tabla PC_COLORES ya existe';
GO


-- Seeds (colores): viven en Seed_Dominio_Motos.sql (pase Seed_*.sql de DbBootstrap e init-db.sh).
