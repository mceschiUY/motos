-- Script generado automáticamente para PC_AGENCIAS
-- Crea la tabla si falta; si existe, agrega columnas faltantes (sin DROP).

IF OBJECT_ID('PC_AGENCIAS', 'U') IS NULL
BEGIN
    PRINT 'Tabla PC_AGENCIAS no existe. Creando...';
    CREATE TABLE [PC_AGENCIAS] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [Nombre] NVARCHAR(255) NOT NULL
    );
    PRINT 'Tabla PC_AGENCIAS creada exitosamente';
END
ELSE
BEGIN
    PRINT 'Tabla PC_AGENCIAS ya existe. Verificando columnas...';
    IF COL_LENGTH('PC_AGENCIAS', 'Nombre') IS NULL
    BEGIN
        ALTER TABLE [PC_AGENCIAS] ADD [Nombre] NVARCHAR(255) NOT NULL CONSTRAINT [DF_PC_AGENCIAS_Nombre] DEFAULT N'';
        PRINT 'Columna PC_AGENCIAS.Nombre agregada';
    END
END
