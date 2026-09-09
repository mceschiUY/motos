-- PC_VENDEDORES — Vendedor de la fuerza de ventas (plan §3.1, Etapa A). Idempotente.
-- Usuario = login del sitio (texto, no id): el bypass dev 'pablo' no existe en Seg_Usuarios.
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PC_VENDEDORES')
BEGIN
    CREATE TABLE [PC_VENDEDORES] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [Nombre] NVARCHAR(120) NOT NULL,
        [Telefono] NVARCHAR(40) NULL,
        [Email] NVARCHAR(120) NULL,
        [Zona] NVARCHAR(80) NULL,
        [ComisionPorcentaje] DECIMAL(5,2) NOT NULL DEFAULT 0,
        [Usuario] NVARCHAR(80) NULL,
        [Activo] BIT NOT NULL DEFAULT 1
    );
    PRINT 'Tabla PC_VENDEDORES creada';
END
ELSE
    PRINT 'Tabla PC_VENDEDORES ya existe';
GO

-- Seeds (vendedores): viven en Seed_Dominio_Motos.sql (pase Seed_*.sql de DbBootstrap e init-db.sh).
