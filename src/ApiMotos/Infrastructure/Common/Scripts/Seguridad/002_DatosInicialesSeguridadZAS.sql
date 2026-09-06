-- ═══════════════════════════════════════════════════════════════════════════════
-- SISTEMA DE SEGURIDAD ZAS
-- Datos iniciales (Seed)
-- ═══════════════════════════════════════════════════════════════════════════════

-- ═══════════════════════════════════════════════════════════════════════════════
-- CAPABILITIES BASE
-- ═══════════════════════════════════════════════════════════════════════════════

-- Módulo: Lead
INSERT INTO Seg_Capabilities (Nombre, Descripcion, Modulo) VALUES
('Lead.Ver', 'Ver listado y detalle de leads', 'Lead'),
('Lead.Crear', 'Crear nuevos leads', 'Lead'),
('Lead.Modificar', 'Modificar leads existentes', 'Lead'),
('Lead.Eliminar', 'Eliminar leads', 'Lead'),
('Lead.Asignar', 'Asignar leads a vendedores', 'Lead'),
('Lead.Convertir', 'Convertir lead a cliente', 'Lead');

-- Módulo: Cliente
INSERT INTO Seg_Capabilities (Nombre, Descripcion, Modulo) VALUES
('Cliente.Ver', 'Ver listado y detalle de clientes', 'Cliente'),
('Cliente.Crear', 'Crear nuevos clientes', 'Cliente'),
('Cliente.Modificar', 'Modificar clientes existentes', 'Cliente'),
('Cliente.Eliminar', 'Eliminar clientes', 'Cliente');

-- Módulo: Pedido
INSERT INTO Seg_Capabilities (Nombre, Descripcion, Modulo) VALUES
('Pedido.Ver', 'Ver listado y detalle de pedidos', 'Pedido'),
('Pedido.Crear', 'Crear nuevos pedidos', 'Pedido'),
('Pedido.Modificar', 'Modificar pedidos existentes', 'Pedido'),
('Pedido.Eliminar', 'Eliminar pedidos', 'Pedido'),
('Pedido.Aprobar', 'Aprobar pedidos', 'Pedido'),
('Pedido.Cancelar', 'Cancelar pedidos', 'Pedido');

-- Módulo: Usuario (Seguridad)
INSERT INTO Seg_Capabilities (Nombre, Descripcion, Modulo) VALUES
('Usuario.Ver', 'Ver listado y detalle de usuarios', 'Usuario'),
('Usuario.Crear', 'Crear nuevos usuarios', 'Usuario'),
('Usuario.Modificar', 'Modificar usuarios existentes', 'Usuario'),
('Usuario.Eliminar', 'Eliminar usuarios', 'Usuario'),
('Usuario.CambiarPerfil', 'Cambiar perfil de usuarios', 'Usuario'),
('Usuario.Desbloquear', 'Desbloquear usuarios bloqueados', 'Usuario');

-- Módulo: Perfil (Seguridad)
INSERT INTO Seg_Capabilities (Nombre, Descripcion, Modulo) VALUES
('Perfil.Ver', 'Ver listado y detalle de perfiles', 'Perfil'),
('Perfil.Crear', 'Crear nuevos perfiles', 'Perfil'),
('Perfil.Modificar', 'Modificar perfiles existentes', 'Perfil'),
('Perfil.Eliminar', 'Eliminar perfiles', 'Perfil');

-- Módulo: Rol (Seguridad)
INSERT INTO Seg_Capabilities (Nombre, Descripcion, Modulo) VALUES
('Rol.Ver', 'Ver listado y detalle de roles', 'Rol'),
('Rol.Crear', 'Crear nuevos roles', 'Rol'),
('Rol.Modificar', 'Modificar roles existentes', 'Rol'),
('Rol.Eliminar', 'Eliminar roles', 'Rol');

-- Módulo: Admin (Super capacidades)
INSERT INTO Seg_Capabilities (Nombre, Descripcion, Modulo) VALUES
('Admin.Full', 'Acceso completo al sistema', 'Admin'),
('Admin.Config', 'Configurar parámetros del sistema', 'Admin'),
('Admin.Reportes', 'Acceso a reportes administrativos', 'Admin');

GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- ROLES BASE
-- ═══════════════════════════════════════════════════════════════════════════════

INSERT INTO Seg_Roles (Nombre, Descripcion) VALUES
('Vendedor', 'Rol básico de vendedor - gestión de leads y clientes'),
('Supervisor', 'Rol de supervisor - puede asignar leads y aprobar pedidos'),
('Administrador', 'Rol de administrador - gestión completa de seguridad'),
('Solo Lectura', 'Rol de solo lectura - solo puede ver información');

GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- ASIGNAR CAPABILITIES A ROLES
-- ═══════════════════════════════════════════════════════════════════════════════

