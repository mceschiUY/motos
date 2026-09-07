IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PC_CATEGORIAS')
BEGIN
    CREATE TABLE [PC_CATEGORIAS] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [Nombre] NVARCHAR(120) NOT NULL,
        [CategoriaPadreId] INT NULL,
        [Activo] BIT NOT NULL DEFAULT 1
    );
    CREATE INDEX [IX_PC_CATEGORIAS_CategoriaPadreId] ON [PC_CATEGORIAS]([CategoriaPadreId]);
    PRINT 'Tabla PC_CATEGORIAS creada';
END
ELSE
    PRINT 'Tabla PC_CATEGORIAS ya existe';
GO

-- Seeds base (idempotentes): categorías raíz + subrubros del negocio de moto.
IF NOT EXISTS (SELECT 1 FROM PC_CATEGORIAS WHERE Nombre = 'Casco')
    INSERT INTO PC_CATEGORIAS (Nombre, CategoriaPadreId, Activo) VALUES ('Casco', NULL, 1);
IF NOT EXISTS (SELECT 1 FROM PC_CATEGORIAS WHERE Nombre = 'Indumentaria')
    INSERT INTO PC_CATEGORIAS (Nombre, CategoriaPadreId, Activo) VALUES ('Indumentaria', NULL, 1);
IF NOT EXISTS (SELECT 1 FROM PC_CATEGORIAS WHERE Nombre = 'Accesorios')
    INSERT INTO PC_CATEGORIAS (Nombre, CategoriaPadreId, Activo) VALUES ('Accesorios', NULL, 1);
GO

DECLARE @casco INT = (SELECT Id FROM PC_CATEGORIAS WHERE Nombre = 'Casco');
DECLARE @indu  INT = (SELECT Id FROM PC_CATEGORIAS WHERE Nombre = 'Indumentaria');

IF @casco IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM PC_CATEGORIAS WHERE Nombre = 'Integral')  INSERT INTO PC_CATEGORIAS (Nombre, CategoriaPadreId, Activo) VALUES ('Integral', @casco, 1);
    IF NOT EXISTS (SELECT 1 FROM PC_CATEGORIAS WHERE Nombre = 'Modular')   INSERT INTO PC_CATEGORIAS (Nombre, CategoriaPadreId, Activo) VALUES ('Modular', @casco, 1);
    IF NOT EXISTS (SELECT 1 FROM PC_CATEGORIAS WHERE Nombre = 'Jet')       INSERT INTO PC_CATEGORIAS (Nombre, CategoriaPadreId, Activo) VALUES ('Jet', @casco, 1);
    IF NOT EXISTS (SELECT 1 FROM PC_CATEGORIAS WHERE Nombre = 'Cross')     INSERT INTO PC_CATEGORIAS (Nombre, CategoriaPadreId, Activo) VALUES ('Cross', @casco, 1);
END

IF @indu IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM PC_CATEGORIAS WHERE Nombre = 'Campera')   INSERT INTO PC_CATEGORIAS (Nombre, CategoriaPadreId, Activo) VALUES ('Campera', @indu, 1);
    IF NOT EXISTS (SELECT 1 FROM PC_CATEGORIAS WHERE Nombre = 'Guantes')   INSERT INTO PC_CATEGORIAS (Nombre, CategoriaPadreId, Activo) VALUES ('Guantes', @indu, 1);
    IF NOT EXISTS (SELECT 1 FROM PC_CATEGORIAS WHERE Nombre = 'Botas')     INSERT INTO PC_CATEGORIAS (Nombre, CategoriaPadreId, Activo) VALUES ('Botas', @indu, 1);
    IF NOT EXISTS (SELECT 1 FROM PC_CATEGORIAS WHERE Nombre = 'Pantalon')  INSERT INTO PC_CATEGORIAS (Nombre, CategoriaPadreId, Activo) VALUES ('Pantalon', @indu, 1);
END
GO
