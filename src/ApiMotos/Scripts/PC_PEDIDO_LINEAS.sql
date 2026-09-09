-- PC_PEDIDO_LINEAS — Línea de pedido (plan §3.5, Etapa B). Idempotente.
-- PrecioUnitarioUsd es una COPIA del precio de lista al armar el pedido: el pedido viejo
-- no cambia de precio cuando cambia la lista. SubtotalUsd = Cantidad × PrecioUnitarioUsd.
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PC_PEDIDO_LINEAS')
BEGIN
    CREATE TABLE [PC_PEDIDO_LINEAS] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [PedidoId] INT NOT NULL,
        [VarianteId] INT NOT NULL,
        [Cantidad] DECIMAL(18,2) NOT NULL,
        [PrecioUnitarioUsd] DECIMAL(18,2) NOT NULL,
        [SubtotalUsd] DECIMAL(18,2) NOT NULL
    );
    CREATE INDEX [IX_PC_PEDIDO_LINEAS_PedidoId] ON [PC_PEDIDO_LINEAS]([PedidoId]);
    CREATE INDEX [IX_PC_PEDIDO_LINEAS_VarianteId] ON [PC_PEDIDO_LINEAS]([VarianteId]);
    PRINT 'Tabla PC_PEDIDO_LINEAS creada';
END
ELSE
    PRINT 'Tabla PC_PEDIDO_LINEAS ya existe';
GO

-- Seeds (líneas): viven en Seed_Dominio_Motos.sql (pase Seed_*.sql de DbBootstrap e init-db.sh).