-- Rol: Vendedor
INSERT INTO Seg_RolCapabilities (RolId, CapabilityId)
SELECT r.Id, c.Id
FROM Seg_Roles r, Seg_Capabilities c
WHERE r.Nombre = 'Vendedor'
AND c.Nombre IN (
    'Lead.Ver', 'Lead.Crear', 'Lead.Modificar', 'Lead.Convertir',
    'Cliente.Ver', 'Cliente.Crear', 'Cliente.Modificar',
    'Pedido.Ver', 'Pedido.Crear', 'Pedido.Modificar'
);

-- Rol: Supervisor (incluye todo lo de Vendedor + extras)
INSERT INTO Seg_RolCapabilities (RolId, CapabilityId)
SELECT r.Id, c.Id
FROM Seg_Roles r, Seg_Capabilities c
WHERE r.Nombre = 'Supervisor'
AND c.Nombre IN (
    'Lead.Ver', 'Lead.Crear', 'Lead.Modificar', 'Lead.Eliminar', 'Lead.Asignar', 'Lead.Convertir',
    'Cliente.Ver', 'Cliente.Crear', 'Cliente.Modificar', 'Cliente.Eliminar',
    'Pedido.Ver', 'Pedido.Crear', 'Pedido.Modificar', 'Pedido.Aprobar', 'Pedido.Cancelar',
    'Usuario.Ver'
);

-- Rol: Administrador (acceso total a seguridad)
INSERT INTO Seg_RolCapabilities (RolId, CapabilityId)
SELECT r.Id, c.Id
FROM Seg_Roles r, Seg_Capabilities c
WHERE r.Nombre = 'Administrador'
AND c.Modulo IN ('Usuario', 'Perfil', 'Rol', 'Admin');

-- Rol: Solo Lectura
INSERT INTO Seg_RolCapabilities (RolId, CapabilityId)
SELECT r.Id, c.Id
FROM Seg_Roles r, Seg_Capabilities c
WHERE r.Nombre = 'Solo Lectura'
AND c.Nombre LIKE '%.Ver';

GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- PERFILES BASE
-- ═══════════════════════════════════════════════════════════════════════════════

INSERT INTO Seg_Perfiles (Nombre, Descripcion) VALUES
('Perfil Comercial', 'Perfil para equipo comercial - vendedores'),
('Perfil Supervisor Comercial', 'Perfil para supervisores de ventas'),
('Perfil Administrador', 'Perfil de administrador del sistema'),
('Perfil Consulta', 'Perfil de solo consulta');

GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- ASIGNAR ROLES A PERFILES
-- ═══════════════════════════════════════════════════════════════════════════════

-- Perfil Comercial = Rol Vendedor
INSERT INTO Seg_PerfilRoles (PerfilId, RolId)
SELECT p.Id, r.Id
FROM Seg_Perfiles p, Seg_Roles r
WHERE p.Nombre = 'Perfil Comercial' AND r.Nombre = 'Vendedor';

-- Perfil Supervisor = Rol Supervisor
INSERT INTO Seg_PerfilRoles (PerfilId, RolId)
SELECT p.Id, r.Id
FROM Seg_Perfiles p, Seg_Roles r
WHERE p.Nombre = 'Perfil Supervisor Comercial' AND r.Nombre = 'Supervisor';

-- Perfil Administrador = Roles Supervisor + Administrador
INSERT INTO Seg_PerfilRoles (PerfilId, RolId)
SELECT p.Id, r.Id
FROM Seg_Perfiles p, Seg_Roles r
WHERE p.Nombre = 'Perfil Administrador' AND r.Nombre IN ('Supervisor', 'Administrador');

-- Perfil Consulta = Rol Solo Lectura
INSERT INTO Seg_PerfilRoles (PerfilId, RolId)
SELECT p.Id, r.Id
FROM Seg_Perfiles p, Seg_Roles r
WHERE p.Nombre = 'Perfil Consulta' AND r.Nombre = 'Solo Lectura';

GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- USUARIO ADMINISTRADOR INICIAL
-- Password: Admin123! (hash BCrypt)
-- ═══════════════════════════════════════════════════════════════════════════════

INSERT INTO Seg_Usuarios (UserName, Email, NombreCompleto, PasswordHash, PerfilId)
SELECT 'admin', 'admin@zas.com', 'Administrador del Sistema',
       '$2a$11$K7bHG8vKq5Y6zL4Jn3mQxOq.HvRlRbQz5nXlYkJ8LuN9K2Dn5MQHi', -- Admin123!
       p.Id
FROM Seg_Perfiles p
WHERE p.Nombre = 'Perfil Administrador';

GO

PRINT '═══════════════════════════════════════════════════════════════════════════════';
PRINT 'SISTEMA DE SEGURIDAD ZAS - Datos iniciales cargados correctamente';
PRINT 'Usuario inicial: admin / Admin123!';
PRINT '═══════════════════════════════════════════════════════════════════════════════';
GO
