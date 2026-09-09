IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PC_TALLAS')
BEGIN
    CREATE TABLE [PC_TALLAS] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [Nombre] NVARCHAR(30) NOT NULL,
        [Tipo] NVARCHAR(20) NOT NULL,
        [Orden] INT NOT NULL DEFAULT 0,
        [Activo] BIT NOT NULL DEFAULT 1
    );
    CREATE UNIQUE INDEX [UX_PC_TALLAS_Nombre] ON [PC_TALLAS]([Nombre]);
    PRINT 'Tabla PC_TALLAS creada';
END
ELSE
    PRINT 'Tabla PC_TALLAS ya existe';
GO

-- Seeds (tallas): viven en Seed_Dominio_Motos.sql (pase Seed_*.sql de DbBootstrap e init-db.sh).
