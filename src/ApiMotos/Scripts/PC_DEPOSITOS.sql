IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PC_DEPOSITOS')
BEGIN
    CREATE TABLE [PC_DEPOSITOS] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [Codigo] NVARCHAR(30) NOT NULL,
        [Nombre] NVARCHAR(120) NOT NULL,
        [Direccion] NVARCHAR(250) NULL,
        [Activo] BIT NOT NULL DEFAULT 1
    );
    CREATE UNIQUE INDEX [UX_PC_DEPOSITOS_Codigo] ON [PC_DEPOSITOS]([Codigo]);
    PRINT 'Tabla PC_DEPOSITOS creada';
END
ELSE
    PRINT 'Tabla PC_DEPOSITOS ya existe';
GO

-- Seed: depósito central por defecto (idempotente).
IF NOT EXISTS (SELECT 1 FROM PC_DEPOSITOS WHERE Codigo = 'DEP-CENTRAL')
    INSERT INTO PC_DEPOSITOS (Codigo, Nombre, Direccion, Activo) VALUES ('DEP-CENTRAL', 'Depósito Central', NULL, 1);
GO
