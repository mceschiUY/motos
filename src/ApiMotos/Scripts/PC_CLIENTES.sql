-- Script generado automáticamente para PC_CLIENTES
-- Crea la tabla si falta; si existe, agrega columnas faltantes (sin DROP).

IF OBJECT_ID('PC_CLIENTES', 'U') IS NULL
BEGIN
    PRINT 'Tabla PC_CLIENTES no existe. Creando...';
    CREATE TABLE [PC_CLIENTES] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [Nombre] NVARCHAR(255) NOT NULL,
        [Telefono] NVARCHAR(255) NULL,
        [DireccionEntrega] NVARCHAR(255) NULL
    );
    PRINT 'Tabla PC_CLIENTES creada exitosamente';
END
ELSE
BEGIN
    PRINT 'Tabla PC_CLIENTES ya existe. Verificando columnas...';
    IF COL_LENGTH('PC_CLIENTES', 'Nombre') IS NULL
    BEGIN
        ALTER TABLE [PC_CLIENTES] ADD [Nombre] NVARCHAR(255) NOT NULL CONSTRAINT [DF_PC_CLIENTES_Nombre] DEFAULT N'';
        PRINT 'Columna PC_CLIENTES.Nombre agregada';
    END
    IF COL_LENGTH('PC_CLIENTES', 'Telefono') IS NULL
    BEGIN
        ALTER TABLE [PC_CLIENTES] ADD [Telefono] NVARCHAR(255) NULL;
        PRINT 'Columna PC_CLIENTES.Telefono agregada';
    END
    IF COL_LENGTH('PC_CLIENTES', 'DireccionEntrega') IS NULL
    BEGIN
        ALTER TABLE [PC_CLIENTES] ADD [DireccionEntrega] NVARCHAR(255) NULL;
        PRINT 'Columna PC_CLIENTES.DireccionEntrega agregada';
    END
END
