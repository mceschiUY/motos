-- PC_METAS — Meta mensual por vendedor en USD (plan §3.8 / §4.8, Etapa A). Idempotente.
-- Periodo = 'YYYY-MM'. Única por (VendedorId, Periodo).
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PC_METAS')
BEGIN
    CREATE TABLE [PC_METAS] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [VendedorId] INT NOT NULL,
        [Periodo] NVARCHAR(7) NOT NULL,
        [ObjetivoUsd] DECIMAL(18,2) NOT NULL
    );
    CREATE UNIQUE INDEX [UX_PC_METAS_VendedorId_Periodo] ON [PC_METAS]([VendedorId], [Periodo]);
    PRINT 'Tabla PC_METAS creada';
END
ELSE
    PRINT 'Tabla PC_METAS ya existe';
GO

-- Seeds (metas): viven en Seed_Dominio_Motos.sql (pase Seed_*.sql de DbBootstrap e init-db.sh).
