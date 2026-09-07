# Plan funcional — Sistema logístico para importación y distribución de indumentaria y cascos de moto

> Documento de análisis funcional. Punto de vista: analista funcional experto en el negocio
> de importación y distribución mayorista de productos para motociclismo (cascos y vestimenta).
> Objetivo: relevar el estado actual del sistema y proponer qué agregar para convertirlo en
> una plataforma logística profesional.

---

## 1. Resumen ejecutivo

El sistema **hoy es un rastreador de envíos con SLA** (última milla): registra envíos con un
código de rastreo, estados y fechas, asociados a un cliente y a una agencia (transportista),
con observaciones y umbrales de alerta por etapa. Es sólido como *tracking*, pero **no cubre el
núcleo de un negocio de importación y distribución**: no hay catálogo de productos, ni stock, ni
compras/importación, ni facturación, ni precios, ni devoluciones.

Para llegar a un sistema logístico profesional del rubro hay que construir la cadena completa:

```
IMPORTACIÓN  →  RECEPCIÓN/ALMACÉN  →  CATÁLOGO+STOCK  →  VENTA B2B  →  FULFILLMENT  →  POSTVENTA
 (compras       (nacionalización,     (SKU talla/color,   (pedidos,     (picking,        (devoluciones,
  al exterior,    landed cost,          homologación,       listas de     packing,          cambios de talla,
  incoterms)      recepción)            lotes/series)       precios)      expedición)       garantías)
```

Este documento propone los módulos, entidades y KPIs para cada eslabón, con foco en las
particularidades de **cascos** (homologación, series, recall, garantía) e **indumentaria**
(temporada, curva de tallas, matriz talla/color).

---

## 2. Estado actual (inventario funcional real)

| Módulo existente | Entidad | Campos clave | Rol |
|---|---|---|---|
| Gestión de clientes | **Cliente** | Nombre, Teléfono, DirecciónEntrega | Destinatario del envío (muy básico) |
| | **Envío** | CódigoRastreo, Estado, FechaRecibido/Factura/Envío/Entrega, MotivoAnulación, ClienteId, AgenciaId | Tracking de paquete por estados |
| Gestión de agencias | **Agencia** | Nombre | Transportista / courier |
| Operaciones y flujo | **Observación** | Texto, FechaHora, Usuario, EnvíoId | Bitácora del envío |
| General | **ParámetroSLA** | Etapa, UmbralAdvertenciaDías, LímiteDías | Alertas de demora por etapa |
| Config/soporte | **ConceptoExpensas** | Nombre, Descripción, Tipo, Activo | Catálogo de conceptos de gasto |
| Transversal | Seguridad (Usuarios/Perfiles/Roles/Capabilities), Auditoría (AuditLog), Reportes (programados + historial), Configuración del sitio | | Base técnica ya disponible |

**Fortalezas a reutilizar:** seguridad con perfiles/roles/capabilities, auditoría, motor de
reportes programados, configuración por sitio y una tabla genérica con estados/pills. Todo esto
sirve de cimiento; el trabajo es de **dominio de negocio**, no de plataforma.

**Conclusión del relevamiento:** el alcance actual equivale al ~10-15% de lo que necesita una
distribuidora importadora. Falta todo el eje **producto → stock → compra/importación → venta**.

---

## 3. Diagnóstico de brecha (gap analysis)

| Capacidad de negocio | ¿Existe hoy? | Criticidad |
|---|---|---|
| Catálogo de productos con variantes (talla/color) | ❌ | 🔴 Bloqueante |
| Stock / inventario multi-depósito | ❌ | 🔴 Bloqueante |
| Compras a proveedores | ❌ | 🔴 Bloqueante |
| Gestión de importación (embarques, aduana, landed cost) | ❌ | 🔴 Bloqueante |
| Ventas B2B (pedidos, reservas, backorder) | ❌ | 🔴 Bloqueante |
| Listas de precios / condiciones comerciales | ❌ | 🔴 Alta |
| Facturación y cuenta corriente | ❌ | 🔴 Alta |
| Fulfillment (picking/packing/expedición/remito) | ⚠️ parcial (solo tracking) | 🟠 Alta |
| Devoluciones / cambios de talla / RMA / garantías | ❌ | 🟠 Alta (rubro) |
| Homologación / certificación de cascos | ❌ | 🟠 Regulatorio |
| Trazabilidad por lote / serie | ❌ | 🟠 Media-alta |
| Reposición / planeamiento de demanda (temporada) | ❌ | 🟠 Media |
| Multi-moneda y tipo de cambio | ❌ | 🔴 Alta (importa) |
| KPIs logísticos (rotación, fill rate, OTIF, cobertura) | ❌ | 🟠 Media |
| Integraciones (e-commerce, courier, fiscal, contable) | ❌ | 🟡 Media |

