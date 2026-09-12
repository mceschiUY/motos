-- PC_PRECIO_HISTORIAL — cambios de precio y costo por Variante (plan §3.8 / §4.8, Etapa C).
-- Idempotente. Solo la escribe VarianteHooks: no tiene CRUD ni pantalla de alta.
-- Campo = 'precio' (PrecioLista) | 'costo' (CostoEstandar).
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PC_PRECIO_HISTORIAL')
BEGIN
    CREATE TABLE [PC_PRECIO_HISTORIAL] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [VarianteId] INT NOT NULL,
        [Campo] NVARCHAR(10) NOT NULL,
        [ValorAnterior] DECIMAL(18,4) NOT NULL,
        [ValorNuevo] DECIMAL(18,4) NOT NULL,
        [Fecha] DATETIME2 NOT NULL,
        [Usuario] NVARCHAR(80) NULL
    );
    CREATE INDEX [IX_PC_PRECIO_HISTORIAL_VarianteId] ON [PC_PRECIO_HISTORIAL]([VarianteId]);
    PRINT 'Tabla PC_PRECIO_HISTORIAL creada';
END
ELSE
    PRINT 'Tabla PC_PRECIO_HISTORIAL ya existe';
GO

-- Seeds (2 cambios de precio de demo): viven en Seed_Dominio_Motos.sql.
