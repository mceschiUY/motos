IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PC_COLORES')
BEGIN
    CREATE TABLE [PC_COLORES] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [Nombre] NVARCHAR(60) NOT NULL,
        [CodigoHex] NVARCHAR(7) NULL,
        [Activo] BIT NOT NULL DEFAULT 1
    );
    CREATE UNIQUE INDEX [UX_PC_COLORES_Nombre] ON [PC_COLORES]([Nombre]);
    PRINT 'Tabla PC_COLORES creada';
END
ELSE
    PRINT 'Tabla PC_COLORES ya existe';
GO

-- Seeds base.
IF NOT EXISTS (SELECT 1 FROM PC_COLORES WHERE Nombre = 'Negro')      INSERT INTO PC_COLORES (Nombre, CodigoHex, Activo) VALUES ('Negro', '#000000', 1);
IF NOT EXISTS (SELECT 1 FROM PC_COLORES WHERE Nombre = 'Negro mate') INSERT INTO PC_COLORES (Nombre, CodigoHex, Activo) VALUES ('Negro mate', '#1A1A1A', 1);
IF NOT EXISTS (SELECT 1 FROM PC_COLORES WHERE Nombre = 'Blanco')     INSERT INTO PC_COLORES (Nombre, CodigoHex, Activo) VALUES ('Blanco', '#FFFFFF', 1);
IF NOT EXISTS (SELECT 1 FROM PC_COLORES WHERE Nombre = 'Rojo')       INSERT INTO PC_COLORES (Nombre, CodigoHex, Activo) VALUES ('Rojo', '#E11D2A', 1);
IF NOT EXISTS (SELECT 1 FROM PC_COLORES WHERE Nombre = 'Azul')       INSERT INTO PC_COLORES (Nombre, CodigoHex, Activo) VALUES ('Azul', '#1E5AA8', 1);
IF NOT EXISTS (SELECT 1 FROM PC_COLORES WHERE Nombre = 'Gris')       INSERT INTO PC_COLORES (Nombre, CodigoHex, Activo) VALUES ('Gris', '#808080', 1);
GO
