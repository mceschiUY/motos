-- FOREIGN KEYs de PC_METAS (Etapa A) — idempotente; lo ejecuta DbBootstrap DESPUÉS de crear todas las tablas.
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_PC_METAS_VendedorId')
    AND EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PC_METAS')
    AND EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PC_VENDEDORES')
    ALTER TABLE [PC_METAS] ADD CONSTRAINT [FK_PC_METAS_VendedorId]
        FOREIGN KEY ([VendedorId]) REFERENCES [PC_VENDEDORES]([Id]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_PC_METAS_VendedorId_Periodo')
    AND EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PC_METAS')
    CREATE UNIQUE INDEX [UX_PC_METAS_VendedorId_Periodo] ON [PC_METAS]([VendedorId], [Periodo]);
GO
