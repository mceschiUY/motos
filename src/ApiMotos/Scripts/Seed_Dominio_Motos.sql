-- ============================================================================
-- Seed de DOMINIO — datos de demostración Motos (importadora/distribuidora de
-- cascos e indumentaria de moto). Mismo contrato que trenes/Seed_Dominio_DBCC.sql:
--   * Idempotente. NUNCA borra. Para vaciar la base: Limpiar_Datos_Dominio.sql.
--   * Maestras (marcas, categorías, tallas, colores, depósitos, agencias, SLA,
--     vendedores, metas): una por una con IF NOT EXISTS, así conviven con bases ya pobladas.
--   * Transaccionales (productos/variantes/movimientos, clientes/envíos, actividades):
--     por bloque, solo si la tabla cabecera está vacía.
--   * Enriquecimientos de filas existentes (datos comerciales del cliente): UPDATE
--     guardado por "campo todavía NULL", así nunca pisan lo que alguien editó.
--   * Anclado a HOY (GETDATE) para que existencias, Kardex, SLA, agenda y alertas
--     muestren movimiento reciente y alguna alerta visible.
-- Lo ejecutan DbBootstrap (pase Seed_*.sql, solo Development) e init-db.sh (Docker).
-- Diseñado para mostrar: matriz talla×color, curva de tallas, transferencia
-- entre depósitos, un saldo NEGATIVO (SKU sin entrada), un casco con
-- homologación VENCIDA, envíos en todos los estados y uno fuera de SLA.
-- Etapa A (fuerza de ventas, secciones 10-13): 3 vendedores por zona (Litoral, Norte y
-- Montevideo y Este; este último enlazado al login dev 'pablo'), los 8 clientes con tipo,
-- ciudad, contacto, vendedor asignado y coordenadas reales aproximadas (ruta del día),
-- 25 actividades comerciales (23 repartidas dentro del mes en curso + 2 viejas para que
-- 2 clientes disparen la alerta "sin visitar 30+ días"), 9 con ProximaAccion en 7 días
-- (agenda con contenido), ninguna con resultado 'pedido' (Pedido llega en Etapa B), y
-- una meta USD del mes corriente por vendedor.
-- Etapa B (sección 14): 12 pedidos repartidos por todo el ciclo (borrador → entregado
-- y uno anulado) con sus líneas a precio de lista, el stock que salió del depósito por
-- los despachados/entregados, el enlace con los envíos ya sembrados y las visitas que
-- terminaron en pedido (para que la tasa de cierre y las comisiones muestren algo).
-- Etapa C (sección 15): catálogo premium — ficha técnica en 4 productos, 2 destacados y
-- 2 novedades, una foto de portada SVG por cada uno de esos 4 (para que la grilla del
-- catálogo no se vea vacía) y 2 cambios de precio/costo en el historial.
-- NOTA: el esquema real lo crea EF con columnas NOT NULL más estrictas que los
-- PC_*.sql (Marca.Pais, Color.CodigoHex, Cliente.Telefono/DireccionEntrega,
-- Envio.Fecha*/MotivoAnulacion). Por eso se completan siempre todas las columnas
-- y las fechas de etapas no alcanzadas repiten FechaRecibido (así lo guarda la app).
-- ============================================================================
SET NOCOUNT ON;
GO
-- sqlcmd arranca con QUOTED_IDENTIFIER OFF y hay índices filtrados/únicos creados por
-- EF: sin esto los INSERT fallan con Msg 1934. Es opción de sesión: vale para todos los GO.
SET QUOTED_IDENTIFIER ON; SET ANSI_NULLS ON;

-- ─── 1. Marcas ──────────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM PC_MARCAS WHERE Nombre = N'LS2')         INSERT INTO PC_MARCAS (Nombre, Pais, Activo) VALUES (N'LS2', N'España', 1);
IF NOT EXISTS (SELECT 1 FROM PC_MARCAS WHERE Nombre = N'Shoei')       INSERT INTO PC_MARCAS (Nombre, Pais, Activo) VALUES (N'Shoei', N'Japón', 1);
IF NOT EXISTS (SELECT 1 FROM PC_MARCAS WHERE Nombre = N'AGV')         INSERT INTO PC_MARCAS (Nombre, Pais, Activo) VALUES (N'AGV', N'Italia', 1);
IF NOT EXISTS (SELECT 1 FROM PC_MARCAS WHERE Nombre = N'Alpinestars') INSERT INTO PC_MARCAS (Nombre, Pais, Activo) VALUES (N'Alpinestars', N'Italia', 1);
IF NOT EXISTS (SELECT 1 FROM PC_MARCAS WHERE Nombre = N'Fox Racing')  INSERT INTO PC_MARCAS (Nombre, Pais, Activo) VALUES (N'Fox Racing', N'Estados Unidos', 1);
IF NOT EXISTS (SELECT 1 FROM PC_MARCAS WHERE Nombre = N'Bell')        INSERT INTO PC_MARCAS (Nombre, Pais, Activo) VALUES (N'Bell', N'Estados Unidos', 1);
GO

-- ─── 2. Categorías (jerárquicas). Comparación sin tildes ni mayúsculas para
--        convivir con bases sembradas antes con 'Pantalon'. ────────────────────
IF NOT EXISTS (SELECT 1 FROM PC_CATEGORIAS WHERE Nombre COLLATE Latin1_General_CI_AI = N'Casco')        INSERT INTO PC_CATEGORIAS (Nombre, CategoriaPadreId, Activo) VALUES (N'Casco', NULL, 1);
IF NOT EXISTS (SELECT 1 FROM PC_CATEGORIAS WHERE Nombre COLLATE Latin1_General_CI_AI = N'Indumentaria') INSERT INTO PC_CATEGORIAS (Nombre, CategoriaPadreId, Activo) VALUES (N'Indumentaria', NULL, 1);
IF NOT EXISTS (SELECT 1 FROM PC_CATEGORIAS WHERE Nombre COLLATE Latin1_General_CI_AI = N'Accesorios')   INSERT INTO PC_CATEGORIAS (Nombre, CategoriaPadreId, Activo) VALUES (N'Accesorios', NULL, 1);
GO
DECLARE @casco INT = (SELECT TOP 1 Id FROM PC_CATEGORIAS WHERE Nombre COLLATE Latin1_General_CI_AI = N'Casco');
DECLARE @indu  INT = (SELECT TOP 1 Id FROM PC_CATEGORIAS WHERE Nombre COLLATE Latin1_General_CI_AI = N'Indumentaria');
IF @casco IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM PC_CATEGORIAS WHERE Nombre COLLATE Latin1_General_CI_AI = N'Integral') INSERT INTO PC_CATEGORIAS (Nombre, CategoriaPadreId, Activo) VALUES (N'Integral', @casco, 1);
    IF NOT EXISTS (SELECT 1 FROM PC_CATEGORIAS WHERE Nombre COLLATE Latin1_General_CI_AI = N'Modular')  INSERT INTO PC_CATEGORIAS (Nombre, CategoriaPadreId, Activo) VALUES (N'Modular', @casco, 1);
    IF NOT EXISTS (SELECT 1 FROM PC_CATEGORIAS WHERE Nombre COLLATE Latin1_General_CI_AI = N'Jet')      INSERT INTO PC_CATEGORIAS (Nombre, CategoriaPadreId, Activo) VALUES (N'Jet', @casco, 1);
    IF NOT EXISTS (SELECT 1 FROM PC_CATEGORIAS WHERE Nombre COLLATE Latin1_General_CI_AI = N'Cross')    INSERT INTO PC_CATEGORIAS (Nombre, CategoriaPadreId, Activo) VALUES (N'Cross', @casco, 1);
END
IF @indu IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM PC_CATEGORIAS WHERE Nombre COLLATE Latin1_General_CI_AI = N'Campera')    INSERT INTO PC_CATEGORIAS (Nombre, CategoriaPadreId, Activo) VALUES (N'Campera', @indu, 1);
    IF NOT EXISTS (SELECT 1 FROM PC_CATEGORIAS WHERE Nombre COLLATE Latin1_General_CI_AI = N'Guantes')    INSERT INTO PC_CATEGORIAS (Nombre, CategoriaPadreId, Activo) VALUES (N'Guantes', @indu, 1);
    IF NOT EXISTS (SELECT 1 FROM PC_CATEGORIAS WHERE Nombre COLLATE Latin1_General_CI_AI = N'Botas')      INSERT INTO PC_CATEGORIAS (Nombre, CategoriaPadreId, Activo) VALUES (N'Botas', @indu, 1);
    IF NOT EXISTS (SELECT 1 FROM PC_CATEGORIAS WHERE Nombre COLLATE Latin1_General_CI_AI = N'Pantalón')   INSERT INTO PC_CATEGORIAS (Nombre, CategoriaPadreId, Activo) VALUES (N'Pantalón', @indu, 1);
    IF NOT EXISTS (SELECT 1 FROM PC_CATEGORIAS WHERE Nombre COLLATE Latin1_General_CI_AI = N'Protección') INSERT INTO PC_CATEGORIAS (Nombre, CategoriaPadreId, Activo) VALUES (N'Protección', @indu, 1);
END
GO

