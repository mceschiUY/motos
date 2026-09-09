-- PC_ACTIVIDADES — Actividad comercial, el CRM mínimo (plan §3.3, Etapa A). Idempotente.
-- Tipo: visita|llamada|whatsapp|email · Resultado: pedido|sin_pedido|reprogramar|sin_contacto.
-- ProximaAccion (DATE) alimenta la agenda del vendedor. PedidoId SIN FK: Pedido llega en Etapa B.
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PC_ACTIVIDADES')
BEGIN
    CREATE TABLE [PC_ACTIVIDADES] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [VendedorId] INT NOT NULL,
        [ClienteId] INT NOT NULL,
        [Tipo] NVARCHAR(20) NOT NULL,
        [Fecha] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        [Resultado] NVARCHAR(20) NOT NULL,
        [Notas] NVARCHAR(1000) NULL,
        [ProximaAccion] DATE NULL,
        [PedidoId] INT NULL
    );
    CREATE INDEX [IX_PC_ACTIVIDADES_VendedorId] ON [PC_ACTIVIDADES]([VendedorId]);
    CREATE INDEX [IX_PC_ACTIVIDADES_ClienteId] ON [PC_ACTIVIDADES]([ClienteId]);
    PRINT 'Tabla PC_ACTIVIDADES creada';
END
ELSE
    PRINT 'Tabla PC_ACTIVIDADES ya existe';
GO

-- Seeds (actividades): viven en Seed_Dominio_Motos.sql (pase Seed_*.sql de DbBootstrap e init-db.sh).
