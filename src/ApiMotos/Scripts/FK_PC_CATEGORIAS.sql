-- FOREIGN KEY self-referencial de PC_CATEGORIAS (jerarquía). Idempotente;
-- lo ejecuta DbBootstrap DESPUÉS de crear todas las tablas.
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_PC_CATEGORIAS_CategoriaPadreId')
    AND EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PC_CATEGORIAS')
    ALTER TABLE [PC_CATEGORIAS] ADD CONSTRAINT [FK_PC_CATEGORIAS_CategoriaPadreId]
        FOREIGN KEY ([CategoriaPadreId]) REFERENCES [PC_CATEGORIAS]([Id]);
GO