-- ─── 3. Tallas: alfabéticas (indumentaria), numéricas de casco y de calzado ──
IF NOT EXISTS (SELECT 1 FROM PC_TALLAS WHERE Nombre = N'XS')  INSERT INTO PC_TALLAS (Nombre, Tipo, Orden, Activo) VALUES (N'XS',  N'alfabetica', 1, 1);
IF NOT EXISTS (SELECT 1 FROM PC_TALLAS WHERE Nombre = N'S')   INSERT INTO PC_TALLAS (Nombre, Tipo, Orden, Activo) VALUES (N'S',   N'alfabetica', 2, 1);
IF NOT EXISTS (SELECT 1 FROM PC_TALLAS WHERE Nombre = N'M')   INSERT INTO PC_TALLAS (Nombre, Tipo, Orden, Activo) VALUES (N'M',   N'alfabetica', 3, 1);
IF NOT EXISTS (SELECT 1 FROM PC_TALLAS WHERE Nombre = N'L')   INSERT INTO PC_TALLAS (Nombre, Tipo, Orden, Activo) VALUES (N'L',   N'alfabetica', 4, 1);
IF NOT EXISTS (SELECT 1 FROM PC_TALLAS WHERE Nombre = N'XL')  INSERT INTO PC_TALLAS (Nombre, Tipo, Orden, Activo) VALUES (N'XL',  N'alfabetica', 5, 1);
IF NOT EXISTS (SELECT 1 FROM PC_TALLAS WHERE Nombre = N'XXL') INSERT INTO PC_TALLAS (Nombre, Tipo, Orden, Activo) VALUES (N'XXL', N'alfabetica', 6, 1);
IF NOT EXISTS (SELECT 1 FROM PC_TALLAS WHERE Nombre = N'53-54') INSERT INTO PC_TALLAS (Nombre, Tipo, Orden, Activo) VALUES (N'53-54', N'numerica', 10, 1);
IF NOT EXISTS (SELECT 1 FROM PC_TALLAS WHERE Nombre = N'55-56') INSERT INTO PC_TALLAS (Nombre, Tipo, Orden, Activo) VALUES (N'55-56', N'numerica', 11, 1);
IF NOT EXISTS (SELECT 1 FROM PC_TALLAS WHERE Nombre = N'57-58') INSERT INTO PC_TALLAS (Nombre, Tipo, Orden, Activo) VALUES (N'57-58', N'numerica', 12, 1);
IF NOT EXISTS (SELECT 1 FROM PC_TALLAS WHERE Nombre = N'59-60') INSERT INTO PC_TALLAS (Nombre, Tipo, Orden, Activo) VALUES (N'59-60', N'numerica', 13, 1);
IF NOT EXISTS (SELECT 1 FROM PC_TALLAS WHERE Nombre = N'61-62') INSERT INTO PC_TALLAS (Nombre, Tipo, Orden, Activo) VALUES (N'61-62', N'numerica', 14, 1);
IF NOT EXISTS (SELECT 1 FROM PC_TALLAS WHERE Nombre = N'63-64') INSERT INTO PC_TALLAS (Nombre, Tipo, Orden, Activo) VALUES (N'63-64', N'numerica', 15, 1);
IF NOT EXISTS (SELECT 1 FROM PC_TALLAS WHERE Nombre = N'40') INSERT INTO PC_TALLAS (Nombre, Tipo, Orden, Activo) VALUES (N'40', N'numerica', 20, 1);
IF NOT EXISTS (SELECT 1 FROM PC_TALLAS WHERE Nombre = N'41') INSERT INTO PC_TALLAS (Nombre, Tipo, Orden, Activo) VALUES (N'41', N'numerica', 21, 1);
IF NOT EXISTS (SELECT 1 FROM PC_TALLAS WHERE Nombre = N'42') INSERT INTO PC_TALLAS (Nombre, Tipo, Orden, Activo) VALUES (N'42', N'numerica', 22, 1);
IF NOT EXISTS (SELECT 1 FROM PC_TALLAS WHERE Nombre = N'43') INSERT INTO PC_TALLAS (Nombre, Tipo, Orden, Activo) VALUES (N'43', N'numerica', 23, 1);
IF NOT EXISTS (SELECT 1 FROM PC_TALLAS WHERE Nombre = N'44') INSERT INTO PC_TALLAS (Nombre, Tipo, Orden, Activo) VALUES (N'44', N'numerica', 24, 1);
IF NOT EXISTS (SELECT 1 FROM PC_TALLAS WHERE Nombre = N'45') INSERT INTO PC_TALLAS (Nombre, Tipo, Orden, Activo) VALUES (N'45', N'numerica', 25, 1);
GO

-- ─── 4. Colores ─────────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM PC_COLORES WHERE Nombre = N'Negro')         INSERT INTO PC_COLORES (Nombre, CodigoHex, Activo) VALUES (N'Negro',         N'#000000', 1);
IF NOT EXISTS (SELECT 1 FROM PC_COLORES WHERE Nombre = N'Negro mate')    INSERT INTO PC_COLORES (Nombre, CodigoHex, Activo) VALUES (N'Negro mate',    N'#1A1A1A', 1);
IF NOT EXISTS (SELECT 1 FROM PC_COLORES WHERE Nombre = N'Blanco')        INSERT INTO PC_COLORES (Nombre, CodigoHex, Activo) VALUES (N'Blanco',        N'#FFFFFF', 1);
IF NOT EXISTS (SELECT 1 FROM PC_COLORES WHERE Nombre = N'Rojo')          INSERT INTO PC_COLORES (Nombre, CodigoHex, Activo) VALUES (N'Rojo',          N'#E11D2A', 1);
IF NOT EXISTS (SELECT 1 FROM PC_COLORES WHERE Nombre = N'Azul')          INSERT INTO PC_COLORES (Nombre, CodigoHex, Activo) VALUES (N'Azul',          N'#1E5AA8', 1);
IF NOT EXISTS (SELECT 1 FROM PC_COLORES WHERE Nombre = N'Gris')          INSERT INTO PC_COLORES (Nombre, CodigoHex, Activo) VALUES (N'Gris',          N'#808080', 1);
IF NOT EXISTS (SELECT 1 FROM PC_COLORES WHERE Nombre = N'Amarillo fluo') INSERT INTO PC_COLORES (Nombre, CodigoHex, Activo) VALUES (N'Amarillo fluo', N'#E8FF00', 1);
GO

-- ─── 5. Depósitos ───────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM PC_DEPOSITOS WHERE Codigo = N'DEP-CENTRAL') INSERT INTO PC_DEPOSITOS (Codigo, Nombre, Direccion, Activo) VALUES (N'DEP-CENTRAL', N'Depósito Central', N'Camino Carrasco 4500, Montevideo', 1);
IF NOT EXISTS (SELECT 1 FROM PC_DEPOSITOS WHERE Codigo = N'DEP-SHOW')    INSERT INTO PC_DEPOSITOS (Codigo, Nombre, Direccion, Activo) VALUES (N'DEP-SHOW',    N'Showroom Centro',  N'Av. 18 de Julio 1200, Montevideo', 1);
GO

-- ─── 6. Parámetros SLA por etapa (advertencia / límite, en días) ────────────
IF NOT EXISTS (SELECT 1 FROM PC_PARAMETROSLAS WHERE Etapa = N'facturacion') INSERT INTO PC_PARAMETROSLAS (Etapa, RangoAlertaUmbralAdvertenciaDias, RangoAlertaLimiteDias) VALUES (N'facturacion', 2, 4);
IF NOT EXISTS (SELECT 1 FROM PC_PARAMETROSLAS WHERE Etapa = N'despacho')    INSERT INTO PC_PARAMETROSLAS (Etapa, RangoAlertaUmbralAdvertenciaDias, RangoAlertaLimiteDias) VALUES (N'despacho',    1, 3);
IF NOT EXISTS (SELECT 1 FROM PC_PARAMETROSLAS WHERE Etapa = N'entrega')     INSERT INTO PC_PARAMETROSLAS (Etapa, RangoAlertaUmbralAdvertenciaDias, RangoAlertaLimiteDias) VALUES (N'entrega',     3, 7);
GO

-- ─── 7. Agencias (transportistas) ───────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM PC_AGENCIAS WHERE Nombre = N'DAC')             INSERT INTO PC_AGENCIAS (Nombre) VALUES (N'DAC');
IF NOT EXISTS (SELECT 1 FROM PC_AGENCIAS WHERE Nombre = N'Mirtrans')        INSERT INTO PC_AGENCIAS (Nombre) VALUES (N'Mirtrans');
IF NOT EXISTS (SELECT 1 FROM PC_AGENCIAS WHERE Nombre = N'Flota propia')    INSERT INTO PC_AGENCIAS (Nombre) VALUES (N'Flota propia');
GO