---

## 4. Procesos de negocio objetivo (end-to-end)

### 4.1 Abastecimiento e importación
1. Selección de proveedor (exterior) y negociación (moneda, incoterm, lead time).
2. **Orden de compra (PO)** con talla/color y curva de tallas.
3. **Embarque / importación**: contenedor, factura comercial, packing list, BL/AWB, seguro, flete.
4. **Nacionalización**: despacho aduanero (DUA), aranceles, tributos, gastos de despachante.
5. **Landed cost**: prorrateo de todos los gastos sobre cada SKU → costo real unitario.
6. **Recepción** contra PO/packing list → alta de stock por SKU/talla/color/lote.

### 4.2 Almacén e inventario
7. Ubicación en depósito (zona/rack/estante), stock disponible/comprometido/en tránsito.
8. Movimientos: entradas, salidas, ajustes, transferencias entre depósitos.
9. Inventario físico y conteo cíclico; valorización (PPP/FIFO).

### 4.3 Comercial y venta B2B
10. Alta de cliente mayorista (distribuidor/tienda) con **límite de crédito** y condiciones.
11. **Lista de precios** por canal/cliente; descuentos por volumen.
12. **Pedido de venta**: reserva stock, backorder si falta, curva de tallas sugerida.
13. Aprobación (crédito/stock) → **facturación** → cuenta corriente.

### 4.4 Fulfillment / distribución
14. **Picking** (por pedido/olas), **packing**, control de calidad.
15. **Expedición**: remito, asignación de transportista (las Agencias actuales), guía.
16. **Tracking** hasta entrega + prueba de entrega (POD).  ← *acá encaja lo que ya existe*.

### 4.5 Postventa (crítico en el rubro)
17. **Devoluciones / cambios de talla** (RMA), reingreso a stock.
18. **Garantías** (cascos), reclamos, notas de crédito.

---

## 5. Módulos propuestos

### 5.1 🔴 Catálogo de productos (núcleo)
**Propósito:** definir qué se vende con las particularidades del rubro.
- **Marca** (Bell, Shoei, Alpinestars, LS2, etc.)
- **Categoría / tipo**: casco (integral, modular, jet, cross, off-road) · indumentaria (campera, guantes, botas, pantalón, protección) · accesorios.
- **Producto (modelo)**: marca, categoría, descripción, temporada/colección, género, material, homologación (cascos), peso y volumen (para flete), imágenes, ficha técnica.
- **Variante / SKU** = producto × **talla** × **color** → cada combinación es un SKU con su código, EAN/UPC/barcode, costo, precio. **Matriz talla/color** es el corazón del rubro.
- **Curva de tallas** (size run) por producto: distribución típica (S/M/L/XL…) para compras y análisis.
- **Atributos de casco**: norma de homologación (ECE 22.06, DOT, SNELL), certificado y vencimiento, número de serie/lote, año de fabricación (relevante para recall y regulación).

### 5.2 🔴 Inventario / Stock (WMS liviano)
- **Depósito** (multi-depósito) y **Ubicación** (zona/rack/estante).
- **Stock por SKU/depósito/ubicación/lote**: disponible, comprometido (reservado), en tránsito.
- **Movimiento de stock** (Kardex): entrada, salida, ajuste, transferencia — con motivo, documento origen, costo.
- **Lote / serie**: trazabilidad (cascos por serie → habilita recall y garantía).
- **Valorización**: PPP o FIFO; costo actualizado por landed cost.
- **Inventario físico / conteo cíclico** con diferencias y ajuste.

### 5.3 🔴 Compras e importación
- **Proveedor** (exterior): datos, moneda, incoterm habitual, lead time.
- **Orden de compra (PO)**: líneas por SKU con curva de tallas, moneda, incoterm.
- **Embarque / Importación**: contenedor(es), factura comercial, packing list, BL/AWB, ETA/ETD, estado (en origen, en tránsito, en aduana, nacionalizado).
- **Gastos de importación**: flete internacional, seguro, aranceles, tributos, despachante, almacenaje.
- **Landed cost / costeo**: prorrateo de gastos (por valor, peso o volumen) sobre cada SKU → **costo real de nacionalización**. Sin esto, el margen es ficticio.
- **Recepción**: contra PO/packing list, con diferencias (faltantes/sobrantes/dañados).

