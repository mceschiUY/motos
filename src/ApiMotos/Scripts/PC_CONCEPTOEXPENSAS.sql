IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PC_CONCEPTOEXPENSAS]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[PC_CONCEPTOEXPENSAS] (
        [id] INT IDENTITY(1,1) PRIMARY KEY,
        [nombre] NVARCHAR(255) NOT NULL,
        [descripcion] NVARCHAR(255) NOT NULL,
        [tipo] NVARCHAR(255) NOT NULL,
        [activo] BIT NOT NULL,
        [createdat] DATETIME2 NOT NULL DEFAULT GETDATE(),
        [updatedat] DATETIME2 NULL
    );
END
