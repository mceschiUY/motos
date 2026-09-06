-- ═══════════════════════════════════════════════════════════════════════════════
-- SISTEMA DE SEGURIDAD ZAS
-- Datos de Prueba (Test Data)
-- ═══════════════════════════════════════════════════════════════════════════════

-- ═══════════════════════════════════════════════════════════════════════════════
-- CAPABILITIES ADICIONALES
-- ═══════════════════════════════════════════════════════════════════════════════

-- Modulo: Producto
INSERT INTO Seg_Capabilities (Nombre, Descripcion, Modulo) VALUES
('Producto.Ver', 'Ver listado y detalle de productos', 'Producto'),
('Producto.Crear', 'Crear nuevos productos', 'Producto'),
('Producto.Modificar', 'Modificar productos existentes', 'Producto'),
('Producto.Eliminar', 'Eliminar productos', 'Producto'),
('Producto.ImportarStock', 'Importar stock masivamente', 'Producto'),
('Producto.AjustarPrecio', 'Ajustar precios de productos', 'Producto');

-- Modulo: Inventario
INSERT INTO Seg_Capabilities (Nombre, Descripcion, Modulo) VALUES
('Inventario.Ver', 'Ver inventario actual', 'Inventario'),
('Inventario.Ajustar', 'Realizar ajustes de inventario', 'Inventario'),
('Inventario.Transferir', 'Transferir entre almacenes', 'Inventario'),
('Inventario.Auditar', 'Realizar auditorias de inventario', 'Inventario');

-- Modulo: Facturacion
INSERT INTO Seg_Capabilities (Nombre, Descripcion, Modulo) VALUES
('Factura.Ver', 'Ver facturas', 'Facturacion'),
('Factura.Crear', 'Crear facturas', 'Facturacion'),
('Factura.Anular', 'Anular facturas', 'Facturacion'),
('Factura.NotaCredito', 'Emitir notas de credito', 'Facturacion'),
('Factura.Exportar', 'Exportar facturas a AFIP/DGI', 'Facturacion');

-- Modulo: Caja
INSERT INTO Seg_Capabilities (Nombre, Descripcion, Modulo) VALUES
('Caja.Ver', 'Ver movimientos de caja', 'Caja'),
('Caja.Abrir', 'Abrir caja', 'Caja'),
('Caja.Cerrar', 'Cerrar caja', 'Caja'),
('Caja.Arqueo', 'Realizar arqueo de caja', 'Caja'),
('Caja.Retiro', 'Realizar retiros de caja', 'Caja');

-- Modulo: Reportes
INSERT INTO Seg_Capabilities (Nombre, Descripcion, Modulo) VALUES
('Reporte.Ventas', 'Ver reportes de ventas', 'Reportes'),
('Reporte.Inventario', 'Ver reportes de inventario', 'Reportes'),
('Reporte.Financiero', 'Ver reportes financieros', 'Reportes'),
('Reporte.Clientes', 'Ver reportes de clientes', 'Reportes'),
('Reporte.Dashboard', 'Ver dashboard gerencial', 'Reportes'),
('Reporte.Exportar', 'Exportar reportes a Excel/PDF', 'Reportes');

-- Modulo: Configuracion
INSERT INTO Seg_Capabilities (Nombre, Descripcion, Modulo) VALUES
('Config.General', 'Configuracion general del sistema', 'Configuracion'),
('Config.Empresa', 'Configurar datos de empresa', 'Configuracion'),
('Config.Impuestos', 'Configurar impuestos', 'Configuracion'),
('Config.Notificaciones', 'Configurar notificaciones', 'Configuracion'),
('Config.Integraciones', 'Configurar integraciones externas', 'Configuracion');

GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- ROLES ADICIONALES
-- ═══════════════════════════════════════════════════════════════════════════════

INSERT INTO Seg_Roles (Nombre, Descripcion) VALUES
('Cajero', 'Rol de cajero - manejo de caja y facturacion basica'),
('Almacenero', 'Rol de almacenero - gestion de inventario'),
('Contador', 'Rol de contador - acceso a reportes financieros'),
('Gerente Comercial', 'Rol de gerente comercial - supervision de ventas'),
('Gerente General', 'Rol de gerente general - acceso completo a reportes'),
('Operador', 'Rol de operador basico - solo operaciones diarias'),
('Auditor', 'Rol de auditor - solo lectura con acceso a auditoria');

GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- ASIGNAR CAPABILITIES A NUEVOS ROLES
-- ═══════════════════════════════════════════════════════════════════════════════