### 5.4 🔴 Ventas y distribución B2B
- **Cliente comercial** (extender el Cliente actual): tipo (distribuidor/tienda/e-commerce), RUT/CUIT, condición fiscal, **límite de crédito**, condición de pago, vendedor asignado, canal.
- **Lista de precios** por canal/cliente/moneda + reglas de descuento por volumen.
- **Pedido de venta**: líneas por SKU, reserva de stock, backorder, estado (borrador→confirmado→facturado→despachado→entregado).
- **Vendedor / comisiones** (opcional).

### 5.5 🔴 Facturación y cuentas corrientes
- **Factura / Nota de crédito / débito** (integrable con e-factura fiscal según país: p. ej. CFE-DGI en Uruguay o AFIP en Argentina).
- **Cuenta corriente** de clientes: saldos, vencimientos, aging.
- **Cobranzas** / recibos.

### 5.6 🟠 Fulfillment (evolución de lo actual)
- **Remito / orden de despacho**, picking (lista de armado), packing, control.
- Reutilizar **Envío + Agencia + Observación + SLA** como capa de **transporte/tracking**, pero
  colgada de un **pedido/remito** (hoy el Envío no tiene qué se envía: falta el detalle de líneas).
- **Prueba de entrega (POD)** y estados de transporte.

### 5.7 🟠 Postventa: devoluciones, cambios y garantías
- **RMA / Devolución**: motivo (talla incorrecta, falla, arrepentimiento), inspección, reingreso a stock o descarte.
- **Cambio de talla/color** (flujo express, altísima frecuencia en indumentaria).
- **Garantía** (cascos): registro por serie, plazo, reclamo, resolución.
- **Logística inversa**: guía de retorno, estado.

### 5.8 🟡 Planeamiento y reposición
- **Punto de pedido / stock mín-máx** por SKU y depósito.
- **Lead time de importación** (meses) → sugerencia de compra anticipada.
- **Forecast estacional** (indumentaria por temporada) y análisis de curva de tallas.
- **Alertas de quiebre** y de sobre-stock/obsolescencia.

---

## 6. Especificidades del rubro (cascos y vestimenta)

| Tema | Por qué importa | Impacto en el modelo |
|---|---|---|
| **Matriz talla × color** | Un mismo modelo son decenas de SKU | Variantes de SKU obligatorias, no opcionales |
| **Curva de tallas** | Se compra/vende por distribución (más M/L que XS/XXL) | Campo en producto + análisis de venta por talla |
| **Homologación (cascos)** | Requisito legal (ECE 22.06/DOT/SNELL) | Atributos + certificado + vencimiento + bloqueo de venta si vencido |
| **Serie / lote (cascos)** | Seguridad y **recall** | Trazabilidad por unidad/lote |
| **Garantía (cascos)** | Postventa frecuente | Módulo de garantías por serie |
| **Temporada / colección (indumentaria)** | Estacional, se liquida | Campo temporada + análisis de obsolescencia |
| **Devolución por talla** | Frecuentísima en apparel | RMA/cambio ágil con reingreso a stock |
| **Peso y volumen** | Cascos ocupan mucho → flete caro | Datos para landed cost y costo de envío |
| **Homologado por país** | Normativa distinta por mercado | Atributo de mercado/norma |

---

## 7. KPIs e indicadores logísticos (para el módulo de Reportes ya existente)

- **Inventario:** rotación, cobertura (días de stock), valorización, aging/obsolescencia, exactitud de inventario.
- **Servicio:** fill rate, **OTIF** (on-time in-full), lead time de entrega, % backorder, quiebres de stock.
- **Importación:** costo de nacionalización por embarque, desvío costo estimado vs real, lead time de importación, tipo de cambio realizado.
- **Comercial:** venta por marca/categoría/talla/color, margen por SKU (con landed cost real), ventas por canal/vendedor, top/bottom sellers.
- **Postventa:** % devoluciones (por talla vs falla), tiempo de resolución de RMA, garantías por marca.

---

## 8. Integraciones (fase avanzada)