-- ============================================================================
-- 8. Catálogo transaccional: productos + matriz de variantes + Kardex.
--    Solo si no hay productos (bloque atómico: si hay uno, se asume sembrado).
-- ============================================================================
IF NOT EXISTS (SELECT 1 FROM PC_PRODUCTOS)
BEGIN
    DECLARE @hoy DATE = CAST(GETDATE() AS DATE);
    DECLARE @mLS2 INT = (SELECT TOP 1 Id FROM PC_MARCAS WHERE Nombre = N'LS2');
    DECLARE @mSho INT = (SELECT TOP 1 Id FROM PC_MARCAS WHERE Nombre = N'Shoei');
    DECLARE @mAGV INT = (SELECT TOP 1 Id FROM PC_MARCAS WHERE Nombre = N'AGV');
    DECLARE @mAlp INT = (SELECT TOP 1 Id FROM PC_MARCAS WHERE Nombre = N'Alpinestars');
    DECLARE @mFox INT = (SELECT TOP 1 Id FROM PC_MARCAS WHERE Nombre = N'Fox Racing');
    DECLARE @mBel INT = (SELECT TOP 1 Id FROM PC_MARCAS WHERE Nombre = N'Bell');
    DECLARE @cInt INT = (SELECT TOP 1 Id FROM PC_CATEGORIAS WHERE Nombre COLLATE Latin1_General_CI_AI = N'Integral');
    DECLARE @cMod INT = (SELECT TOP 1 Id FROM PC_CATEGORIAS WHERE Nombre COLLATE Latin1_General_CI_AI = N'Modular');
    DECLARE @cCro INT = (SELECT TOP 1 Id FROM PC_CATEGORIAS WHERE Nombre COLLATE Latin1_General_CI_AI = N'Cross');
    DECLARE @cCam INT = (SELECT TOP 1 Id FROM PC_CATEGORIAS WHERE Nombre COLLATE Latin1_General_CI_AI = N'Campera');
    DECLARE @cGua INT = (SELECT TOP 1 Id FROM PC_CATEGORIAS WHERE Nombre COLLATE Latin1_General_CI_AI = N'Guantes');
    DECLARE @cBot INT = (SELECT TOP 1 Id FROM PC_CATEGORIAS WHERE Nombre COLLATE Latin1_General_CI_AI = N'Botas');
    DECLARE @cPan INT = (SELECT TOP 1 Id FROM PC_CATEGORIAS WHERE Nombre COLLATE Latin1_General_CI_AI = N'Pantalón');
    DECLARE @cAcc INT = (SELECT TOP 1 Id FROM PC_CATEGORIAS WHERE Nombre COLLATE Latin1_General_CI_AI = N'Accesorios');

    -- 8.1 Productos (modelos). Cascos con homologación; uno VENCIDO a propósito (AGV K1 S).
    INSERT INTO PC_PRODUCTOS (Codigo, Nombre, MarcaId, CategoriaId, Descripcion, Genero, Temporada, Material, PesoGramos, TipoCasco, Homologacion, HomologacionVigente, FechaVencHomologacion, Activo) VALUES
        (N'LS2-FF800',  N'Casco Integral LS2 FF800 Storm II',      @mLS2, @cInt, N'Integral de policarbonato KPA, visera interna solar, Pinlock incluido.', N'unisex', NULL, N'Policarbonato KPA', 1550, N'integral', N'ece2206', 1, DATEADD(YEAR, 3, @hoy), 1),
        (N'SHO-NXR2',   N'Casco Integral Shoei NXR2',              @mSho, @cInt, N'Integral premium en fibra AIM, ventilación de competición.',            N'unisex', NULL, N'Fibra AIM',         1350, N'integral', N'ece2206', 1, DATEADD(YEAR, 4, @hoy), 1),
        (N'LS2-FF906',  N'Casco Modular LS2 FF906 Advant',         @mLS2, @cMod, N'Modular con mentonera abatible 180°, doble homologación P/J.',           N'unisex', NULL, N'Policarbonato KPA', 1700, N'modular',  N'ece2206', 1, DATEADD(YEAR, 2, @hoy), 1),
        (N'AGV-K1S',    N'Casco Integral AGV K1 S',                @mAGV, @cInt, N'Integral deportivo. Lote con homologación DOT vencida: NO vender.',      N'unisex', NULL, N'Termoplástico',     1500, N'integral', N'dot',     0, DATEADD(MONTH, -2, @hoy), 1),
        (N'BEL-MX9',    N'Casco Cross Bell MX-9 MIPS',             @mBel, @cCro, N'Off-road con tecnología MIPS, visera ajustable.',                        N'unisex', NULL, N'Policarbonato',     1450, N'cross',    N'ece2206', 1, DATEADD(YEAR, 3, @hoy), 1),
        (N'ALP-TGPR3',  N'Campera Alpinestars T-GP Plus R v3 Air', @mAlp, @cCam, N'Campera textil ventilada con protecciones CE en hombros y codos.',       N'unisex', N'Verano 2026', N'Poliéster 600D', 1800, NULL, NULL, NULL, NULL, 1),
        (N'ALP-SMX1',   N'Guantes Alpinestars SMX-1 Air v2',       @mAlp, @cGua, N'Guante corto ventilado con nudillera de TPR.',                            N'unisex', N'Verano 2026', N'Cuero y malla', 250, NULL, NULL, NULL, NULL, 1),
        (N'FOX-180',    N'Pantalón Fox 180 Nitro',                 @mFox, @cPan, N'Pantalón de motocross, refuerzos en rodillas y panel elástico.',         N'hombre', N'Verano 2026', N'Poliéster',      900, NULL, NULL, NULL, NULL, 1),
        (N'ALP-SMX6',   N'Botas Alpinestars SMX-6 v2',             @mAlp, @cBot, N'Bota deportiva con protección de tobillo y bielas.',                    N'unisex', NULL, N'Microfibra',        2100, NULL, NULL, NULL, NULL, 1),
        (N'LS2-VISIRI', N'Visor LS2 FF800 Iridium',                @mLS2, @cAcc, N'Visor de repuesto espejado para FF800. Talla y color únicos.',           N'unisex', NULL, N'Policarbonato',      150, NULL, NULL, NULL, NULL, 1);

    -- 8.2 Variantes = producto × talla × color. Sufijo de color de 3 letras.
    --     Cascos: tallas numéricas 55-56..61-62 · Indumentaria: S..XL · Botas: 41..44.
    DECLARE @def TABLE (Codigo NVARCHAR(60), Talla NVARCHAR(30), Color NVARCHAR(60), Costo DECIMAL(18,4), Precio DECIMAL(18,2));
    INSERT INTO @def
    SELECT p.Codigo, t.Nombre, c.Nombre,
           CASE p.Codigo WHEN N'LS2-FF800' THEN 95 WHEN N'SHO-NXR2' THEN 380 WHEN N'LS2-FF906' THEN 130 WHEN N'AGV-K1S' THEN 150 WHEN N'BEL-MX9' THEN 140 END,
           CASE p.Codigo WHEN N'LS2-FF800' THEN 189 WHEN N'SHO-NXR2' THEN 690 WHEN N'LS2-FF906' THEN 259 WHEN N'AGV-K1S' THEN 299 WHEN N'BEL-MX9' THEN 279 END
    FROM PC_PRODUCTOS p
    CROSS JOIN PC_TALLAS t
    CROSS JOIN PC_COLORES c
    WHERE p.Codigo IN (N'LS2-FF800', N'SHO-NXR2', N'LS2-FF906', N'AGV-K1S', N'BEL-MX9')
      AND t.Nombre IN (N'55-56', N'57-58', N'59-60', N'61-62')
      AND ((p.Codigo = N'LS2-FF800' AND c.Nombre IN (N'Negro mate', N'Blanco', N'Rojo'))
        OR (p.Codigo = N'SHO-NXR2'  AND c.Nombre IN (N'Negro', N'Blanco'))
        OR (p.Codigo = N'LS2-FF906' AND c.Nombre IN (N'Negro mate', N'Gris'))
        OR (p.Codigo = N'AGV-K1S'   AND c.Nombre IN (N'Negro'))
        OR (p.Codigo = N'BEL-MX9'   AND c.Nombre IN (N'Rojo', N'Azul', N'Amarillo fluo')));

    INSERT INTO @def
    SELECT p.Codigo, t.Nombre, c.Nombre,
           CASE p.Codigo WHEN N'ALP-TGPR3' THEN 110 WHEN N'ALP-SMX1' THEN 32 WHEN N'FOX-180' THEN 45 END,
           CASE p.Codigo WHEN N'ALP-TGPR3' THEN 229 WHEN N'ALP-SMX1' THEN 69 WHEN N'FOX-180' THEN 99 END
    FROM PC_PRODUCTOS p
    CROSS JOIN PC_TALLAS t
    CROSS JOIN PC_COLORES c
    WHERE p.Codigo IN (N'ALP-TGPR3', N'ALP-SMX1', N'FOX-180')
      AND t.Nombre IN (N'S', N'M', N'L', N'XL')
      AND ((p.Codigo = N'ALP-TGPR3' AND c.Nombre IN (N'Negro', N'Rojo'))
        OR (p.Codigo = N'ALP-SMX1'  AND c.Nombre IN (N'Negro', N'Rojo'))
        OR (p.Codigo = N'FOX-180'   AND c.Nombre IN (N'Azul', N'Negro')));

    INSERT INTO @def
    SELECT p.Codigo, t.Nombre, c.Nombre, 120, 249
    FROM PC_PRODUCTOS p CROSS JOIN PC_TALLAS t CROSS JOIN PC_COLORES c
    WHERE p.Codigo = N'ALP-SMX6' AND t.Nombre IN (N'41', N'42', N'43', N'44') AND c.Nombre = N'Negro';

    INSERT INTO PC_VARIANTES (ProductoId, TallaId, ColorId, Sku, CodigoBarras, CostoEstandar, PrecioLista, Activo)
    SELECT p.Id, t.Id, c.Id,
           d.Codigo + N'-' + REPLACE(d.Talla, N'-', N'') + N'-' +
           CASE d.Color WHEN N'Negro' THEN N'NEG' WHEN N'Negro mate' THEN N'NGM' WHEN N'Blanco' THEN N'BLA' WHEN N'Rojo' THEN N'ROJ'
                        WHEN N'Azul' THEN N'AZU' WHEN N'Gris' THEN N'GRI' WHEN N'Amarillo fluo' THEN N'AMF' ELSE N'XXX' END,
           NULL, d.Costo, d.Precio, 1
    FROM @def d
    JOIN PC_PRODUCTOS p ON p.Codigo = d.Codigo
    JOIN PC_TALLAS t ON t.Nombre = d.Talla
    JOIN PC_COLORES c ON c.Nombre = d.Color;

    -- Accesorio sin talla ni color (variante única)
    INSERT INTO PC_VARIANTES (ProductoId, TallaId, ColorId, Sku, CodigoBarras, CostoEstandar, PrecioLista, Activo)
    SELECT Id, NULL, NULL, N'LS2-VISIRI-U', N'8431010999001', 18, 45, 1 FROM PC_PRODUCTOS WHERE Codigo = N'LS2-VISIRI';

    -- 8.3 Kardex. Curva de tallas: más M/L (57-58, 59-60) que extremos.
    DECLARE @dCen INT = (SELECT TOP 1 Id FROM PC_DEPOSITOS WHERE Codigo = N'DEP-CENTRAL');
    DECLARE @dSho INT = (SELECT TOP 1 Id FROM PC_DEPOSITOS WHERE Codigo = N'DEP-SHOW');

    -- Recepción PO-2026-001 (hace 20 días): cascos LS2/Shoei y campera/guantes. AGV NO entra (queda sin stock).
    INSERT INTO PC_MOVIMIENTOS_STOCK (VarianteId, DepositoId, DepositoDestinoId, Tipo, Cantidad, CostoUnitario, Motivo, DocumentoOrigen, Fecha, Usuario)
    SELECT v.Id, @dCen, NULL, N'entrada',
           CASE t.Nombre WHEN N'55-56' THEN 3 WHEN N'57-58' THEN 8 WHEN N'59-60' THEN 8 WHEN N'61-62' THEN 3
                         WHEN N'S' THEN 4 WHEN N'M' THEN 10 WHEN N'L' THEN 10 WHEN N'XL' THEN 4 ELSE 5 END,
           v.CostoEstandar, N'Recepción contenedor MSKU-4471', N'PO-2026-001', DATEADD(DAY, -20, @hoy), N'deposito'
    FROM PC_VARIANTES v JOIN PC_PRODUCTOS p ON p.Id = v.ProductoId LEFT JOIN PC_TALLAS t ON t.Id = v.TallaId
    WHERE p.Codigo IN (N'LS2-FF800', N'SHO-NXR2', N'LS2-FF906', N'ALP-TGPR3', N'ALP-SMX1');

    -- Recepción PO-2026-002 (hace 12 días): Bell, Fox, botas y visores.
    INSERT INTO PC_MOVIMIENTOS_STOCK (VarianteId, DepositoId, DepositoDestinoId, Tipo, Cantidad, CostoUnitario, Motivo, DocumentoOrigen, Fecha, Usuario)
    SELECT v.Id, @dCen, NULL, N'entrada',
           CASE WHEN p.Codigo = N'LS2-VISIRI' THEN 40 WHEN p.Codigo = N'ALP-SMX6' THEN 3
                ELSE CASE t.Nombre WHEN N'55-56' THEN 2 WHEN N'57-58' THEN 6 WHEN N'59-60' THEN 6 WHEN N'61-62' THEN 2
                                   WHEN N'S' THEN 3 WHEN N'M' THEN 8 WHEN N'L' THEN 8 WHEN N'XL' THEN 3 ELSE 4 END END,
           v.CostoEstandar, N'Recepción contenedor TCLU-9902', N'PO-2026-002', DATEADD(DAY, -12, @hoy), N'deposito'
    FROM PC_VARIANTES v JOIN PC_PRODUCTOS p ON p.Id = v.ProductoId LEFT JOIN PC_TALLAS t ON t.Id = v.TallaId
    WHERE p.Codigo IN (N'BEL-MX9', N'FOX-180', N'ALP-SMX6', N'LS2-VISIRI');

    -- Transferencias al showroom (hace 9 días): 2 unidades de cada talla central de LS2 FF800 negro mate y Shoei negro.
    INSERT INTO PC_MOVIMIENTOS_STOCK (VarianteId, DepositoId, DepositoDestinoId, Tipo, Cantidad, CostoUnitario, Motivo, DocumentoOrigen, Fecha, Usuario)
    SELECT v.Id, @dCen, @dSho, N'transferencia', 2, v.CostoEstandar, N'Reposición vidriera showroom', N'TR-2026-014', DATEADD(DAY, -9, @hoy), N'deposito'
    FROM PC_VARIANTES v WHERE v.Sku IN (N'LS2-FF800-5758-NGM', N'LS2-FF800-5960-NGM', N'SHO-NXR2-5758-NEG', N'SHO-NXR2-5960-NEG');

    -- Salidas por ventas mayoristas (últimos 8 días)
    INSERT INTO PC_MOVIMIENTOS_STOCK (VarianteId, DepositoId, DepositoDestinoId, Tipo, Cantidad, CostoUnitario, Motivo, DocumentoOrigen, Fecha, Usuario)
    SELECT v.Id, @dCen, NULL, N'salida', x.Cant, v.CostoEstandar, N'Venta mayorista', x.Doc, DATEADD(DAY, -x.Dias, @hoy), N'ventas'
    FROM (VALUES
        (N'LS2-FF800-5758-NGM', 4, N'REM-0001', 8), (N'LS2-FF800-5960-ROJ', 3, N'REM-0001', 8), (N'LS2-FF800-5758-BLA', 2, N'REM-0002', 7),
        (N'SHO-NXR2-5960-BLA',  2, N'REM-0003', 6), (N'LS2-FF906-5758-NGM', 3, N'REM-0003', 6), (N'ALP-TGPR3-M-NEG', 5, N'REM-0004', 5),
        (N'ALP-TGPR3-L-NEG',    4, N'REM-0004', 5), (N'ALP-SMX1-M-NEG',    6, N'REM-0005', 4), (N'ALP-SMX1-L-ROJ',    3, N'REM-0005', 4),
        (N'BEL-MX9-5758-AMF',   2, N'REM-0006', 3), (N'FOX-180-M-AZU',     4, N'REM-0006', 3), (N'ALP-SMX6-42-NEG',   1, N'REM-0007', 2),
        (N'LS2-VISIRI-U',      12, N'REM-0007', 2), (N'LS2-FF800-5960-NGM', 6, N'REM-0008', 1)
    ) AS x(Sku, Cant, Doc, Dias)
    JOIN PC_VARIANTES v ON v.Sku = x.Sku;

    -- Venta desde el showroom (hace 2 días)
    INSERT INTO PC_MOVIMIENTOS_STOCK (VarianteId, DepositoId, DepositoDestinoId, Tipo, Cantidad, CostoUnitario, Motivo, DocumentoOrigen, Fecha, Usuario)
    SELECT v.Id, @dSho, NULL, N'salida', 1, v.CostoEstandar, N'Venta mostrador', N'TKT-00021', DATEADD(DAY, -2, @hoy), N'showroom'
    FROM PC_VARIANTES v WHERE v.Sku = N'SHO-NXR2-5758-NEG';

    -- Ajuste por inventario físico (hace 1 día): faltante de 1 guante → ajuste NEGATIVO (el ajuste lleva signo)
    INSERT INTO PC_MOVIMIENTOS_STOCK (VarianteId, DepositoId, DepositoDestinoId, Tipo, Cantidad, CostoUnitario, Motivo, DocumentoOrigen, Fecha, Usuario)
    SELECT v.Id, @dCen, NULL, N'ajuste', -1, v.CostoEstandar, N'Ajuste por conteo cíclico: faltante', N'INV-2026-09', DATEADD(DAY, -1, @hoy), N'deposito'
    FROM PC_VARIANTES v WHERE v.Sku = N'ALP-SMX1-S-NEG';

    -- Saldo NEGATIVO a propósito: se vendió un AGV K1 S que nunca ingresó (error operativo visible en Existencias).
    INSERT INTO PC_MOVIMIENTOS_STOCK (VarianteId, DepositoId, DepositoDestinoId, Tipo, Cantidad, CostoUnitario, Motivo, DocumentoOrigen, Fecha, Usuario)
    SELECT v.Id, @dCen, NULL, N'salida', 1, v.CostoEstandar, N'Venta mayorista (sin recepción previa: revisar)', N'REM-0009', @hoy, N'ventas'
    FROM PC_VARIANTES v WHERE v.Sku = N'AGV-K1S-5758-NEG';

    PRINT 'Seed de dominio: catálogo, variantes y Kardex sembrados.';