-- Rol: Cajero
INSERT INTO Seg_RolCapabilities (RolId, CapabilityId)
SELECT r.Id, c.Id
FROM Seg_Roles r, Seg_Capabilities c
WHERE r.Nombre = 'Cajero'
AND c.Nombre IN (
    'Cliente.Ver', 'Cliente.Crear',
    'Producto.Ver',
    'Factura.Ver', 'Factura.Crear',
    'Caja.Ver', 'Caja.Abrir', 'Caja.Cerrar'
);

-- Rol: Almacenero
INSERT INTO Seg_RolCapabilities (RolId, CapabilityId)
SELECT r.Id, c.Id
FROM Seg_Roles r, Seg_Capabilities c
WHERE r.Nombre = 'Almacenero'
AND c.Nombre IN (
    'Producto.Ver', 'Producto.Modificar',
    'Inventario.Ver', 'Inventario.Ajustar', 'Inventario.Transferir'
);

-- Rol: Contador
INSERT INTO Seg_RolCapabilities (RolId, CapabilityId)
SELECT r.Id, c.Id
FROM Seg_Roles r, Seg_Capabilities c
WHERE r.Nombre = 'Contador'
AND c.Nombre IN (
    'Factura.Ver', 'Factura.Anular', 'Factura.NotaCredito', 'Factura.Exportar',
    'Caja.Ver', 'Caja.Arqueo',
    'Reporte.Ventas', 'Reporte.Financiero', 'Reporte.Exportar'
);

-- Rol: Gerente Comercial
INSERT INTO Seg_RolCapabilities (RolId, CapabilityId)
SELECT r.Id, c.Id
FROM Seg_Roles r, Seg_Capabilities c
WHERE r.Nombre = 'Gerente Comercial'
AND c.Nombre IN (
    'Lead.Ver', 'Lead.Crear', 'Lead.Modificar', 'Lead.Eliminar', 'Lead.Asignar', 'Lead.Convertir',
    'Cliente.Ver', 'Cliente.Crear', 'Cliente.Modificar', 'Cliente.Eliminar',
    'Pedido.Ver', 'Pedido.Crear', 'Pedido.Modificar', 'Pedido.Aprobar', 'Pedido.Cancelar',
    'Producto.Ver', 'Producto.AjustarPrecio',
    'Reporte.Ventas', 'Reporte.Clientes', 'Reporte.Dashboard', 'Reporte.Exportar'
);

-- Rol: Gerente General
INSERT INTO Seg_RolCapabilities (RolId, CapabilityId)
SELECT r.Id, c.Id
FROM Seg_Roles r, Seg_Capabilities c
WHERE r.Nombre = 'Gerente General'
AND c.Modulo IN ('Reportes', 'Configuracion');

-- Agregar caps adicionales al Gerente General
INSERT INTO Seg_RolCapabilities (RolId, CapabilityId)
SELECT r.Id, c.Id
FROM Seg_Roles r, Seg_Capabilities c
WHERE r.Nombre = 'Gerente General'
AND c.Nombre IN ('Admin.Full', 'Admin.Reportes');

-- Rol: Operador
INSERT INTO Seg_RolCapabilities (RolId, CapabilityId)
SELECT r.Id, c.Id
FROM Seg_Roles r, Seg_Capabilities c
WHERE r.Nombre = 'Operador'
AND c.Nombre IN (
    'Lead.Ver', 'Lead.Crear', 'Lead.Modificar',
    'Cliente.Ver',
    'Pedido.Ver', 'Pedido.Crear',
    'Producto.Ver'
);

-- Rol: Auditor
INSERT INTO Seg_RolCapabilities (RolId, CapabilityId)
SELECT r.Id, c.Id
FROM Seg_Roles r, Seg_Capabilities c
WHERE r.Nombre = 'Auditor'
AND (c.Nombre LIKE '%.Ver' OR c.Nombre IN ('Inventario.Auditar', 'Caja.Arqueo', 'Reporte.Exportar'));

GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- PERFILES ADICIONALES
-- ═══════════════════════════════════════════════════════════════════════════════

INSERT INTO Seg_Perfiles (Nombre, Descripcion) VALUES
('Perfil Cajero', 'Perfil para cajeros - caja y facturacion'),
('Perfil Almacen', 'Perfil para personal de almacen'),
('Perfil Contabilidad', 'Perfil para area contable'),
('Perfil Gerencia Comercial', 'Perfil para gerentes comerciales'),
('Perfil Gerencia General', 'Perfil para gerencia general - acceso total'),
('Perfil Operador', 'Perfil para operadores basicos'),
('Perfil Auditor', 'Perfil para auditores internos/externos');

GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- ASIGNAR ROLES A NUEVOS PERFILES
-- ═══════════════════════════════════════════════════════════════════════════════

