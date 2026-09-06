-- Script generado automáticamente para PC_PARAMETROSLAS
-- Crea la tabla si falta; si existe, agrega columnas faltantes (sin DROP).

IF OBJECT_ID('PC_PARAMETROSLAS', 'U') IS NULL
BEGIN
    PRINT 'Tabla PC_PARAMETROSLAS no existe. Creando...';
    CREATE TABLE [PC_PARAMETROSLAS] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [Etapa] NVARCHAR(255) NOT NULL,
        [RangoAlertaUmbralAdvertenciaDias] INT NOT NULL,
        [RangoAlertaLimiteDias] INT NOT NULL
    );
    PRINT 'Tabla PC_PARAMETROSLAS creada exitosamente';
END
ELSE
BEGIN
    PRINT 'Tabla PC_PARAMETROSLAS ya existe. Verificando columnas...';
    IF COL_LENGTH('PC_PARAMETROSLAS', 'Etapa') IS NULL
    BEGIN
        ALTER TABLE [PC_PARAMETROSLAS] ADD [Etapa] NVARCHAR(255) NOT NULL CONSTRAINT [DF_PC_PARAMETROSLAS_Etapa] DEFAULT N'';
        PRINT 'Columna PC_PARAMETROSLAS.Etapa agregada';
    END
    IF COL_LENGTH('PC_PARAMETROSLAS', 'RangoAlertaUmbralAdvertenciaDias') IS NULL
    BEGIN
        ALTER TABLE [PC_PARAMETROSLAS] ADD [RangoAlertaUmbralAdvertenciaDias] INT NOT NULL CONSTRAINT [DF_PC_PARAMETROSLAS_RangoAlertaUmbralAdvertenciaDias] DEFAULT 0;
        PRINT 'Columna PC_PARAMETROSLAS.RangoAlertaUmbralAdvertenciaDias agregada';
    END
    IF COL_LENGTH('PC_PARAMETROSLAS', 'RangoAlertaLimiteDias') IS NULL
    BEGIN
        ALTER TABLE [PC_PARAMETROSLAS] ADD [RangoAlertaLimiteDias] INT NOT NULL CONSTRAINT [DF_PC_PARAMETROSLAS_RangoAlertaLimiteDias] DEFAULT 0;
        PRINT 'Columna PC_PARAMETROSLAS.RangoAlertaLimiteDias agregada';
    END
END