END
ELSE
    PRINT 'Seed de dominio: ya hay productos, se omite catálogo/Kardex.';
GO

-- ============================================================================
-- 9. Clientes mayoristas + envíos con observaciones. Solo si no hay envíos.
-- ============================================================================
IF NOT EXISTS (SELECT 1 FROM PC_CLIENTES WHERE Nombre = N'Moto Center Rivera')        INSERT INTO PC_CLIENTES (Nombre, Telefono, DireccionEntrega) VALUES (N'Moto Center Rivera',        N'+598 4622 3344', N'Sarandí 512, Rivera');
IF NOT EXISTS (SELECT 1 FROM PC_CLIENTES WHERE Nombre = N'Casa Bike Salto')           INSERT INTO PC_CLIENTES (Nombre, Telefono, DireccionEntrega) VALUES (N'Casa Bike Salto',           N'+598 4733 1020', N'Uruguay 780, Salto');
IF NOT EXISTS (SELECT 1 FROM PC_CLIENTES WHERE Nombre = N'Ruta 5 Motos')              INSERT INTO PC_CLIENTES (Nombre, Telefono, DireccionEntrega) VALUES (N'Ruta 5 Motos',              N'+598 4362 5566', N'Ruta 5 km 183, Durazno');
IF NOT EXISTS (SELECT 1 FROM PC_CLIENTES WHERE Nombre = N'Punto Moto Maldonado')      INSERT INTO PC_CLIENTES (Nombre, Telefono, DireccionEntrega) VALUES (N'Punto Moto Maldonado',      N'+598 4222 9090', N'Av. Roosevelt 1450, Maldonado');
IF NOT EXISTS (SELECT 1 FROM PC_CLIENTES WHERE Nombre = N'Racing Store Montevideo')   INSERT INTO PC_CLIENTES (Nombre, Telefono, DireccionEntrega) VALUES (N'Racing Store Montevideo',   N'+598 2400 1234', N'Av. Italia 3300, Montevideo');
IF NOT EXISTS (SELECT 1 FROM PC_CLIENTES WHERE Nombre = N'El Cruce Motopartes')       INSERT INTO PC_CLIENTES (Nombre, Telefono, DireccionEntrega) VALUES (N'El Cruce Motopartes',       N'+598 4632 7788', N'18 de Julio 240, Tacuarembó');
IF NOT EXISTS (SELECT 1 FROM PC_CLIENTES WHERE Nombre = N'Motos del Este')            INSERT INTO PC_CLIENTES (Nombre, Telefono, DireccionEntrega) VALUES (N'Motos del Este',            N'+598 4472 4455', N'Gral. Artigas 88, Rocha');
IF NOT EXISTS (SELECT 1 FROM PC_CLIENTES WHERE Nombre = N'Distribuidora Litoral')     INSERT INTO PC_CLIENTES (Nombre, Telefono, DireccionEntrega) VALUES (N'Distribuidora Litoral',     N'+598 4722 6677', N'Zorrilla 1010, Paysandú');
GO

IF NOT EXISTS (SELECT 1 FROM PC_ENVIOS)
BEGIN
    DECLARE @hoy DATETIME2 = CAST(CAST(GETDATE() AS DATE) AS DATETIME2);
    DECLARE @aDAC INT = (SELECT TOP 1 Id FROM PC_AGENCIAS WHERE Nombre = N'DAC');
    DECLARE @aMir INT = (SELECT TOP 1 Id FROM PC_AGENCIAS WHERE Nombre = N'Mirtrans');
    DECLARE @aFlo INT = (SELECT TOP 1 Id FROM PC_AGENCIAS WHERE Nombre = N'Flota propia');

    -- Las fechas de etapas no alcanzadas repiten la anterior (comportamiento de la app).
    INSERT INTO PC_ENVIOS (CodigoRastreo, Estado, FechaRecibido, FechaFactura, FechaEnvio, FechaEntrega, MotivoAnulacion, ClienteId, AgenciaId)
    SELECT x.Cod, x.Estado,
           DATEADD(DAY, -x.dRec, @hoy),
           DATEADD(DAY, -ISNULL(x.dFac, x.dRec), @hoy),
           DATEADD(DAY, -ISNULL(x.dEnv, ISNULL(x.dFac, x.dRec)), @hoy),
           DATEADD(DAY, -ISNULL(x.dEnt, ISNULL(x.dEnv, ISNULL(x.dFac, x.dRec))), @hoy),
           ISNULL(x.Motivo, N''), c.Id, x.Ag
    FROM (VALUES
        -- Cod, Estado, dRec, dFac, dEnv, dEnt, Motivo, Cliente, Agencia
        (N'MT-2026-0101', N'entregado',  18, 17, 16, 14, NULL, N'Moto Center Rivera',      @aDAC),
        (N'MT-2026-0102', N'entregado',  16, 15, 14, 12, NULL, N'Casa Bike Salto',         @aDAC),
        (N'MT-2026-0103', N'entregado',  14, 13, 12, 11, NULL, N'Racing Store Montevideo', @aFlo),
        (N'MT-2026-0104', N'despachado', 10,  9,  8, NULL, NULL, N'Ruta 5 Motos',           @aMir),
        (N'MT-2026-0105', N'despachado',  9,  8,  6, NULL, NULL, N'Punto Moto Maldonado',   @aDAC),
        (N'MT-2026-0106', N'despachado',  7,  6,  5, NULL, NULL, N'El Cruce Motopartes',    @aMir),
        (N'MT-2026-0107', N'despachado',  4,  3,  2, NULL, NULL, N'Motos del Este',         @aDAC),
        (N'MT-2026-0108', N'facturado',   6,  5, NULL, NULL, NULL, N'Distribuidora Litoral', @aMir),
        (N'MT-2026-0109', N'facturado',   3,  2, NULL, NULL, NULL, N'Moto Center Rivera',    @aDAC),
        (N'MT-2026-0110', N'facturado',   1,  0, NULL, NULL, NULL, N'Racing Store Montevideo', @aFlo),
        (N'MT-2026-0111', N'recibido',   12, NULL, NULL, NULL, NULL, N'Casa Bike Salto',      @aDAC),   -- fuera de SLA de facturación
        (N'MT-2026-0112', N'recibido',    3, NULL, NULL, NULL, NULL, N'Punto Moto Maldonado', @aMir),
        (N'MT-2026-0113', N'recibido',    1, NULL, NULL, NULL, NULL, N'Ruta 5 Motos',         @aDAC),
        (N'MT-2026-0114', N'recibido',    0, NULL, NULL, NULL, NULL, N'Motos del Este',       @aFlo),
        (N'MT-2026-0115', N'anulado',     8,  7, NULL, NULL, N'Cliente canceló el pedido por demora de importación', N'El Cruce Motopartes', @aMir)
    ) AS x(Cod, Estado, dRec, dFac, dEnv, dEnt, Motivo, Cliente, Ag)
    JOIN PC_CLIENTES c ON c.Nombre = x.Cliente;

    INSERT INTO PC_OBSERVACIONES (Texto, FechaHora, Usuario, EnvioId)
    SELECT x.Texto, DATEADD(DAY, -x.Dias, @hoy), x.Usr, e.Id
    FROM (VALUES
        (N'MT-2026-0101', N'Entregado en depósito del cliente, firma en remito.', 14, N'agencia'),
        (N'MT-2026-0104', N'Salió con la carga de Durazno, llega mañana temprano.', 8, N'despacho'),
        (N'MT-2026-0105', N'Cliente pide que avisen antes de entregar (local cierra 13 a 15h).', 7, N'ventas'),
        (N'MT-2026-0107', N'Bulto 2 de 3 con embalaje dañado, se reembaló antes de despachar.', 2, N'deposito'),
        (N'MT-2026-0108', N'Esperando confirmación de stock de visores para completar el pedido.', 4, N'ventas'),
        (N'MT-2026-0111', N'Pedido recibido por WhatsApp, falta cargar la factura.', 12, N'ventas'),
        (N'MT-2026-0111', N'Se reclamó a administración: 12 días sin facturar.', 1, N'gerencia'),
        (N'MT-2026-0115', N'Cliente avisó que compró en otro proveedor. Se libera el stock reservado.', 6, N'ventas')
    ) AS x(Cod, Texto, Dias, Usr)
    JOIN PC_ENVIOS e ON e.CodigoRastreo = x.Cod;

    PRINT 'Seed de dominio: clientes, envíos y observaciones sembrados.';
