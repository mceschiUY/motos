-- PC_PEDIDOS — Pedido de venta (plan §3.4, Etapa B). Idempotente.
-- Estado: borrador → confirmado → preparado → despachado → entregado · anulado.
-- Numero lo sella PedidoHooks al crear (PED-000n) — por eso el índice único es FILTRADO:
-- la fila nace con Numero = '' y recién después se sella (dos vacíos no chocan).
-- Todos los importes son USD (plan §7: solo dólares).
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PC_PEDIDOS')
BEGIN
    CREATE TABLE [PC_PEDIDOS] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [Numero] NVARCHAR(20) NOT NULL,
        [Fecha] DATETIME2 NOT NULL,
        [ClienteId] INT NOT NULL,
        [VendedorId] INT NOT NULL,
        [DepositoId] INT NOT NULL,
        [AgenciaId] INT NULL,
        [Estado] NVARCHAR(20) NOT NULL,
        [TotalUsd] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [ComisionUsd] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [Observaciones] NVARCHAR(500) NULL,
        [EnvioId] INT NULL,
        [MotivoAnulacion] NVARCHAR(250) NULL
    );
    CREATE UNIQUE INDEX [UX_PC_PEDIDOS_Numero] ON [PC_PEDIDOS]([Numero]) WHERE [Numero] <> '';
    CREATE INDEX [IX_PC_PEDIDOS_ClienteId] ON [PC_PEDIDOS]([ClienteId]);
    CREATE INDEX [IX_PC_PEDIDOS_VendedorId] ON [PC_PEDIDOS]([VendedorId]);
    PRINT 'Tabla PC_PEDIDOS creada';
END
ELSE
    PRINT 'Tabla PC_PEDIDOS ya existe';
GO

-- Seeds (pedidos): viven en Seed_Dominio_Motos.sql (pase Seed_*.sql de DbBootstrap e init-db.sh).