-- Perfil Cajero = Rol Cajero
INSERT INTO Seg_PerfilRoles (PerfilId, RolId)
SELECT p.Id, r.Id
FROM Seg_Perfiles p, Seg_Roles r
WHERE p.Nombre = 'Perfil Cajero' AND r.Nombre = 'Cajero';

-- Perfil Almacen = Rol Almacenero
INSERT INTO Seg_PerfilRoles (PerfilId, RolId)
SELECT p.Id, r.Id
FROM Seg_Perfiles p, Seg_Roles r
WHERE p.Nombre = 'Perfil Almacen' AND r.Nombre = 'Almacenero';

-- Perfil Contabilidad = Rol Contador
INSERT INTO Seg_PerfilRoles (PerfilId, RolId)
SELECT p.Id, r.Id
FROM Seg_Perfiles p, Seg_Roles r
WHERE p.Nombre = 'Perfil Contabilidad' AND r.Nombre = 'Contador';

-- Perfil Gerencia Comercial = Roles Gerente Comercial + Supervisor
INSERT INTO Seg_PerfilRoles (PerfilId, RolId)
SELECT p.Id, r.Id
FROM Seg_Perfiles p, Seg_Roles r
WHERE p.Nombre = 'Perfil Gerencia Comercial' AND r.Nombre IN ('Gerente Comercial', 'Supervisor');

-- Perfil Gerencia General = Roles Gerente General + Administrador
INSERT INTO Seg_PerfilRoles (PerfilId, RolId)
SELECT p.Id, r.Id
FROM Seg_Perfiles p, Seg_Roles r
WHERE p.Nombre = 'Perfil Gerencia General' AND r.Nombre IN ('Gerente General', 'Administrador');

-- Perfil Operador = Rol Operador
INSERT INTO Seg_PerfilRoles (PerfilId, RolId)
SELECT p.Id, r.Id
FROM Seg_Perfiles p, Seg_Roles r
WHERE p.Nombre = 'Perfil Operador' AND r.Nombre = 'Operador';

-- Perfil Auditor = Rol Auditor
INSERT INTO Seg_PerfilRoles (PerfilId, RolId)
SELECT p.Id, r.Id
FROM Seg_Perfiles p, Seg_Roles r
WHERE p.Nombre = 'Perfil Auditor' AND r.Nombre = 'Auditor';

GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- USUARIOS DE PRUEBA
-- Password para todos: Test123! (hash BCrypt)
-- ═══════════════════════════════════════════════════════════════════════════════

-- Usuario: vendedor1 (Perfil Comercial)
INSERT INTO Seg_Usuarios (UserName, Email, NombreCompleto, PasswordHash, PerfilId)
SELECT 'vendedor1', 'juan.perez@empresa.com', 'Juan Perez',
       '$2a$11$K7bHG8vKq5Y6zL4Jn3mQxOq.HvRlRbQz5nXlYkJ8LuN9K2Dn5MQHi', -- Test123!
       p.Id
FROM Seg_Perfiles p WHERE p.Nombre = 'Perfil Comercial';

-- Usuario: vendedor2 (Perfil Comercial)
INSERT INTO Seg_Usuarios (UserName, Email, NombreCompleto, PasswordHash, PerfilId)
SELECT 'vendedor2', 'maria.garcia@empresa.com', 'Maria Garcia',
       '$2a$11$K7bHG8vKq5Y6zL4Jn3mQxOq.HvRlRbQz5nXlYkJ8LuN9K2Dn5MQHi',
       p.Id
FROM Seg_Perfiles p WHERE p.Nombre = 'Perfil Comercial';

-- Usuario: vendedor3 (Perfil Comercial)
INSERT INTO Seg_Usuarios (UserName, Email, NombreCompleto, PasswordHash, PerfilId)
SELECT 'vendedor3', 'carlos.rodriguez@empresa.com', 'Carlos Rodriguez',
       '$2a$11$K7bHG8vKq5Y6zL4Jn3mQxOq.HvRlRbQz5nXlYkJ8LuN9K2Dn5MQHi',
       p.Id
FROM Seg_Perfiles p WHERE p.Nombre = 'Perfil Comercial';

-- Usuario: supervisor1 (Perfil Supervisor Comercial)
INSERT INTO Seg_Usuarios (UserName, Email, NombreCompleto, PasswordHash, PerfilId)
SELECT 'supervisor1', 'ana.martinez@empresa.com', 'Ana Martinez',
       '$2a$11$K7bHG8vKq5Y6zL4Jn3mQxOq.HvRlRbQz5nXlYkJ8LuN9K2Dn5MQHi',
       p.Id
