IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PC_MOVIMIENTOS_STOCK')
BEGIN
    CREATE TABLE [PC_MOVIMIENTOS_STOCK] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [VarianteId] INT NOT NULL,
        [DepositoId] INT NOT NULL,
        [DepositoDestinoId] INT NULL,
        [Tipo] NVARCHAR(20) NOT NULL,
        [Cantidad] DECIMAL(18,2) NOT NULL,
        [CostoUnitario] DECIMAL(18,4) NULL,
        [Motivo] NVARCHAR(250) NULL,
        [DocumentoOrigen] NVARCHAR(80) NULL,
        [Fecha] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        [Usuario] NVARCHAR(120) NULL
    );
    CREATE INDEX [IX_PC_MOVIMIENTOS_STOCK_VarianteId] ON [PC_MOVIMIENTOS_STOCK]([VarianteId]);
    CREATE INDEX [IX_PC_MOVIMIENTOS_STOCK_DepositoId] ON [PC_MOVIMIENTOS_STOCK]([DepositoId]);
    PRINT 'Tabla PC_MOVIMIENTOS_STOCK creada';
END
ELSE
    PRINT 'Tabla PC_MOVIMIENTOS_STOCK ya existe';
GO
