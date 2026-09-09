-- ============================================================================
-- LIMPIEZA de datos de DOMINIO (a pedido, NUNCA automática).
-- Vacía todas las tablas PC_* del negocio y reinicia sus identidades para que
-- Seed_Dominio_Motos.sql vuelva a sembrar en el próximo arranque (DbBootstrap
-- en Development) o corriendo el seed a mano.
-- CONSERVA: seguridad (Seg_*), configuración del sitio (Cfg_*), reportes,
-- mutaciones y auditoría (RT_*), Evolution (Evo_*).
-- Orden de borrado = orden inverso de dependencias (FKs).
-- Uso: sqlcmd -S localhost\SQLEXPRESS -E -d Motos -C -i Limpiar_Datos_Dominio.sql
-- ============================================================================
SET NOCOUNT ON;
-- sqlcmd arranca con QUOTED_IDENTIFIER OFF y la base tiene índices filtrados/únicos
-- creados por EF: sin esto el DELETE falla con Msg 1934.
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

BEGIN TRANSACTION;

-- Etapa A (fuerza de ventas): hijos de Cliente/Vendedor primero; Vendedor después de Cliente
-- (PC_CLIENTES.VendedorId). Guardadas por existencia: bases anteriores a la Etapa A no las tienen.
IF OBJECT_ID('PC_ACTIVIDADES', 'U') IS NOT NULL DELETE FROM PC_ACTIVIDADES;
IF OBJECT_ID('PC_METAS', 'U') IS NOT NULL DELETE FROM PC_METAS;
DELETE FROM PC_OBSERVACIONES;
DELETE FROM PC_ENVIOS;
DELETE FROM PC_MOVIMIENTOS_STOCK;
DELETE FROM PC_VARIANTES;
DELETE FROM PC_PRODUCTOS;
DELETE FROM PC_CATEGORIAS;
DELETE FROM PC_MARCAS;
DELETE FROM PC_TALLAS;
DELETE FROM PC_COLORES;
DELETE FROM PC_DEPOSITOS;
DELETE FROM PC_CLIENTES;
IF OBJECT_ID('PC_VENDEDORES', 'U') IS NOT NULL DELETE FROM PC_VENDEDORES;
DELETE FROM PC_AGENCIAS;
DELETE FROM PC_PARAMETROSLAS;
IF OBJECT_ID('PC_DOCUMENTOS', 'U') IS NOT NULL DELETE FROM PC_DOCUMENTOS;

DECLARE @t NVARCHAR(128);
DECLARE cur CURSOR LOCAL FAST_FORWARD FOR
    SELECT name FROM sys.tables
    WHERE name IN ('PC_ACTIVIDADES','PC_METAS','PC_OBSERVACIONES','PC_ENVIOS','PC_MOVIMIENTOS_STOCK',
                   'PC_VARIANTES','PC_PRODUCTOS','PC_CATEGORIAS','PC_MARCAS','PC_TALLAS','PC_COLORES',
                   'PC_DEPOSITOS','PC_CLIENTES','PC_VENDEDORES','PC_AGENCIAS','PC_PARAMETROSLAS','PC_DOCUMENTOS')
      AND OBJECTPROPERTY(object_id, 'TableHasIdentity') = 1;
OPEN cur;
FETCH NEXT FROM cur INTO @t;
WHILE @@FETCH_STATUS = 0
BEGIN
    DBCC CHECKIDENT (@t, RESEED, 0) WITH NO_INFOMSGS;
    FETCH NEXT FROM cur INTO @t;
END
CLOSE cur; DEALLOCATE cur;

COMMIT TRANSACTION;

PRINT 'Datos de dominio eliminados. Seguridad, configuración y auditoría intactas.';
PRINT 'Próximo arranque de la API en Development vuelve a sembrar Seed_Dominio_Motos.sql.';
GO
