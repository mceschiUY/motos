-- FOREIGN KEYs de PC_ACTIVIDADES (Etapa A) — idempotente; lo ejecuta DbBootstrap DESPUÉS de crear todas las tablas.
-- PedidoId queda sin FK hasta que exista PC_PEDIDOS (Etapa B).
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_PC_ACTIVIDADES_VendedorId')
    AND EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PC_ACTIVIDADES')
    AND EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PC_VENDEDORES')
    ALTER TABLE [PC_ACTIVIDADES] ADD CONSTRAINT [FK_PC_ACTIVIDADES_VendedorId]
        FOREIGN KEY ([VendedorId]) REFERENCES [PC_VENDEDORES]([Id]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_PC_ACTIVIDADES_ClienteId')
    AND EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PC_ACTIVIDADES')
    AND EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PC_CLIENTES')
    ALTER TABLE [PC_ACTIVIDADES] ADD CONSTRAINT [FK_PC_ACTIVIDADES_ClienteId]
        FOREIGN KEY ([ClienteId]) REFERENCES [PC_CLIENTES]([Id]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PC_ACTIVIDADES_VendedorId')
    AND EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PC_ACTIVIDADES')
    CREATE INDEX [IX_PC_ACTIVIDADES_VendedorId] ON [PC_ACTIVIDADES]([VendedorId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PC_ACTIVIDADES_ClienteId')
    AND EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PC_ACTIVIDADES')
    CREATE INDEX [IX_PC_ACTIVIDADES_ClienteId] ON [PC_ACTIVIDADES]([ClienteId]);
GO
-- Etapa B: la visita que terminó en pedido lo referencia (PC_PEDIDOS ya existe).
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_PC_ACTIVIDADES_PedidoId')
    AND EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PC_ACTIVIDADES')
    AND EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PC_PEDIDOS')
    ALTER TABLE [PC_ACTIVIDADES] ADD CONSTRAINT [FK_PC_ACTIVIDADES_PedidoId]
        FOREIGN KEY ([PedidoId]) REFERENCES [PC_PEDIDOS]([Id]);
GO
