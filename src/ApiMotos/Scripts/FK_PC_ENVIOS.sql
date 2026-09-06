-- FOREIGN KEYs de PC_ENVIOS — generado por Forja; idempotente;
-- lo ejecuta DbBootstrap DESPUÉS de crear todas las tablas.
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_PC_ENVIOS_ClienteId')
    AND EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PC_ENVIOS')
    AND EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PC_CLIENTES')
    ALTER TABLE [PC_ENVIOS] ADD CONSTRAINT [FK_PC_ENVIOS_ClienteId]
        FOREIGN KEY ([ClienteId]) REFERENCES [PC_CLIENTES]([Id]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PC_ENVIOS_ClienteId')
    AND EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PC_ENVIOS')
    CREATE INDEX [IX_PC_ENVIOS_ClienteId] ON [PC_ENVIOS]([ClienteId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_PC_ENVIOS_AgenciaId')
    AND EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PC_ENVIOS')
    AND EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PC_AGENCIAS')
    ALTER TABLE [PC_ENVIOS] ADD CONSTRAINT [FK_PC_ENVIOS_AgenciaId]
        FOREIGN KEY ([AgenciaId]) REFERENCES [PC_AGENCIAS]([Id]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PC_ENVIOS_AgenciaId')
    AND EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PC_ENVIOS')
    CREATE INDEX [IX_PC_ENVIOS_AgenciaId] ON [PC_ENVIOS]([AgenciaId]);
GO