FROM Seg_Perfiles p WHERE p.Nombre = 'Perfil Supervisor Comercial';

-- Usuario: cajero1 (Perfil Cajero)
INSERT INTO Seg_Usuarios (UserName, Email, NombreCompleto, PasswordHash, PerfilId)
SELECT 'cajero1', 'pedro.sanchez@empresa.com', 'Pedro Sanchez',
       '$2a$11$K7bHG8vKq5Y6zL4Jn3mQxOq.HvRlRbQz5nXlYkJ8LuN9K2Dn5MQHi',
       p.Id
FROM Seg_Perfiles p WHERE p.Nombre = 'Perfil Cajero';

-- Usuario: cajero2 (Perfil Cajero)
INSERT INTO Seg_Usuarios (UserName, Email, NombreCompleto, PasswordHash, PerfilId)
SELECT 'cajero2', 'lucia.fernandez@empresa.com', 'Lucia Fernandez',
       '$2a$11$K7bHG8vKq5Y6zL4Jn3mQxOq.HvRlRbQz5nXlYkJ8LuN9K2Dn5MQHi',
       p.Id
FROM Seg_Perfiles p WHERE p.Nombre = 'Perfil Cajero';

-- Usuario: almacen1 (Perfil Almacen)
INSERT INTO Seg_Usuarios (UserName, Email, NombreCompleto, PasswordHash, PerfilId)
SELECT 'almacen1', 'roberto.lopez@empresa.com', 'Roberto Lopez',
       '$2a$11$K7bHG8vKq5Y6zL4Jn3mQxOq.HvRlRbQz5nXlYkJ8LuN9K2Dn5MQHi',
       p.Id
FROM Seg_Perfiles p WHERE p.Nombre = 'Perfil Almacen';

-- Usuario: contador1 (Perfil Contabilidad)
INSERT INTO Seg_Usuarios (UserName, Email, NombreCompleto, PasswordHash, PerfilId)
SELECT 'contador1', 'patricia.gomez@empresa.com', 'Patricia Gomez',
       '$2a$11$K7bHG8vKq5Y6zL4Jn3mQxOq.HvRlRbQz5nXlYkJ8LuN9K2Dn5MQHi',
       p.Id
FROM Seg_Perfiles p WHERE p.Nombre = 'Perfil Contabilidad';

-- Usuario: gerente.comercial (Perfil Gerencia Comercial)
INSERT INTO Seg_Usuarios (UserName, Email, NombreCompleto, PasswordHash, PerfilId)
SELECT 'gerente.comercial', 'fernando.diaz@empresa.com', 'Fernando Diaz',
       '$2a$11$K7bHG8vKq5Y6zL4Jn3mQxOq.HvRlRbQz5nXlYkJ8LuN9K2Dn5MQHi',
       p.Id
FROM Seg_Perfiles p WHERE p.Nombre = 'Perfil Gerencia Comercial';

-- Usuario: gerente.general (Perfil Gerencia General)
INSERT INTO Seg_Usuarios (UserName, Email, NombreCompleto, PasswordHash, PerfilId)
SELECT 'gerente.general', 'martin.ruiz@empresa.com', 'Martin Ruiz',
       '$2a$11$K7bHG8vKq5Y6zL4Jn3mQxOq.HvRlRbQz5nXlYkJ8LuN9K2Dn5MQHi',
       p.Id
FROM Seg_Perfiles p WHERE p.Nombre = 'Perfil Gerencia General';

-- Usuario: operador1 (Perfil Operador)
INSERT INTO Seg_Usuarios (UserName, Email, NombreCompleto, PasswordHash, PerfilId)
SELECT 'operador1', 'sofia.torres@empresa.com', 'Sofia Torres',
       '$2a$11$K7bHG8vKq5Y6zL4Jn3mQxOq.HvRlRbQz5nXlYkJ8LuN9K2Dn5MQHi',
       p.Id
FROM Seg_Perfiles p WHERE p.Nombre = 'Perfil Operador';

-- Usuario: operador2 (Perfil Operador)
INSERT INTO Seg_Usuarios (UserName, Email, NombreCompleto, PasswordHash, PerfilId)
SELECT 'operador2', 'diego.morales@empresa.com', 'Diego Morales',
       '$2a$11$K7bHG8vKq5Y6zL4Jn3mQxOq.HvRlRbQz5nXlYkJ8LuN9K2Dn5MQHi',
       p.Id
FROM Seg_Perfiles p WHERE p.Nombre = 'Perfil Operador';

