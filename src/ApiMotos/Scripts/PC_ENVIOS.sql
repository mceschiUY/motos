-- Script generado automáticamente para PC_ENVIOS
-- Crea la tabla si falta; si existe, agrega columnas faltantes (sin DROP).

IF OBJECT_ID('PC_ENVIOS', 'U') IS NULL
BEGIN
    PRINT 'Tabla PC_ENVIOS no existe. Creando...';
    CREATE TABLE [PC_ENVIOS] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [CodigoRastreo] NVARCHAR(255) NOT NULL,
        [Estado] NVARCHAR(255) NOT NULL,
        [FechaRecibido] DATETIME2 NOT NULL,
        [FechaFactura] DATETIME2 NULL,
        [FechaEnvio] DATETIME2 NULL,
        [FechaEntrega] DATETIME2 NULL,
        [MotivoAnulacion] NVARCHAR(255) NULL,
        [ClienteId] INT NOT NULL,
        [AgenciaId] INT NOT NULL,
        [PedidoId] INT NULL          -- Etapa B (plan §3.6): pedido que originó el envío
    );
    PRINT 'Tabla PC_ENVIOS creada exitosamente';
END
ELSE
BEGIN
    PRINT 'Tabla PC_ENVIOS ya existe. Verificando columnas...';
    IF COL_LENGTH('PC_ENVIOS', 'CodigoRastreo') IS NULL
    BEGIN
        ALTER TABLE [PC_ENVIOS] ADD [CodigoRastreo] NVARCHAR(255) NOT NULL CONSTRAINT [DF_PC_ENVIOS_CodigoRastreo] DEFAULT N'';
        PRINT 'Columna PC_ENVIOS.CodigoRastreo agregada';
    END
    IF COL_LENGTH('PC_ENVIOS', 'Estado') IS NULL
    BEGIN
        ALTER TABLE [PC_ENVIOS] ADD [Estado] NVARCHAR(255) NOT NULL CONSTRAINT [DF_PC_ENVIOS_Estado] DEFAULT N'';
        PRINT 'Columna PC_ENVIOS.Estado agregada';
    END
    IF COL_LENGTH('PC_ENVIOS', 'FechaRecibido') IS NULL
    BEGIN
        ALTER TABLE [PC_ENVIOS] ADD [FechaRecibido] DATETIME2 NOT NULL CONSTRAINT [DF_PC_ENVIOS_FechaRecibido] DEFAULT GETUTCDATE();
        PRINT 'Columna PC_ENVIOS.FechaRecibido agregada';
    END
    IF COL_LENGTH('PC_ENVIOS', 'FechaFactura') IS NULL
    BEGIN
        ALTER TABLE [PC_ENVIOS] ADD [FechaFactura] DATETIME2 NULL;
        PRINT 'Columna PC_ENVIOS.FechaFactura agregada';
    END
    IF COL_LENGTH('PC_ENVIOS', 'FechaEnvio') IS NULL
    BEGIN
        ALTER TABLE [PC_ENVIOS] ADD [FechaEnvio] DATETIME2 NULL;
        PRINT 'Columna PC_ENVIOS.FechaEnvio agregada';
    END
    IF COL_LENGTH('PC_ENVIOS', 'FechaEntrega') IS NULL
    BEGIN
        ALTER TABLE [PC_ENVIOS] ADD [FechaEntrega] DATETIME2 NULL;
        PRINT 'Columna PC_ENVIOS.FechaEntrega agregada';
    END
    IF COL_LENGTH('PC_ENVIOS', 'MotivoAnulacion') IS NULL
    BEGIN
        ALTER TABLE [PC_ENVIOS] ADD [MotivoAnulacion] NVARCHAR(255) NULL;
        PRINT 'Columna PC_ENVIOS.MotivoAnulacion agregada';
    END
    IF COL_LENGTH('PC_ENVIOS', 'ClienteId') IS NULL
    BEGIN
        ALTER TABLE [PC_ENVIOS] ADD [ClienteId] INT NOT NULL CONSTRAINT [DF_PC_ENVIOS_ClienteId] DEFAULT 0;
        PRINT 'Columna PC_ENVIOS.ClienteId agregada';
    END
    IF COL_LENGTH('PC_ENVIOS', 'AgenciaId') IS NULL
    BEGIN
        ALTER TABLE [PC_ENVIOS] ADD [AgenciaId] INT NOT NULL CONSTRAINT [DF_PC_ENVIOS_AgenciaId] DEFAULT 0;
        PRINT 'Columna PC_ENVIOS.AgenciaId agregada';
    END
    -- Etapa B (plan §3.6): enlace al pedido. Nullable — los envíos sueltos siguen valiendo.
    IF COL_LENGTH('PC_ENVIOS', 'PedidoId') IS NULL
    BEGIN
        ALTER TABLE [PC_ENVIOS] ADD [PedidoId] INT NULL;
        PRINT 'Columna PC_ENVIOS.PedidoId agregada';
    END
END
