-- FOREIGN KEYs de PC_PEDIDOS y PC_PEDIDO_LINEAS (Etapa B) — idempotente;
-- lo ejecuta DbBootstrap DESPUÉS de crear todas las tablas.
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_PC_PEDIDOS_ClienteId')
    AND EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PC_PEDIDOS')
    AND EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PC_CLIENTES')
    ALTER TABLE [PC_PEDIDOS] ADD CONSTRAINT [FK_PC_PEDIDOS_ClienteId]
        FOREIGN KEY ([ClienteId]) REFERENCES [PC_CLIENTES]([Id]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_PC_PEDIDOS_VendedorId')
    AND EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PC_PEDIDOS')
    AND EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PC_VENDEDORES')
    ALTER TABLE [PC_PEDIDOS] ADD CONSTRAINT [FK_PC_PEDIDOS_VendedorId]
        FOREIGN KEY ([VendedorId]) REFERENCES [PC_VENDEDORES]([Id]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_PC_PEDIDOS_DepositoId')
    AND EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PC_PEDIDOS')
    AND EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PC_DEPOSITOS')
    ALTER TABLE [PC_PEDIDOS] ADD CONSTRAINT [FK_PC_PEDIDOS_DepositoId]
        FOREIGN KEY ([DepositoId]) REFERENCES [PC_DEPOSITOS]([Id]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_PC_PEDIDOS_AgenciaId')
    AND EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PC_PEDIDOS')
    AND EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PC_AGENCIAS')
    ALTER TABLE [PC_PEDIDOS] ADD CONSTRAINT [FK_PC_PEDIDOS_AgenciaId]
        FOREIGN KEY ([AgenciaId]) REFERENCES [PC_AGENCIAS]([Id]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_PC_PEDIDOS_Numero')
    AND EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PC_PEDIDOS')
    CREATE UNIQUE INDEX [UX_PC_PEDIDOS_Numero] ON [PC_PEDIDOS]([Numero]) WHERE [Numero] <> '';
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_PC_PEDIDO_LINEAS_PedidoId')
    AND EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PC_PEDIDO_LINEAS')
    AND EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PC_PEDIDOS')
    ALTER TABLE [PC_PEDIDO_LINEAS] ADD CONSTRAINT [FK_PC_PEDIDO_LINEAS_PedidoId]
        FOREIGN KEY ([PedidoId]) REFERENCES [PC_PEDIDOS]([Id]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_PC_PEDIDO_LINEAS_VarianteId')
    AND EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PC_PEDIDO_LINEAS')
    AND EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PC_VARIANTES')
    ALTER TABLE [PC_PEDIDO_LINEAS] ADD CONSTRAINT [FK_PC_PEDIDO_LINEAS_VarianteId]
        FOREIGN KEY ([VarianteId]) REFERENCES [PC_VARIANTES]([Id]);
GO
-- PC_ENVIOS.PedidoId (plan §3.6): el envío que nació de un pedido lo referencia. Nullable:
-- los envíos sueltos (los que ya existían) siguen valiendo. La columna la agrega DbBootstrap
-- en Development (sync del modelo EF); acá queda para Docker y bases ya creadas.
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PC_ENVIOS') AND COL_LENGTH('PC_ENVIOS', 'PedidoId') IS NULL
BEGIN
    ALTER TABLE [PC_ENVIOS] ADD [PedidoId] INT NULL;
    PRINT 'Columna PC_ENVIOS.PedidoId agregada';
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_PC_ENVIOS_PedidoId')
    AND EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PC_ENVIOS')
    AND EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PC_PEDIDOS')
    AND COL_LENGTH('PC_ENVIOS', 'PedidoId') IS NOT NULL
    ALTER TABLE [PC_ENVIOS] ADD CONSTRAINT [FK_PC_ENVIOS_PedidoId]
        FOREIGN KEY ([PedidoId]) REFERENCES [PC_PEDIDOS]([Id]);
GO
-- Índices de navegación (los crea EF al provisionar de cero; acá quedan por si esa
-- pasada abortó porque la tabla ya existía).
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PC_PEDIDOS_ClienteId')
    AND EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PC_PEDIDOS')
    CREATE INDEX [IX_PC_PEDIDOS_ClienteId] ON [PC_PEDIDOS]([ClienteId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PC_PEDIDOS_VendedorId')
    AND EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PC_PEDIDOS')
    CREATE INDEX [IX_PC_PEDIDOS_VendedorId] ON [PC_PEDIDOS]([VendedorId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PC_PEDIDO_LINEAS_PedidoId')
    AND EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PC_PEDIDO_LINEAS')
    CREATE INDEX [IX_PC_PEDIDO_LINEAS_PedidoId] ON [PC_PEDIDO_LINEAS]([PedidoId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PC_PEDIDO_LINEAS_VarianteId')
    AND EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PC_PEDIDO_LINEAS')
    CREATE INDEX [IX_PC_PEDIDO_LINEAS_VarianteId] ON [PC_PEDIDO_LINEAS]([VarianteId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PC_ENVIOS_PedidoId')
    AND EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PC_ENVIOS')
    AND COL_LENGTH('PC_ENVIOS', 'PedidoId') IS NOT NULL
    CREATE INDEX [IX_PC_ENVIOS_PedidoId] ON [PC_ENVIOS]([PedidoId]);
GO