END
ELSE
    PRINT 'Seed de dominio: ya hay envíos, se omite.';
GO

-- ============================================================================
-- 10. Vendedores (maestra, fila por fila). El de Montevideo y Este queda
--     enlazado al login de desarrollo 'pablo' para que /api/Vendedor/mio
--     responda y la agenda arranque en "lo mío".
-- ============================================================================
IF NOT EXISTS (SELECT 1 FROM PC_VENDEDORES WHERE Nombre = N'Andrés Ferreira')
    INSERT INTO PC_VENDEDORES (Nombre, Telefono, Email, Zona, ComisionPorcentaje, Usuario, Activo)
    VALUES (N'Andrés Ferreira', N'+598 99 412 300', N'andres.ferreira@motos.com.uy', N'Litoral', 4.50, NULL, 1);
IF NOT EXISTS (SELECT 1 FROM PC_VENDEDORES WHERE Nombre = N'Lucía Méndez')
    INSERT INTO PC_VENDEDORES (Nombre, Telefono, Email, Zona, ComisionPorcentaje, Usuario, Activo)
    VALUES (N'Lucía Méndez', N'+598 99 655 118', N'lucia.mendez@motos.com.uy', N'Norte', 5.00, NULL, 1);
IF NOT EXISTS (SELECT 1 FROM PC_VENDEDORES WHERE Nombre = N'Pablo Ceschi')
    INSERT INTO PC_VENDEDORES (Nombre, Telefono, Email, Zona, ComisionPorcentaje, Usuario, Activo)
    VALUES (N'Pablo Ceschi', N'+598 99 120 447', N'pablo@motos.com.uy', N'Montevideo y Este', 3.50, N'pablo', 1);
GO

-- ============================================================================
-- 11. Datos comerciales de los clientes ya sembrados (§3.2 y §3.8): tipo,
--     ciudad, contacto, email, vendedor asignado, notas y coordenadas para la
--     ruta del día. ISNULL por columna: rellena lo vacío, nunca pisa lo editado.
-- ============================================================================
UPDATE c SET
      c.Tipo       = ISNULL(c.Tipo, x.Tipo)
    , c.Ciudad     = ISNULL(c.Ciudad, x.Ciudad)
    , c.Contacto   = ISNULL(c.Contacto, x.Contacto)
    , c.Email      = ISNULL(c.Email, x.Email)
    , c.Notas      = ISNULL(c.Notas, x.Notas)
    , c.Latitud    = ISNULL(c.Latitud, x.Lat)
    , c.Longitud   = ISNULL(c.Longitud, x.Lng)
    , c.VendedorId = ISNULL(c.VendedorId, v.Id)
FROM PC_CLIENTES c
JOIN (VALUES
    -- Nombre, Tipo, Ciudad, Contacto, Email, Notas, Lat, Lng, Zona del vendedor
    (N'Moto Center Rivera',      N'tienda',       N'Rivera',      N'Gustavo Silveira', N'compras@motocenter.com.uy',  N'Compra fuerte antes de temporada alta. Paga a 30 días.',        -30.905300, -55.550800, N'Norte'),
    (N'Casa Bike Salto',         N'tienda',       N'Salto',       N'Marcela Rodríguez', N'ventas@casabike.com.uy',    N'Cierra de 13 a 15. Pedir por WhatsApp antes de pasar.',         -31.383300, -57.966700, N'Litoral'),
    (N'Ruta 5 Motos',            N'tienda',       N'Durazno',     N'Julio Barreto',    N'ruta5motos@gmail.com',       N'Sobre la ruta, fácil para la agencia. Prefiere entregas de mañana.', -33.383300, -56.516700, N'Litoral'),
    (N'Punto Moto Maldonado',    N'tienda',       N'Maldonado',   N'Valeria Núñez',    N'compras@puntomoto.com.uy',   N'Verano es su pico: reponer cascos jet y guantes en noviembre.', -34.916000, -54.950000, N'Montevideo y Este'),
    (N'Racing Store Montevideo', N'distribuidor', N'Montevideo',  N'Diego Pereira',    N'diego@racingstore.com.uy',   N'El cliente más grande. Pide factura con orden de compra.',      -34.885000, -56.135000, N'Montevideo y Este'),
    (N'El Cruce Motopartes',     N'tienda',       N'Tacuarembó',  N'Ramón Olivera',    N'elcruce@adinet.com.uy',      N'Local chico, compra poco y seguido.',                          -31.733300, -55.983300, N'Norte'),
    (N'Motos del Este',          N'online',       N'Rocha',       N'Sofía Cabrera',    N'hola@motosdeleste.uy',       N'Vende por Instagram, sin local a la calle. Coordinar por WhatsApp.', -34.483300, -54.333300, N'Montevideo y Este'),
    (N'Distribuidora Litoral',   N'distribuidor', N'Paysandú',    N'Enrique Sosa',     N'compras@distlitoral.com.uy', N'Revende a talleres del litoral. Negocia volumen.',              -32.321400, -58.075600, N'Litoral')
) AS x(Nombre, Tipo, Ciudad, Contacto, Email, Notas, Lat, Lng, Zona) ON c.Nombre = x.Nombre
LEFT JOIN PC_VENDEDORES v ON v.Zona = x.Zona;
GO

-- ============================================================================
-- 12. Actividad comercial. Solo si no hay actividades (bloque atómico).
--     23 en las últimas 3 semanas + 2 viejas para que "El Cruce Motopartes" y
--     "Motos del Este" disparen la alerta de 30 días sin visita; 8 con
--     ProximaAccion dentro de los próximos 7 días para que la agenda tenga
--     paradas. El vendedor sale del cliente (el asignado en la sección 11).
--     Ninguna con Resultado = 'pedido': Pedido llega en la Etapa B.
-- ============================================================================
IF NOT EXISTS (SELECT 1 FROM PC_ACTIVIDADES)
BEGIN
    DECLARE @hoyAct DATETIME2 = CAST(CAST(GETDATE() AS DATE) AS DATETIME2);
    -- Las 3 semanas de contacto se COMPRIMEN dentro del mes en curso (mismo motivo que
    -- los pedidos: visitas del mes y tasa de cierre se miden por mes). Las 2 actividades
    -- viejas (+30 días) NO se tocan: son las que disparan la alerta de "sin visitar".
    DECLARE @escalaAct DECIMAL(9,4) = CASE WHEN DAY(@hoyAct) > 1 THEN (DAY(@hoyAct) - 1) / 21.0 ELSE 0 END;

    INSERT INTO PC_ACTIVIDADES (VendedorId, ClienteId, Tipo, Fecha, Resultado, Notas, ProximaAccion, PedidoId)
    SELECT c.VendedorId, c.Id, x.Tipo,
           DATEADD(HOUR, x.Hora, DATEADD(DAY,
               -CASE WHEN x.Dias > 30 THEN x.Dias ELSE CAST(ROUND(x.Dias * @escalaAct, 0) AS INT) END, @hoyAct)),
           x.Resultado, x.Notas,
           CASE WHEN x.Prox IS NULL THEN NULL ELSE CAST(DATEADD(DAY, x.Prox, @hoyAct) AS DATE) END,
           NULL
    FROM (VALUES
        -- Cliente, Tipo, DíasAtrás, Hora, Resultado, Notas, PróximaAcción (días desde hoy)
        (N'Racing Store Montevideo', N'visita',   19, 10, N'sin_pedido',   N'Recorrida de catálogo nuevo. Quedaron en pedir cuando baje el stock de integrales.', NULL),
        (N'Racing Store Montevideo', N'llamada',  12, 15, N'reprogramar',  N'Diego de licencia, atiende el encargado. Volver a llamar la semana que viene.', 2),
        (N'Racing Store Montevideo', N'whatsapp',  6, 11, N'sin_pedido',   N'Le pasé precios de la línea Alpinestars. Los está comparando.', NULL),
        (N'Racing Store Montevideo', N'visita',    2,  9, N'reprogramar',  N'Arma la orden de compra con el contador. Pasar a buscarla.', 3),
        (N'Racing Store Montevideo', N'email',     1, 16, N'sin_pedido',   N'Le mandé la lista de precios en dólares actualizada.', 6),
        (N'Punto Moto Maldonado',   N'visita',    17, 11, N'sin_pedido',   N'Todavía con stock del verano. Reponer en octubre.', NULL),
        (N'Punto Moto Maldonado',   N'whatsapp',  10, 12, N'sin_pedido',   N'Consultó por talles L y XL de campera de cordura.', NULL),
        (N'Punto Moto Maldonado',   N'llamada',    4, 10, N'reprogramar',  N'Pidió que pase el jueves con muestras.', 4),
        (N'Punto Moto Maldonado',   N'visita',     0, 15, N'sin_contacto', N'Local cerrado, cartel de "vuelvo en una hora". Reintentar.', 1),
        (N'Motos del Este',         N'whatsapp',  38, 13, N'sin_pedido',   N'Cerró la temporada, dijo que retoma después de setiembre.', NULL),
        (N'Casa Bike Salto',        N'visita',    20,  9, N'sin_pedido',   N'Le mostré la matriz de talles. Va a definir con el socio.', NULL),
        (N'Casa Bike Salto',        N'llamada',   13, 14, N'sin_pedido',   N'Preguntó por plazos de entrega a Salto: 3 días por DAC.', NULL),
        (N'Casa Bike Salto',        N'visita',     7, 10, N'reprogramar',  N'Estaban con inventario. Volver la semana próxima.', 5),
        (N'Casa Bike Salto',        N'whatsapp',   3, 17, N'sin_pedido',   N'Mandó foto de la vidriera: le faltan cascos jet negros.', NULL),
        (N'Distribuidora Litoral',  N'visita',    18, 11, N'sin_pedido',   N'Negocia descuento por volumen. Escalar a gerencia.', NULL),
        (N'Distribuidora Litoral',  N'email',     11, 10, N'sin_pedido',   N'Le envié la propuesta de bonificación por 50 unidades.', NULL),
        (N'Distribuidora Litoral',  N'llamada',    5, 16, N'reprogramar',  N'Revisa la propuesta con el directorio. Llamar el lunes.', 3),
        (N'Distribuidora Litoral',  N'visita',     1, 12, N'sin_pedido',   N'Buen clima, quedó en confirmar la compra esta semana.', 7),
        (N'Ruta 5 Motos',           N'visita',    21, 10, N'sin_pedido',   N'Sobre la ruta, paré de paso. Poco movimiento.', NULL),
        (N'Ruta 5 Motos',           N'whatsapp',  14,  9, N'sin_contacto', N'No respondió el mensaje.', NULL),
        (N'Ruta 5 Motos',           N'llamada',    8, 15, N'sin_pedido',   N'Julio pide esperar a cobrar para reponer.', NULL),
        (N'Moto Center Rivera',     N'visita',    16, 10, N'sin_pedido',   N'Recorrida de temporada. Interesado en guantes y antiparras.', NULL),
        (N'Moto Center Rivera',     N'llamada',    9, 11, N'sin_pedido',   N'Confirmó recepción del último envío, todo conforme.', NULL),
        (N'Moto Center Rivera',     N'visita',     3, 14, N'reprogramar',  N'Gustavo viajando a la frontera. Volver el viernes.', 5),
        (N'El Cruce Motopartes',    N'llamada',   45, 10, N'sin_pedido',   N'Ramón pidió no insistir hasta después de la zafra.', NULL)
    ) AS x(Cliente, Tipo, Dias, Hora, Resultado, Notas, Prox)
    JOIN PC_CLIENTES c ON c.Nombre = x.Cliente
    WHERE c.VendedorId IS NOT NULL;

    PRINT 'Seed de dominio: actividades comerciales sembradas.';