- **E-commerce / marketplaces** (Tienda Nube, Shopify, Mercado Libre): sincronización de stock y pedidos.
- **Transportistas / couriers**: API de guías y tracking (extiende las Agencias actuales).
- **Fiscal / e-factura** (DGI-CFE / AFIP según país).
- **Contable / ERP**: asientos de compras, ventas, inventario.
- **Código de barras / lectores RF**: recepción, picking y conteo por scanner (EAN/QR).
- **EDI / catálogos de proveedores** para carga de PO.

---

## 9. Roadmap por fases

> **Leyenda de seguimiento:** `- [ ]` pendiente · `- [x]` hecho · `- [~]` en curso.
> Marcá cada ítem a medida que se ejecuta.

### Fase 0 — Cimientos de dominio (habilitadores)
- [ ] Multi-moneda + tipo de cambio.
- [ ] Depósitos y ubicaciones.
- [ ] Extender Cliente (fiscal, crédito, canal) y crear Proveedor.

### Fase 1 — MVP logístico (lo mínimo para operar)
- [x] **Catálogo** con variantes talla/color (SKU) + marcas/categorías. Maestras (Marca, Categoría, Talla, Color) + **Producto** + **Variante/SKU** completos en las 6 capas. Ver `doc/modelo-catalogo.md §8`.
- [~] **Stock** por SKU/depósito con Kardex de movimientos. Depósito + Movimiento de stock (Kardex) + query de Existencias completos en backend y CRUD en front. Ver `doc/modelo-stock.md`. Pendiente: pantalla de existencias (solo-lectura) en el front.
- [ ] **Compra + Recepción** básica (PO → recepción → alta de stock).
- [ ] **Pedido de venta + reserva de stock** y **remito**.
- [ ] Enganchar el **Envío/tracking actual** al remito (líneas de qué se envía).

### Fase 2 — Importación y comercial completo
- [ ] **Importación + landed cost** (embarques, gastos, prorrateo, costo real).
- [ ] **Listas de precios** y condiciones comerciales.
- [ ] **Facturación + cuenta corriente**.
- [ ] **Devoluciones / cambios de talla / RMA**.

### Fase 3 — Profesionalización
- [ ] **Garantías (cascos)** + trazabilidad por serie/lote + homologación.
- [ ] **Planeamiento de reposición** (mín/máx, temporada, forecast).
- [ ] **KPIs / tablero logístico**.
- [ ] **Integraciones** (e-commerce, courier, fiscal).

---

## 10. Quick wins sobre lo existente (bajo esfuerzo, alto valor)

- [ ] **Detalle del Envío**: hoy un envío no dice *qué* contiene. Agregar líneas (SKU + cantidad) lo conecta con producto/stock.
- [ ] **Enriquecer Cliente**: documento fiscal, dirección fiscal vs entrega, canal, condición de pago.
- [ ] **Enriquecer Agencia**: tipo (courier/flota propia), zonas que cubre, costo/tarifa, tiempos.
- [ ] **Reusar estados + pills de color** (ya en la tabla genérica) para los nuevos flujos (pedido, importación, RMA).
- [ ] **Aprovechar el motor de Reportes** para los primeros KPIs (stock, ventas) apenas exista el catálogo.

---

## 11. Riesgos y consideraciones

- **Costeo de importación (landed cost):** si no se implementa bien, el margen y los precios quedan mal calculados — es el punto más sensible del negocio importador.
- **Fiscal por país:** la facturación electrónica cambia según jurisdicción; conviene aislarla detrás de una interfaz.
- **Regulatorio (cascos):** homologación obligatoria; el sistema debería impedir vender casco sin homologación vigente.
- **Multi-moneda:** compras en USD/EUR y ventas en moneda local exigen tipo de cambio por documento y ajustes.
- **Migración:** el modelo actual (Envío/Cliente/Agencia) se conserva como capa de *transporte*; el nuevo núcleo (producto/stock/compra/venta) se construye alrededor.

---

## 12. Próximos pasos sugeridos

- [ ] Validar con negocio el **alcance del MVP (Fase 1)** y priorizar.
- [ ] Definir el **modelo de datos del catálogo** (producto ↔ variante/SKU ↔ talla/color) — es la piedra angular.
- [ ] Definir la política de **valorización de stock** (PPP vs FIFO) y de **landed cost**.
- [ ] Relevar requisitos **fiscales y de homologación** del/los mercado(s) objetivo.
