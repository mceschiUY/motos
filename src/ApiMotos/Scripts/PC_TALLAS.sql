IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PC_TALLAS')
BEGIN
    CREATE TABLE [PC_TALLAS] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [Nombre] NVARCHAR(30) NOT NULL,
        [Tipo] NVARCHAR(20) NOT NULL,
        [Orden] INT NOT NULL DEFAULT 0,
        [Activo] BIT NOT NULL DEFAULT 1
    );
    CREATE UNIQUE INDEX [UX_PC_TALLAS_Nombre] ON [PC_TALLAS]([Nombre]);
    PRINT 'Tabla PC_TALLAS creada';
END
ELSE
    PRINT 'Tabla PC_TALLAS ya existe';
GO

-- Seeds: tallas alfabéticas (indumentaria) y numéricas (cascos).
IF NOT EXISTS (SELECT 1 FROM PC_TALLAS WHERE Nombre = 'XS')  INSERT INTO PC_TALLAS (Nombre, Tipo, Orden, Activo) VALUES ('XS', 'alfabetica', 1, 1);
IF NOT EXISTS (SELECT 1 FROM PC_TALLAS WHERE Nombre = 'S')   INSERT INTO PC_TALLAS (Nombre, Tipo, Orden, Activo) VALUES ('S', 'alfabetica', 2, 1);
IF NOT EXISTS (SELECT 1 FROM PC_TALLAS WHERE Nombre = 'M')   INSERT INTO PC_TALLAS (Nombre, Tipo, Orden, Activo) VALUES ('M', 'alfabetica', 3, 1);
IF NOT EXISTS (SELECT 1 FROM PC_TALLAS WHERE Nombre = 'L')   INSERT INTO PC_TALLAS (Nombre, Tipo, Orden, Activo) VALUES ('L', 'alfabetica', 4, 1);
IF NOT EXISTS (SELECT 1 FROM PC_TALLAS WHERE Nombre = 'XL')  INSERT INTO PC_TALLAS (Nombre, Tipo, Orden, Activo) VALUES ('XL', 'alfabetica', 5, 1);
IF NOT EXISTS (SELECT 1 FROM PC_TALLAS WHERE Nombre = 'XXL') INSERT INTO PC_TALLAS (Nombre, Tipo, Orden, Activo) VALUES ('XXL', 'alfabetica', 6, 1);
IF NOT EXISTS (SELECT 1 FROM PC_TALLAS WHERE Nombre = '53-54') INSERT INTO PC_TALLAS (Nombre, Tipo, Orden, Activo) VALUES ('53-54', 'numerica', 10, 1);
IF NOT EXISTS (SELECT 1 FROM PC_TALLAS WHERE Nombre = '55-56') INSERT INTO PC_TALLAS (Nombre, Tipo, Orden, Activo) VALUES ('55-56', 'numerica', 11, 1);
IF NOT EXISTS (SELECT 1 FROM PC_TALLAS WHERE Nombre = '57-58') INSERT INTO PC_TALLAS (Nombre, Tipo, Orden, Activo) VALUES ('57-58', 'numerica', 12, 1);
IF NOT EXISTS (SELECT 1 FROM PC_TALLAS WHERE Nombre = '59-60') INSERT INTO PC_TALLAS (Nombre, Tipo, Orden, Activo) VALUES ('59-60', 'numerica', 13, 1);
IF NOT EXISTS (SELECT 1 FROM PC_TALLAS WHERE Nombre = '61-62') INSERT INTO PC_TALLAS (Nombre, Tipo, Orden, Activo) VALUES ('61-62', 'numerica', 14, 1);
IF NOT EXISTS (SELECT 1 FROM PC_TALLAS WHERE Nombre = '63-64') INSERT INTO PC_TALLAS (Nombre, Tipo, Orden, Activo) VALUES ('63-64', 'numerica', 15, 1);
GO