END
ELSE
    PRINT 'Seed de dominio: ya hay actividades, se omite.';
GO

-- ============================================================================
-- 13. Metas del mes corriente (una por vendedor, §3.8). Única por vendedor y
--     período, así que el IF NOT EXISTS mira las dos columnas.
-- ============================================================================
DECLARE @periodo NVARCHAR(7) = FORMAT(GETDATE(), 'yyyy-MM');
DECLARE @periodoAnt NVARCHAR(7) = FORMAT(DATEADD(MONTH, -1, GETDATE()), 'yyyy-MM');

INSERT INTO PC_METAS (VendedorId, Periodo, ObjetivoUsd)
SELECT v.Id, @periodo, x.Objetivo
FROM (VALUES
    (N'Andrés Ferreira', 18000.00),
    (N'Lucía Méndez',    12000.00),
    (N'Pablo Ceschi',    25000.00)
) AS x(Nombre, Objetivo)
JOIN PC_VENDEDORES v ON v.Nombre = x.Nombre
WHERE NOT EXISTS (SELECT 1 FROM PC_METAS m WHERE m.VendedorId = v.Id AND m.Periodo = @periodo);

-- Metas del mes anterior (Etapa H.6): la pantalla de comisiones compara contra el mes
-- pasado, y sin meta previa la comparación queda coja.
INSERT INTO PC_METAS (VendedorId, Periodo, ObjetivoUsd)
SELECT v.Id, @periodoAnt, x.Objetivo
FROM (VALUES
    (N'Andrés Ferreira', 16000.00),
    (N'Lucía Méndez',    12000.00),
    (N'Pablo Ceschi',    22000.00)
) AS x(Nombre, Objetivo)
JOIN PC_VENDEDORES v ON v.Nombre = x.Nombre
WHERE NOT EXISTS (SELECT 1 FROM PC_METAS m WHERE m.VendedorId = v.Id AND m.Periodo = @periodoAnt);
GO

-- ============================================================================
-- 14. Pedidos de venta con sus líneas (Etapa B). Solo si no hay pedidos.
--     12 pedidos anclados a HOY repartidos por todo el ciclo, para que el kanban
--     tenga columnas con contenido y la liquidación de comisiones muestre plata.
--     El vendedor sale del cliente; el depósito es el Central salvo dos del
--     showroom. Los importes se calculan desde PC_VARIANTES: el seed nunca
--     inventa precios, copia el de lista como lo haría el armado de pedido.
--     Los despachados/entregados descuentan stock (salidas con DocumentoOrigen
--     = Numero, igual que PedidoHooks) y se enlazan a envíos ya sembrados.
-- ============================================================================
IF NOT EXISTS (SELECT 1 FROM PC_PEDIDOS)
BEGIN
    DECLARE @hoyPed DATETIME2 = CAST(CAST(GETDATE() AS DATE) AS DATETIME2);
    DECLARE @depCentral INT = (SELECT TOP 1 Id FROM PC_DEPOSITOS WHERE Codigo = N'DEP-CENTRAL');
    DECLARE @depShow    INT = (SELECT TOP 1 Id FROM PC_DEPOSITOS WHERE Codigo = N'DEP-SHOW');
    -- Los 24 días de historia se COMPRIMEN dentro del mes en curso: la comisión y la meta
    -- se liquidan por mes, y un seed sembrado un día 8 dejaría los entregados en el mes
    -- anterior (pantalla de comisiones vacía). Se conserva el orden relativo del relato;
    -- sembrado un día 1, todos caen hoy.
    DECLARE @escala DECIMAL(9,4) = CASE WHEN DAY(@hoyPed) > 1 THEN (DAY(@hoyPed) - 1) / 24.0 ELSE 0 END;

    -- ── 14.1 Cabeceras ──────────────────────────────────────────────────────
    INSERT INTO PC_PEDIDOS (Numero, Fecha, ClienteId, VendedorId, DepositoId, AgenciaId, Estado, TotalUsd, ComisionUsd, Observaciones, EnvioId, MotivoAnulacion)
    SELECT x.Numero, DATEADD(DAY, -CAST(ROUND(x.Dias * @escala, 0) AS INT), @hoyPed), c.Id, c.VendedorId,
           CASE WHEN x.Deposito = N'SHOW' THEN @depShow ELSE @depCentral END,
           a.Id, x.Estado, 0, 0, x.Observaciones, NULL, ISNULL(x.Motivo, N'')
    FROM (VALUES
        -- Numero, DíasAtrás, Cliente, Estado, Agencia, Depósito, Observaciones, MotivoAnulación
        (N'PED-0001', 24, N'Racing Store Montevideo', N'entregado',  N'Flota propia', N'CENTRAL', N'Reposición de temporada.', NULL),
        (N'PED-0002', 21, N'Casa Bike Salto',         N'entregado',  N'DAC',          N'CENTRAL', N'Pedido cerrado en la visita.', NULL),
        (N'PED-0003', 18, N'Punto Moto Maldonado',    N'entregado',  N'DAC',          N'CENTRAL', N'Pidió que avisen antes de entregar.', NULL),
        (N'PED-0004', 15, N'Distribuidora Litoral',   N'entregado',  N'Mirtrans',     N'CENTRAL', N'Compra por volumen del litoral.', NULL),
        (N'PED-0005', 11, N'Moto Center Rivera',      N'despachado', N'DAC',          N'CENTRAL', N'Salió con la carga del norte.', NULL),
        (N'PED-0006',  8, N'Motos del Este',          N'despachado', N'Mirtrans',     N'CENTRAL', N'Coordinado por WhatsApp.', NULL),
        (N'PED-0007',  5, N'Racing Store Montevideo', N'preparado',  N'Flota propia', N'SHOW',    N'Retira la flota propia mañana.', NULL),
        (N'PED-0008',  4, N'Casa Bike Salto',         N'preparado',  N'DAC',          N'CENTRAL', N'Falta que pase la agencia.', NULL),
        (N'PED-0009',  3, N'Punto Moto Maldonado',    N'confirmado', N'DAC',          N'CENTRAL', N'Confirmado por teléfono.', NULL),
        (N'PED-0010',  2, N'Distribuidora Litoral',   N'confirmado', N'Mirtrans',     N'CENTRAL', N'Espera la bonificación por volumen.', NULL),
        (N'PED-0011',  1, N'Ruta 5 Motos',            N'borrador',   N'DAC',          N'CENTRAL', N'Armado en la visita, falta confirmar.', NULL),
        (N'PED-0012',  9, N'El Cruce Motopartes',     N'anulado',    N'Mirtrans',     N'SHOW',    N'Se dio de baja antes de preparar.', N'El cliente consiguió el producto en plaza')
    ) AS x(Numero, Dias, Cliente, Estado, Agencia, Deposito, Observaciones, Motivo)
    JOIN PC_CLIENTES c ON c.Nombre = x.Cliente
    LEFT JOIN PC_AGENCIAS a ON a.Nombre = x.Agencia
    WHERE c.VendedorId IS NOT NULL;

    -- ── 14.2 Líneas (el precio se copia del de lista, como en el armado) ────
    INSERT INTO PC_PEDIDO_LINEAS (PedidoId, VarianteId, Cantidad, PrecioUnitarioUsd, SubtotalUsd)
    SELECT p.Id, v.Id, x.Cantidad, v.PrecioLista, x.Cantidad * v.PrecioLista
    FROM (VALUES
        (N'PED-0001', N'LS2-FF906-5960-GRI', 2), (N'PED-0001', N'ALP-TGPR3-L-ROJ', 2), (N'PED-0001', N'LS2-VISIRI-U', 4),
        (N'PED-0002', N'LS2-FF800-5960-BLA', 2), (N'PED-0002', N'ALP-SMX1-L-NEG', 3),
        (N'PED-0003', N'BEL-MX9-5960-ROJ', 1),   (N'PED-0003', N'FOX-180-L-NEG', 2),
        (N'PED-0004', N'LS2-FF800-5758-ROJ', 3), (N'PED-0004', N'LS2-VISIRI-U', 6), (N'PED-0004', N'ALP-SMX1-M-ROJ', 2),
        (N'PED-0005', N'LS2-FF906-5758-GRI', 2), (N'PED-0005', N'FOX-180-M-NEG', 2),
        (N'PED-0006', N'ALP-TGPR3-M-ROJ', 1),    (N'PED-0006', N'ALP-SMX1-L-ROJ', 2),
        (N'PED-0007', N'SHO-NXR2-5758-BLA', 1),  (N'PED-0007', N'LS2-VISIRI-U', 2),
        (N'PED-0008', N'LS2-FF800-5758-BLA', 2), (N'PED-0008', N'ALP-SMX1-L-NEG', 2),
        (N'PED-0009', N'BEL-MX9-5758-AZU', 2),   (N'PED-0009', N'FOX-180-L-AZU', 2),
        (N'PED-0010', N'LS2-FF906-5960-NGM', 3), (N'PED-0010', N'ALP-TGPR3-L-ROJ', 2), (N'PED-0010', N'LS2-VISIRI-U', 4),
        (N'PED-0011', N'BEL-MX9-5960-AZU', 1),   (N'PED-0011', N'FOX-180-L-NEG', 1),
        (N'PED-0012', N'LS2-FF800-5960-BLA', 1)
    ) AS x(Numero, Sku, Cantidad)
    JOIN PC_PEDIDOS p ON p.Numero = x.Numero
    JOIN PC_VARIANTES v ON v.Sku = x.Sku;

    -- ── 14.3 Totales y comisión (la comisión se devenga al entregar) ────────
    UPDATE p SET p.TotalUsd = ISNULL(l.Total, 0)
    FROM PC_PEDIDOS p
    LEFT JOIN (SELECT PedidoId, SUM(SubtotalUsd) AS Total FROM PC_PEDIDO_LINEAS GROUP BY PedidoId) l ON l.PedidoId = p.Id;

    UPDATE p SET p.ComisionUsd = ROUND(p.TotalUsd * v.ComisionPorcentaje / 100.0, 2)
    FROM PC_PEDIDOS p
    JOIN PC_VENDEDORES v ON v.Id = p.VendedorId
    WHERE p.Estado = N'entregado';

    -- ── 14.4 El stock que salió del depósito (mismo Kardex que PedidoHooks) ─
    INSERT INTO PC_MOVIMIENTOS_STOCK (VarianteId, DepositoId, DepositoDestinoId, Tipo, Cantidad, CostoUnitario, Motivo, DocumentoOrigen, Fecha, Usuario)
    SELECT l.VarianteId, p.DepositoId, NULL, N'salida', l.Cantidad, v.CostoEstandar,
           N'Despacho del pedido ' + p.Numero, p.Numero, p.Fecha, N'deposito'
    FROM PC_PEDIDO_LINEAS l
    JOIN PC_PEDIDOS p ON p.Id = l.PedidoId
    JOIN PC_VARIANTES v ON v.Id = l.VarianteId
    WHERE p.Estado IN (N'despachado', N'entregado');

    -- ── 14.5 Enlace con los envíos ya sembrados (plan §3.6) ─────────────────
    -- A cada pedido despachado/entregado se le asigna un envío libre del mismo
    -- cliente. Si no hay, el pedido queda sin envío: el tracking sigue valiendo solo.
    UPDATE p SET p.EnvioId = e.Id
    FROM PC_PEDIDOS p
    CROSS APPLY (SELECT TOP 1 e2.Id FROM PC_ENVIOS e2
                 WHERE e2.ClienteId = p.ClienteId AND e2.PedidoId IS NULL AND e2.Estado <> N'anulado'
                 ORDER BY e2.Id) e
    WHERE p.Estado IN (N'despachado', N'entregado') AND p.EnvioId IS NULL;

    UPDATE e SET e.PedidoId = p.Id
    FROM PC_ENVIOS e
    JOIN PC_PEDIDOS p ON p.EnvioId = e.Id
    WHERE e.PedidoId IS NULL;

    -- ── 14.6 Las visitas que terminaron en pedido ───────────────────────────
    -- La actividad más reciente de cada cliente con pedido queda con resultado
    -- 'pedido' apuntándolo: así la tasa de cierre del vendedor deja de ser cero.
    UPDATE a SET a.Resultado = N'pedido', a.PedidoId = p.Id, a.Notas = N'Cerró el pedido ' + p.Numero + N' en la visita.'
    FROM PC_ACTIVIDADES a
    JOIN (
        SELECT ClienteId, MAX(Id) AS ActividadId
        FROM PC_ACTIVIDADES
        WHERE Tipo = N'visita' AND Resultado = N'sin_pedido'
        GROUP BY ClienteId
    ) ult ON ult.ActividadId = a.Id
    JOIN (
        SELECT ClienteId, MIN(Id) AS PedidoId
        FROM PC_PEDIDOS
        WHERE Estado IN (N'entregado', N'despachado')
        GROUP BY ClienteId
    ) x ON x.ClienteId = a.ClienteId
    JOIN PC_PEDIDOS p ON p.Id = x.PedidoId;

    PRINT 'Seed de dominio: pedidos, líneas y su stock sembrados.';
