-- Script generado automáticamente para PC_CLIENTES
-- Crea la tabla si falta; si existe, agrega columnas faltantes (sin DROP).
-- Etapa A (plan §3.2 / §3.8): columnas comerciales NULLABLE (Tipo, Ciudad, Contacto, Email,
-- VendedorId, Notas, Latitud, Longitud). En Development las agrega DbBootstrap (sync de
-- columnas del modelo EF, ALTER ADD nullable); acá quedan para Docker (init-db.sh) y bases
-- existentes. La FK de VendedorId vive en FK_PC_CLIENTES.sql.

IF OBJECT_ID('PC_CLIENTES', 'U') IS NULL
BEGIN
    PRINT 'Tabla PC_CLIENTES no existe. Creando...';
    CREATE TABLE [PC_CLIENTES] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [Nombre] NVARCHAR(255) NOT NULL,
        [Telefono] NVARCHAR(255) NULL,
        [DireccionEntrega] NVARCHAR(255) NULL,
        [Tipo] NVARCHAR(20) NULL,
        [Ciudad] NVARCHAR(80) NULL,
        [Contacto] NVARCHAR(120) NULL,
        [Email] NVARCHAR(120) NULL,
        [VendedorId] INT NULL,
        [Notas] NVARCHAR(1000) NULL,
        [Latitud] DECIMAL(9,6) NULL,
        [Longitud] DECIMAL(9,6) NULL
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
    -- Etapa A: datos comerciales (todas nullable)
    IF COL_LENGTH('PC_CLIENTES', 'Tipo') IS NULL
    BEGIN
        ALTER TABLE [PC_CLIENTES] ADD [Tipo] NVARCHAR(20) NULL;
        PRINT 'Columna PC_CLIENTES.Tipo agregada';
    END
    IF COL_LENGTH('PC_CLIENTES', 'Ciudad') IS NULL
    BEGIN
        ALTER TABLE [PC_CLIENTES] ADD [Ciudad] NVARCHAR(80) NULL;
        PRINT 'Columna PC_CLIENTES.Ciudad agregada';
    END
    IF COL_LENGTH('PC_CLIENTES', 'Contacto') IS NULL
    BEGIN
        ALTER TABLE [PC_CLIENTES] ADD [Contacto] NVARCHAR(120) NULL;
        PRINT 'Columna PC_CLIENTES.Contacto agregada';
    END
    IF COL_LENGTH('PC_CLIENTES', 'Email') IS NULL
    BEGIN
        ALTER TABLE [PC_CLIENTES] ADD [Email] NVARCHAR(120) NULL;
        PRINT 'Columna PC_CLIENTES.Email agregada';
    END
    IF COL_LENGTH('PC_CLIENTES', 'VendedorId') IS NULL
    BEGIN
        ALTER TABLE [PC_CLIENTES] ADD [VendedorId] INT NULL;
        PRINT 'Columna PC_CLIENTES.VendedorId agregada';
    END
    IF COL_LENGTH('PC_CLIENTES', 'Notas') IS NULL
    BEGIN
        ALTER TABLE [PC_CLIENTES] ADD [Notas] NVARCHAR(1000) NULL;
        PRINT 'Columna PC_CLIENTES.Notas agregada';
    END
    IF COL_LENGTH('PC_CLIENTES', 'Latitud') IS NULL
    BEGIN
        ALTER TABLE [PC_CLIENTES] ADD [Latitud] DECIMAL(9,6) NULL;
        PRINT 'Columna PC_CLIENTES.Latitud agregada';
    END
    IF COL_LENGTH('PC_CLIENTES', 'Longitud') IS NULL
    BEGIN
        ALTER TABLE [PC_CLIENTES] ADD [Longitud] DECIMAL(9,6) NULL;
        PRINT 'Columna PC_CLIENTES.Longitud agregada';
    END
END
