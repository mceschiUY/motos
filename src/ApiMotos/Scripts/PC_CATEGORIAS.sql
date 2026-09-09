IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PC_CATEGORIAS')
BEGIN
    CREATE TABLE [PC_CATEGORIAS] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [Nombre] NVARCHAR(120) NOT NULL,
        [CategoriaPadreId] INT NULL,
        [Activo] BIT NOT NULL DEFAULT 1
    );
    CREATE INDEX [IX_PC_CATEGORIAS_CategoriaPadreId] ON [PC_CATEGORIAS]([CategoriaPadreId]);
    PRINT 'Tabla PC_CATEGORIAS creada';
END
ELSE
    PRINT 'Tabla PC_CATEGORIAS ya existe';
GO


-- Seeds (categorias): viven en Seed_Dominio_Motos.sql (pase Seed_*.sql de DbBootstrap e init-db.sh).