END
ELSE
    PRINT 'Seed de dominio: ya hay pedidos, se omite.';
GO

-- ============================================================================
-- 14 bis. Pedidos ENTREGADOS del mes anterior (Etapa H.6). La liquidación de
--     comisiones compara el mes con el anterior; con todo el relato comprimido
--     en el mes en curso, el mes pasado daba cero. Se siembran 5 entregados
--     repartidos entre los tres vendedores, solo si no hay ningún entregado
--     anterior al mes en curso (así también entra en una base ya sembrada).
--     * Numerados a continuación del último Id (el número real lo pone
--       PedidoHooks a partir del Id); son historia, no salen en el kanban del mes.
--     * Kardex: cada salida tiene su entrada previa por la misma cantidad
--       (contenedor recibido el mes pasado), así las existencias de HOY no
--       cambian y el relato del stock (SKU con 2 unidades, el negativo) sigue.
--     * Sin envío: los del mes pasado ya se cerraron; el tracking no los necesita.
-- ============================================================================
IF NOT EXISTS (SELECT 1 FROM PC_PEDIDOS WHERE Estado = N'entregado' AND Fecha < DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1))
   AND EXISTS (SELECT 1 FROM PC_PEDIDOS)
BEGIN
    DECLARE @iniMes DATETIME2 = CAST(DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1) AS DATETIME2);
    DECLARE @depCentralAnt INT = (SELECT TOP 1 Id FROM PC_DEPOSITOS WHERE Codigo = N'DEP-CENTRAL');
    DECLARE @seq INT = (SELECT ISNULL(MAX(Id), 0) FROM PC_PEDIDOS);

    INSERT INTO PC_PEDIDOS (Numero, Fecha, ClienteId, VendedorId, DepositoId, AgenciaId, Estado, TotalUsd, ComisionUsd, Observaciones, EnvioId, MotivoAnulacion)
    SELECT N'PED-' + RIGHT('0000' + CAST(@seq + x.N AS NVARCHAR(10)), 4),
           DATEADD(DAY, -x.DiasAntesDelMes, @iniMes), c.Id, c.VendedorId, @depCentralAnt, a.Id,
           N'entregado', 0, 0, x.Observaciones, NULL, N''
    FROM (VALUES
        -- N, DíasAntesDelInicioDelMes, Cliente, Agencia, Observaciones
        (1, 22, N'Moto Center Rivera',      N'DAC',          N'Reposición de invierno del norte.'),
        (2, 18, N'Racing Store Montevideo', N'Flota propia', N'Pedido grande de fin de mes.'),
        (3, 13, N'Ruta 5 Motos',            N'DAC',          N'Entregado de mañana, como pidió.'),
        (4,  9, N'El Cruce Motopartes',     N'Mirtrans',     N'Compra chica, cerrada en la visita.'),
        (5,  4, N'Distribuidora Litoral',   N'Mirtrans',     N'Volumen para talleres del litoral.')
    ) AS x(N, DiasAntesDelMes, Cliente, Agencia, Observaciones)
    JOIN PC_CLIENTES c ON c.Nombre = x.Cliente
    LEFT JOIN PC_AGENCIAS a ON a.Nombre = x.Agencia
    WHERE c.VendedorId IS NOT NULL;

    INSERT INTO PC_PEDIDO_LINEAS (PedidoId, VarianteId, Cantidad, PrecioUnitarioUsd, SubtotalUsd)
    SELECT p.Id, v.Id, x.Cantidad, v.PrecioLista, x.Cantidad * v.PrecioLista
    FROM (VALUES
        (1, N'LS2-FF800-5960-BLA', 3), (1, N'FOX-180-L-NEG', 2),     (1, N'LS2-VISIRI-U', 4),
        (2, N'LS2-FF906-5960-GRI', 3), (2, N'SHO-NXR2-5758-BLA', 2), (2, N'ALP-TGPR3-L-ROJ', 2), (2, N'LS2-VISIRI-U', 6),
        (3, N'BEL-MX9-5960-ROJ', 1),   (3, N'ALP-SMX1-L-NEG', 2),
        (4, N'LS2-FF800-5758-ROJ', 1), (4, N'LS2-VISIRI-U', 2),
        (5, N'LS2-FF906-5758-GRI', 2), (5, N'ALP-SMX1-M-ROJ', 3),    (5, N'FOX-180-M-NEG', 2)
    ) AS x(N, Sku, Cantidad)
    JOIN PC_PEDIDOS p ON p.Numero = N'PED-' + RIGHT('0000' + CAST(@seq + x.N AS NVARCHAR(10)), 4)
    JOIN PC_VARIANTES v ON v.Sku = x.Sku;

    UPDATE p SET p.TotalUsd = ISNULL(l.Total, 0)
    FROM PC_PEDIDOS p
    LEFT JOIN (SELECT PedidoId, SUM(SubtotalUsd) AS Total FROM PC_PEDIDO_LINEAS GROUP BY PedidoId) l ON l.PedidoId = p.Id
    WHERE p.Id > @seq;

    UPDATE p SET p.ComisionUsd = ROUND(p.TotalUsd * v.ComisionPorcentaje / 100.0, 2)
    FROM PC_PEDIDOS p
    JOIN PC_VENDEDORES v ON v.Id = p.VendedorId
    WHERE p.Id > @seq;

    -- Entrada previa por la misma cantidad (el contenedor del mes pasado) y la salida del despacho.
    INSERT INTO PC_MOVIMIENTOS_STOCK (VarianteId, DepositoId, DepositoDestinoId, Tipo, Cantidad, CostoUnitario, Motivo, DocumentoOrigen, Fecha, Usuario)
    SELECT l.VarianteId, p.DepositoId, NULL, N'entrada', SUM(l.Cantidad), MAX(v.CostoEstandar),
           N'Recepción contenedor MRKU-3318', N'PO-2026-000', DATEADD(DAY, -26, @iniMes), N'deposito'
    FROM PC_PEDIDO_LINEAS l
    JOIN PC_PEDIDOS p ON p.Id = l.PedidoId
    JOIN PC_VARIANTES v ON v.Id = l.VarianteId
    WHERE p.Id > @seq
    GROUP BY l.VarianteId, p.DepositoId;

    INSERT INTO PC_MOVIMIENTOS_STOCK (VarianteId, DepositoId, DepositoDestinoId, Tipo, Cantidad, CostoUnitario, Motivo, DocumentoOrigen, Fecha, Usuario)
    SELECT l.VarianteId, p.DepositoId, NULL, N'salida', l.Cantidad, v.CostoEstandar,
           N'Despacho del pedido ' + p.Numero, p.Numero, p.Fecha, N'deposito'
    FROM PC_PEDIDO_LINEAS l
    JOIN PC_PEDIDOS p ON p.Id = l.PedidoId
    JOIN PC_VARIANTES v ON v.Id = l.VarianteId
    WHERE p.Id > @seq;

    PRINT 'Seed de dominio: pedidos entregados del mes anterior sembrados.';
