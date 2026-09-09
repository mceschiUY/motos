-- FOREIGN KEYs de PC_CLIENTES (Etapa A) — idempotente; lo ejecuta DbBootstrap DESPUÉS de crear
-- todas las tablas y de sincronizar columnas (VendedorId se agrega por ALTER ADD en bases existentes).
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_PC_CLIENTES_VendedorId')
    AND EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PC_CLIENTES')
    AND EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PC_VENDEDORES')
    AND COL_LENGTH('PC_CLIENTES', 'VendedorId') IS NOT NULL
    ALTER TABLE [PC_CLIENTES] ADD CONSTRAINT [FK_PC_CLIENTES_VendedorId]
        FOREIGN KEY ([VendedorId]) REFERENCES [PC_VENDEDORES]([Id]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PC_CLIENTES_VendedorId')
    AND EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PC_CLIENTES')
    AND COL_LENGTH('PC_CLIENTES', 'VendedorId') IS NOT NULL
    CREATE INDEX [IX_PC_CLIENTES_VendedorId] ON [PC_CLIENTES]([VendedorId]);
GO