-- Usuario: auditor1 (Perfil Auditor)
INSERT INTO Seg_Usuarios (UserName, Email, NombreCompleto, PasswordHash, PerfilId)
SELECT 'auditor1', 'gabriela.silva@empresa.com', 'Gabriela Silva',
       '$2a$11$K7bHG8vKq5Y6zL4Jn3mQxOq.HvRlRbQz5nXlYkJ8LuN9K2Dn5MQHi',
       p.Id
FROM Seg_Perfiles p WHERE p.Nombre = 'Perfil Auditor';

-- Usuario: consulta1 (Perfil Consulta - solo lectura)
INSERT INTO Seg_Usuarios (UserName, Email, NombreCompleto, PasswordHash, PerfilId)
SELECT 'consulta1', 'invitado@empresa.com', 'Usuario Invitado',
       '$2a$11$K7bHG8vKq5Y6zL4Jn3mQxOq.HvRlRbQz5nXlYkJ8LuN9K2Dn5MQHi',
       p.Id
FROM Seg_Perfiles p WHERE p.Nombre = 'Perfil Consulta';

-- Usuario bloqueado para pruebas
INSERT INTO Seg_Usuarios (UserName, Email, NombreCompleto, PasswordHash, PerfilId, Activo, IntentosFallidos, BloqueadoHasta)
SELECT 'bloqueado', 'bloqueado@empresa.com', 'Usuario Bloqueado',
       '$2a$11$K7bHG8vKq5Y6zL4Jn3mQxOq.HvRlRbQz5nXlYkJ8LuN9K2Dn5MQHi',
       p.Id, 1, 5, DATEADD(HOUR, 1, GETDATE())
FROM Seg_Perfiles p WHERE p.Nombre = 'Perfil Operador';

-- Usuario inactivo para pruebas
INSERT INTO Seg_Usuarios (UserName, Email, NombreCompleto, PasswordHash, PerfilId, Activo)
SELECT 'inactivo', 'inactivo@empresa.com', 'Usuario Inactivo',
       '$2a$11$K7bHG8vKq5Y6zL4Jn3mQxOq.HvRlRbQz5nXlYkJ8LuN9K2Dn5MQHi',
       p.Id, 0
FROM Seg_Perfiles p WHERE p.Nombre = 'Perfil Comercial';

GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- RESUMEN DE DATOS DE PRUEBA
-- ═══════════════════════════════════════════════════════════════════════════════

PRINT '═══════════════════════════════════════════════════════════════════════════════';
PRINT 'DATOS DE PRUEBA - SISTEMA DE SEGURIDAD ZAS';
PRINT '═══════════════════════════════════════════════════════════════════════════════';
PRINT '';
PRINT 'CAPABILITIES AGREGADAS:';
PRINT '  - Modulo Producto: 6 capabilities';
PRINT '  - Modulo Inventario: 4 capabilities';
PRINT '  - Modulo Facturacion: 5 capabilities';
PRINT '  - Modulo Caja: 5 capabilities';
PRINT '  - Modulo Reportes: 6 capabilities';
PRINT '  - Modulo Configuracion: 5 capabilities';
PRINT '';
PRINT 'ROLES AGREGADOS:';
PRINT '  - Cajero, Almacenero, Contador';
PRINT '  - Gerente Comercial, Gerente General';
PRINT '  - Operador, Auditor';
PRINT '';
PRINT 'PERFILES AGREGADOS:';
PRINT '  - Perfil Cajero, Perfil Almacen';
PRINT '  - Perfil Contabilidad, Perfil Gerencia Comercial';
PRINT '  - Perfil Gerencia General, Perfil Operador';
PRINT '  - Perfil Auditor';
PRINT '';
PRINT 'USUARIOS DE PRUEBA (Password: Test123!):';
PRINT '  - vendedor1, vendedor2, vendedor3 (Perfil Comercial)';
PRINT '  - supervisor1 (Perfil Supervisor Comercial)';
PRINT '  - cajero1, cajero2 (Perfil Cajero)';
PRINT '  - almacen1 (Perfil Almacen)';
PRINT '  - contador1 (Perfil Contabilidad)';
PRINT '  - gerente.comercial (Perfil Gerencia Comercial)';
PRINT '  - gerente.general (Perfil Gerencia General)';
PRINT '  - operador1, operador2 (Perfil Operador)';
PRINT '  - auditor1 (Perfil Auditor)';
PRINT '  - consulta1 (Perfil Consulta)';
PRINT '  - bloqueado (Usuario bloqueado para pruebas)';
PRINT '  - inactivo (Usuario inactivo para pruebas)';
PRINT '';
PRINT '═══════════════════════════════════════════════════════════════════════════════';
GO