END
GO

-- ============================================================================
-- 15. Etapa C — catálogo premium (plan §3.7 / §4.5 / §4.7): ficha técnica,
--     destacados y novedades, foto de portada e historial de precios.
--     * Enriquecimientos de productos: ISNULL por columna, nunca pisan lo editado.
--     * Fotos: SVG livianos (~1 KB) para que el catálogo no se vea vacío en la demo.
--       Se insertan SOLO si todavía no hay documentos de Producto, y se guardan como
--       VARCHAR->VARBINARY (ASCII puro a propósito: sin tildes, para no depender del
--       collation de la base al convertir).
--     * Historial de precios: solo si la tabla está vacía. En producción lo llena
--       VarianteHooks en cada PUT; acá se siembran 2 cambios para que el timeline
--       de la ficha de Variante y el margen de la ficha comercial muestren algo.
-- ============================================================================
UPDATE p SET
      p.FichaTecnica = ISNULL(p.FichaTecnica, x.FichaTecnica)
    , p.Destacado    = ISNULL(p.Destacado, x.Destacado)
    , p.Novedad      = ISNULL(p.Novedad, x.Novedad)
FROM PC_PRODUCTOS p
JOIN (VALUES
    (N'SHO-NXR2', CAST(N'**Casco integral premium de competición.**

- Calota en fibra AIM (multicompuesto de fibras orgánicas y resina).
- Peso: 1.350 g (talla M, +/- 50 g).
- Homologación ECE 22.06. Doble anillo en D.
- Ventilación de 6 entradas y 4 salidas, todas regulables con guante puesto.
- Interior desmontable y lavable, tratamiento antibacterial.
- Pinlock EVO incluido; visor CWR-F2 con Pinlock preinstalado.
- **Talle recomendado:** medir el contorno de la cabeza 2 cm por encima de las cejas.' AS NVARCHAR(MAX)), CAST(1 AS BIT), CAST(0 AS BIT)),

    (N'ALP-TGPR3', N'**Campera textil ventilada, uso urbano y ruta en verano.**

- Chasis en poliéster 600D con paneles de malla en pecho, espalda y mangas.
- Protecciones CE nivel 1 en hombros y codos (bolsillo para espaldera opcional).
- Refuerzos en zonas de impacto y costuras de seguridad en toda la estructura.
- Ajuste de cintura y puños; cuello forrado en neopreno.
- Peso: 1.800 g. Certificación prEN 17092.
- **Talle recomendado:** entallada; quien dude entre dos talles, llevar la mayor.', CAST(1 AS BIT), CAST(0 AS BIT)),

    (N'LS2-FF906', N'**Modular con mentonera abatible 180 grados.**

- Calota en policarbonato KPA, 3 tamaños de calota.
- Doble homologación P/J: se puede circular con la mentonera arriba.
- Peso: 1.700 g. Visera interna solar retráctil.
- Sistema de cierre micrométrico y Pinlock incluido.
- Preparado para intercomunicador (alojamiento de parlantes).
- **Talle recomendado:** el modular calza algo más justo que el integral de la misma talla.', CAST(0 AS BIT), CAST(1 AS BIT)),

    (N'BEL-MX9', N'**Casco off-road con tecnología MIPS.**

- MIPS: capa deslizante que reduce la energía rotacional en impactos oblicuos.
- Calota en policarbonato, forro Velocity Flow con canales de ventilación.
- Peso: 1.450 g. Visera ajustable y desmontable.
- Preparado para gafas: apertura ampliada y goma de sujeción trasera.
- Homologación ECE 22.06.
- **Talle recomendado:** dejar lugar para las gafas; no apretar sobre las sienes.', CAST(0 AS BIT), CAST(1 AS BIT))
) AS x(Codigo, FichaTecnica, Destacado, Novedad) ON p.Codigo = x.Codigo;
GO

-- 15.1 Fotos de portada. SVG en ASCII puro (sin tildes) para que CONVERT no dependa del
--      collation; el front las pide en base64 y arma un data-URL, igual que cualquier foto
--      subida a mano. Bloque atomico: si ya hay documentos de Producto, no toca nada.
IF NOT EXISTS (SELECT 1 FROM PC_DOCUMENTOS WHERE relacionnombre = N'Producto')
BEGIN
    DECLARE @svgCasco VARCHAR(MAX) = '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 400 300" width="400" height="300"><defs><linearGradient id="f" x1="0" y1="0" x2="1" y2="1"><stop offset="0" stop-color="@C1"/><stop offset="1" stop-color="@C2"/></linearGradient></defs><rect width="400" height="300" fill="url(#f)"/><g transform="translate(200,150)"><path d="M-95 18a95 88 0 0 1 190 0v24a18 18 0 0 1-18 18h-54l-15 22h-69a34 34 0 0 1-34-34z" fill="#0f151d" opacity=".92"/><path d="M-70 4a72 66 0 0 1 140 8c0 10-8 16-18 16h-104c-13 0-20-10-18-24z" fill="#e9eff7" opacity=".88"/><path d="M-95 42h60v18h-60z" fill="#0b1017" opacity=".35"/></g><text x="200" y="272" font-family="Segoe UI,Arial,sans-serif" font-size="17" font-weight="600" fill="#ffffff" opacity=".92" text-anchor="middle">@TXT</text></svg>';
    DECLARE @svgCampera VARCHAR(MAX) = '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 400 300" width="400" height="300"><defs><linearGradient id="f" x1="0" y1="0" x2="1" y2="1"><stop offset="0" stop-color="@C1"/><stop offset="1" stop-color="@C2"/></linearGradient></defs><rect width="400" height="300" fill="url(#f)"/><g transform="translate(200,140)"><path d="M-34-74h14l20 16 20-16h14l46 26-20 44-18-11v83h-84v-83l-18 11-20-44z" fill="#0f151d" opacity=".92"/><path d="M0-58l14 14-14 100-14-100z" fill="#e9eff7" opacity=".75"/><path d="M-42 34h84v10h-84z" fill="#e9eff7" opacity=".35"/></g><text x="200" y="272" font-family="Segoe UI,Arial,sans-serif" font-size="17" font-weight="600" fill="#ffffff" opacity=".92" text-anchor="middle">@TXT</text></svg>';

    INSERT INTO PC_DOCUMENTOS (nombre, extension, contenido, mimetype, fechacarga, relacionid, relacionnombre)
    SELECT x.Nombre, N'.svg',
           CONVERT(VARBINARY(MAX), REPLACE(REPLACE(REPLACE(x.Plantilla, '@C1', x.C1), '@C2', x.C2), '@TXT', x.Texto)),
           N'image/svg+xml', GETDATE(), p.Id, N'Producto'
    FROM PC_PRODUCTOS p
    JOIN (VALUES
        (N'SHO-NXR2',  N'shoei-nxr2',      @svgCasco,   '#1b2a4a', '#0b1220', 'SHOEI NXR2'),
        (N'LS2-FF906', N'ls2-ff906',       @svgCasco,   '#123543', '#08151b', 'LS2 FF906 ADVANT'),
        (N'BEL-MX9',   N'bell-mx9',        @svgCasco,   '#4a2a12', '#1b0f06', 'BELL MX-9 MIPS'),
        (N'ALP-TGPR3', N'alpinestars-tgp', @svgCampera, '#3a1220', '#160709', 'ALPINESTARS T-GP PLUS R V3')
    ) AS x(Codigo, Nombre, Plantilla, C1, C2, Texto) ON p.Codigo = x.Codigo;

    -- La portada es la unica foto que tiene cada uno de esos productos.
    UPDATE p SET p.ImagenPrincipalId = d.id
    FROM PC_PRODUCTOS p
    JOIN PC_DOCUMENTOS d ON d.relacionid = p.Id AND d.relacionnombre = N'Producto'
    WHERE p.ImagenPrincipalId IS NULL;

    PRINT 'Seed de dominio: fotos de portada (SVG) sembradas para 4 productos.';
END
ELSE
    PRINT 'Seed de dominio: ya hay documentos de Producto, se omiten las fotos.';
GO

-- 15.2 Historial de precios. Dos cambios dentro del mes en curso (mismo criterio que los
--      pedidos de la Etapa B: comision y meta se liquidan por mes, sembrar fuera del mes
--      deja las pantallas vacias). En produccion estas filas las escribe VarianteHooks.
-- IFs anidados y no un AND: T-SQL no garantiza corto-circuito, y si la tabla todavia no
-- existiera el NOT EXISTS se evaluaria igual.
IF OBJECT_ID('PC_PRECIO_HISTORIAL', 'U') IS NOT NULL
BEGIN
IF NOT EXISTS (SELECT 1 FROM PC_PRECIO_HISTORIAL)
BEGIN
    DECLARE @inicioMes DATE = DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1);
    DECLARE @hoyPh DATE = CAST(GETDATE() AS DATE);
    -- Hacia atras, pero sin salirse del mes en curso: si hoy es dia 2, ambos caen el dia 1.
    DECLARE @fPrecio DATETIME2 = CAST(IIF(DATEADD(DAY, -6, @hoyPh) < @inicioMes, @inicioMes, DATEADD(DAY, -6, @hoyPh)) AS DATETIME2);
    DECLARE @fCosto  DATETIME2 = CAST(IIF(DATEADD(DAY, -12, @hoyPh) < @inicioMes, @inicioMes, DATEADD(DAY, -12, @hoyPh)) AS DATETIME2);

    -- Suba de precio del Shoei (el destacado): +8% hace unos dias.
    INSERT INTO PC_PRECIO_HISTORIAL (VarianteId, Campo, ValorAnterior, ValorNuevo, Fecha, Usuario)
    SELECT TOP 1 v.Id, N'precio',
           ROUND(v.PrecioLista / 1.08, 2), v.PrecioLista,
           @fPrecio, N'pablo'
    FROM PC_VARIANTES v
    JOIN PC_PRODUCTOS p ON p.Id = v.ProductoId
    WHERE p.Codigo = N'SHO-NXR2'
    ORDER BY v.Id;

    -- Suba de costo de la campera (llego un embarque mas caro): +12%.
    INSERT INTO PC_PRECIO_HISTORIAL (VarianteId, Campo, ValorAnterior, ValorNuevo, Fecha, Usuario)
    SELECT TOP 1 v.Id, N'costo',
           ROUND(v.CostoEstandar / 1.12, 2), v.CostoEstandar,
           @fCosto, N'pablo'
    FROM PC_VARIANTES v
    JOIN PC_PRODUCTOS p ON p.Id = v.ProductoId
    WHERE p.Codigo = N'ALP-TGPR3' AND v.CostoEstandar > 0
    ORDER BY v.Id;

    PRINT 'Seed de dominio: historial de precios sembrado (2 cambios).';
END
ELSE
    PRINT 'Seed de dominio: ya hay historial de precios, se omite.';
END
GO
