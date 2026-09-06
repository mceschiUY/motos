CREATE TABLE RT_Clientes (
    Id INT IDENTITY(1,1) PRIMARY KEY, -- Clave primaria incremental
    Nombre NVARCHAR(100) NOT NULL,   -- Nombre, obligatorio, hasta 100 caracteres
    Direccion NVARCHAR(200) NULL,    -- Dirección, opcional, hasta 200 caracteres
    RUT INT NOT NULL,                -- RUT, obligatorio
    Observaciones NVARCHAR(500) NULL -- Observaciones, opcional, hasta 500 caracteres
);

GO 

CREATE TABLE RT_OPORTUNIDADES (
    Id INT IDENTITY(1,1) PRIMARY KEY, -- Clave primaria con incremento automático
    Nombre NVARCHAR(100) NOT NULL, -- Nombre con un tamaño máximo de 100 caracteres
    ClienteId INT NOT NULL, -- ID del cliente
    Fecha DATETIME NOT NULL, -- Fecha de la oportunidad
    EstadoOportunidadId INT NOT NULL -- Estado de la oportunidad
);

GO 

CREATE TABLE RT_SolicitudesDePrecios (
    Id INT IDENTITY(1,1) PRIMARY KEY, -- Columna de identidad como clave primaria
    OportunidadId INT NOT NULL,       -- ID de la oportunidad asociada
    Descripcion NVARCHAR(255) NOT NULL, -- Descripción de la solicitud
    ProveedorId INT NOT NULL,         -- ID del proveedor asociado
    Plazo INT NOT NULL,               -- Plazo en días
    IdEstadoSolicitud INT NOT NULL,   -- ID del estado de la solicitud
    FechaSolicitud DATETIME NOT NULL, -- Fecha de creación de la solicitud
   
);

GO 

CREATE TABLE RT_Documentos (
    id INT IDENTITY(1,1) PRIMARY KEY, -- Clave primaria auto incremental
    nombre NVARCHAR(255) NOT NULL, -- Nombre del documento
    ruta NVARCHAR(MAX) NOT NULL, -- Ruta del documento
    FechaCreacion DATETIME NOT NULL, -- Fecha de creación
    EstadoDocumento INT NOT NULL -- Estado del documento
);