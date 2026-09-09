# Plan de producto — Motos: catálogo, fuerza de ventas y control de la operación

> Documento funcional orientado a **demo**. Reemplaza al plan logístico completo (importación,
> landed cost, facturación, RMA…) que quedó en el historial de git (`doc/plan.md` en `7329d6f`).
> Todavía no sabemos hacia dónde va el producto: este plan elige lo que **se puede mostrar en
> una demo y que sirve a cualquier rumbo**: un catálogo que luzca, vendedores que salen a vender,
> pedidos, comisiones, contacto con los clientes y un tablero de control.
>
> Regla de los .md (heredada de CESKIA): lo escrito tiene que ser REAL y verificable contra el
> código. Ante la duda, el código manda. Leyenda: `- [ ]` pendiente · `- [~]` en curso · `- [x]` hecho.

---

## 1. Qué hay hoy (base sobre la que se construye, no se toca)

| Bloque | Estado | Qué hace |
|---|---|---|
| **Catálogo** | ✅ | Marca, Categoría (jerárquica), Talla, Color, Producto (con atributos de casco y homologación), Variante/SKU = producto × talla × color con costo y precio de lista. `doc/modelo-catalogo.md`. |
| **Stock** | ✅ | Depósito (multi-depósito), Movimiento de stock (Kardex inmutable), Existencias calculadas y pantalla de solo lectura `/existencias`. `doc/modelo-stock.md`. |
| **Envíos** | ✅ | Cliente, Agencia (transportista), Envío con estados `recibido → facturado → despachado → entregado | anulado`, Observaciones, Parámetros SLA por etapa. |
| **Plataforma** | ✅ | Seguridad (usuarios/perfiles/roles), auditoría, reportes programados, configuración del sitio, documentos adjuntos (`PC_DOCUMENTOS`, por entidad e id), tabla genérica con pills de enum, búsqueda global, asistente de voz. |
| **Datos de demo** | ✅ | `Scripts/Seed_Dominio_Motos.sql`: 6 marcas, 10 productos, 73 SKU, Kardex, 8 clientes, 15 envíos. Idempotente. Limpieza: `Limpiar_Datos_Dominio.sql`. |

**Reglas de compatibilidad para todo lo nuevo:**
1. Tablas nuevas `PC_*` con el patrón de 6 capas (`CLAUDE.md`). Las existentes solo reciben **columnas nuevas nullable** (DbBootstrap hace ALTER ADD, nunca DROP).
2. **Solo dólares.** Todo precio, total y comisión es USD. No hay entidad Moneda ni tipo de cambio. La UI muestra `US$`.
3. El Envío actual **no cambia de significado**: sigue siendo el tracking. Un Pedido despachado *genera* un Envío y lo referencia. Un envío puede seguir existiendo sin pedido.
4. El stock **no se edita**: un pedido despachado genera movimientos de `salida` en el Kardex con `DocumentoOrigen = 'PED-000n'`.
5. Vistas de tablero y liquidaciones van en zona **artesanal** (`web/src/app/modules/artesanal/`, `Application/Artesanal/`), como Existencias. Los CRUD van generados.
6. Reglas de negocio en `*Hooks.cs`, nunca en los handlers generados.

---

## 2. La historia que cuenta la demo

> *Un vendedor sale a recorrer tiendas de moto en el interior. En cada visita registra qué pasó
> (compró, no compró, volver la semana que viene). Si hay pedido, lo arma desde el celular con el
> catálogo con fotos y precios en dólares, eligiendo talla y color. El pedido reserva stock,
> el depósito lo prepara y lo despacha por una agencia: ahí nace el envío que ya se rastrea hoy.
> A fin de mes el sistema liquida las comisiones. La gerencia ve todo desde un centro de control:
> ventas del mes, quién vendió, qué se debe entregar, qué SKU se están acabando y qué envíos
> están atrasados.*

Cada frase de esa historia es un módulo de la sección 4.

---

## 3. Modelo de datos nuevo (mínimo)

```
Vendedor 1───N Cliente (VendedorId, nullable)        Producto ─── Documento(s) (imágenes, ya existe)
   │                                                    │
   │1                                                   │1
   └───N Actividad ──N→1 Cliente                        └──N Variante ←──N PedidoLinea N──1 Pedido N──1 Cliente
                                                                                          │           N──1 Vendedor
                                                                                          │           N──1 Depósito
                                                                                          └── 1→0..1 Envío (PedidoId nullable en PC_ENVIOS)
```

### 3.1 `PC_VENDEDORES` — Vendedor
| Campo | Tipo | Nota |
|---|---|---|
| Id | INT IDENTITY PK | |
| Nombre | NVARCHAR(120) NOT NULL | |
| Telefono, Email | NVARCHAR | |
| Zona | NVARCHAR(80) | Texto libre; en el seed: "Litoral", "Norte", "Montevideo y Este" |
| ComisionPorcentaje | DECIMAL(5,2) NOT NULL default 0 | % sobre el total USD de pedidos **entregados** |
| Usuario | NVARCHAR(80) NULL | Login del usuario del sitio para que el vendedor vea "lo mío" (`GET /api/Vendedor/mio` compara con el claim `Name` del JWT). Se eligió el login y no un id porque el bypass de desarrollo `pablo` no existe en `Seg_Usuarios`. |
| Activo | BIT | |

### 3.2 `PC_CLIENTES` — columnas nuevas (todas nullable)
| Campo | Tipo | Nota |
|---|---|---|
| Tipo | NVARCHAR(20) | enum `tienda` \| `distribuidor` \| `online` \| `particular` → pill |
| Ciudad | NVARCHAR(80) | Para agrupar rutas de visita |
| Contacto | NVARCHAR(120) | Persona con la que se habla |
| Email | NVARCHAR(120) | |
| VendedorId | INT NULL | FK → PC_VENDEDORES. Vendedor asignado |
| Notas | NVARCHAR(1000) | "Cierra 13 a 15", "paga a 30 días" |

### 3.3 `PC_ACTIVIDADES` — Actividad comercial (el CRM mínimo)
| Campo | Tipo | Nota |
|---|---|---|
| Id | INT IDENTITY PK | |
| VendedorId, ClienteId | INT NOT NULL | FKs |
| Tipo | NVARCHAR(20) NOT NULL | enum `visita` \| `llamada` \| `whatsapp` \| `email` → pill |
| Fecha | DATETIME2 NOT NULL | Cuándo ocurrió (timeline y calendario del generador) |
| Resultado | NVARCHAR(20) NOT NULL | enum `pedido` \| `sin_pedido` \| `reprogramar` \| `sin_contacto` → pill |
| Notas | NVARCHAR(1000) | Qué se habló |
| ProximaAccion | DATE NULL | "Volver el 15" → alimenta la agenda del vendedor |
| PedidoId | INT NULL | Si la visita terminó en pedido |

### 3.4 `PC_PEDIDOS` — Pedido de venta
| Campo | Tipo | Nota |
|---|---|---|
| Id | INT IDENTITY PK | |
| Numero | NVARCHAR(20) NOT NULL único | `PED-000n`, lo genera el Hook al crear |
| Fecha | DATETIME2 NOT NULL | |
| ClienteId, VendedorId, DepositoId | INT NOT NULL | Depósito desde el que se prepara |
| Estado | NVARCHAR(20) NOT NULL | enum `borrador` → `confirmado` → `preparado` → `despachado` → `entregado` · `anulado` → pills |
| TotalUsd | DECIMAL(18,2) NOT NULL | Suma de líneas, lo recalcula el Hook |
| ComisionUsd | DECIMAL(18,2) NOT NULL default 0 | `TotalUsd × Vendedor.ComisionPorcentaje / 100`, se fija al **entregar** |
| Observaciones | NVARCHAR(500) | |
| EnvioId | INT NULL | Se completa al despachar |
| MotivoAnulacion | NVARCHAR(250) | |

### 3.5 `PC_PEDIDO_LINEAS` — Línea de pedido
| Campo | Tipo | Nota |
|---|---|---|
| Id | INT IDENTITY PK | |
| PedidoId, VarianteId | INT NOT NULL | |
| Cantidad | DECIMAL(18,2) NOT NULL > 0 | |
| PrecioUnitarioUsd | DECIMAL(18,2) NOT NULL | Copia de `Variante.PrecioLista` al momento del pedido |
| SubtotalUsd | DECIMAL(18,2) NOT NULL | Cantidad × precio |

### 3.6 `PC_ENVIOS` — columna nueva
| Campo | Tipo | Nota |
|---|---|---|
| PedidoId | INT NULL | Enlace al pedido que lo originó. Nullable: los envíos sueltos siguen valiendo |

### 3.7 `PC_PRODUCTOS` — columnas nuevas (catálogo premium)
| Campo | Tipo | Nota |
|---|---|---|
| Destacado | BIT NULL | Aparece primero en el catálogo |
| Novedad | BIT NULL | Etiqueta "Nuevo" |
| FichaTecnica | NVARCHAR(MAX) NULL | Markdown simple: peso, materiales, certificaciones, talle recomendado |
| ImagenPrincipalId | INT NULL | Id en `PC_DOCUMENTOS` (las imágenes ya se adjuntan por Documentos) |

### 3.8 Tablas y columnas de los extras (§4.8)
| Tabla / columna | Tipo | Nota |
|---|---|---|
| `PC_CLIENTES.Latitud`, `.Longitud` | DECIMAL(9,6) NULL | Para la ruta del día en el mapa |
| `PC_METAS` (Id, VendedorId, Periodo `YYYY-MM`, ObjetivoUsd) | tabla nueva | Única por vendedor y período; CRUD generado, se edita desde la ficha del vendedor |
| `PC_PRECIO_HISTORIAL` (Id, VarianteId, Campo `precio`\|`costo`, ValorAnterior, ValorNuevo, Fecha, Usuario) | tabla nueva | Solo inserta el Hook de Variante; sin form de alta, se ve como timeline en la ficha |
| Configuración del sitio: `stock.umbral_bajo` (3), `crm.dias_sin_visita` (30) | claves en `Cfg_ConfiguracionSitio` | Parámetros de las alertas |

**Reglas de dominio (en Hooks):**
- `Pedido.confirmar`: al menos una línea; verifica **existencias** por SKU en el depósito (consulta `ExistenciasQuery`); si falta stock avisa pero deja confirmar (backorder simple: el aviso queda en Observaciones).
- `Pedido.despachar`: genera una `salida` en el Kardex por cada línea (`DocumentoOrigen = Numero`), crea el **Envío** (`recibido`, cliente y agencia elegidos, `PedidoId`) y guarda `EnvioId`.
- `Pedido.entregar`: fija `ComisionUsd`. Si el Envío vinculado pasa a `entregado`, el Hook de Envío entrega el pedido (sincronía en una sola dirección para no complicar).
- `Pedido.anular`: si ya estaba despachado, genera `entrada` de reversa en el Kardex.
- `Actividad` con `Resultado = pedido` exige `PedidoId`.

---

## 4. Módulos (qué se construye)

### 4.1 Vendedores y clientes
- **Vendedor** (maestra CRUD generada) con comisión y zona.
- **Cliente enriquecido**: tipo, ciudad, contacto, vendedor asignado, notas. Ficha con pestañas: actividades, pedidos, envíos (relaciones `by-cliente`).
- **Filtro "mis clientes"**: si el usuario logueado tiene un Vendedor con su login en `Usuario`, la agenda arranca filtrada por él y la lista de clientes ofrece el botón (`/cliente?vendedorId=N`).

### 4.2 Actividad comercial (contacto con el cliente)
- **Actividad** CRUD generada con vistas `table`, `cards`, `timeline` y `calendario` (el generador las emite al tener `Fecha`).
- **Agenda del vendedor** (artesanal, `/agenda`): hoy y próximos 7 días por `ProximaAccion` y actividades planificadas, agrupado por ciudad. Botón "Registrar visita" precargado con cliente y vendedor.
- **Registro rápido desde el celular**: el form de Actividad con 4 campos visibles (cliente, tipo, resultado, notas) y el resto plegado. Es el mismo form generado con `enLista/primario` bien elegidos, no un form aparte.

### 4.3 Pedidos
- **Pedido + Líneas** generados, con ficha que muestra líneas, totales USD, estado como pill y acciones de ciclo (`confirmar`, `preparar`, `despachar`, `entregar`, `anular`) usando las acciones custom del descriptor.
- **Armado de pedido** (artesanal, `/pedidos/nuevo`): buscador de SKU con foto, talla y color, muestra existencias del depósito elegido, precio de lista USD y total en vivo. Es la pantalla "que vende" en la demo.
- **Vista kanban** de pedidos por estado (el generador ya tiene kanban con matriz de transiciones; se declara en el descriptor).
- Al despachar, elegir Agencia: nace el Envío y desde ahí sigue el tracking de siempre.

### 4.4 Comisiones
- **Liquidación de comisiones** (artesanal read-only, `/comisiones`): por vendedor y mes, pedidos entregados, total USD, comisión USD, con export CSV. Query en `Application/Artesanal`. Sin tabla nueva: se calcula desde `PC_PEDIDOS`.
- **KPI en la ficha del vendedor**: vendido este mes, comisión acumulada, visitas del mes, tasa de cierre (`pedido / actividades`).

### 4.5 Catálogo premium
- **Imágenes**: usar Documentos (`PC_DOCUMENTOS` con `relacionnombre = 'Producto'`) para subir fotos; `ImagenPrincipalId` marca la portada. Miniatura en la lista y en las cards.
- **Ficha de producto comercial** (artesanal, `/catalogo/:id`): foto grande, marca, ficha técnica en markdown, matriz talla × color con existencias y precio USD, botón "Agregar al pedido".
- **Catálogo navegable** (artesanal, `/catalogo`): grilla de cards con foto, por marca y categoría, destacados primero, etiqueta "Nuevo", buscador. Pensado para mostrarle al cliente en la tienda.
- **Impresión**: la ficha se imprime con el membrete existente (`membrete-impresion`) para dejarle una hoja al cliente.

### 4.6 Centro de control (home)
- Reemplazar `ProductoHomeComponent` por una **sala de control artesanal** (`/`), como en trenes:
  - **Hoy**: pedidos nuevos, pedidos a despachar, visitas planificadas, envíos fuera de SLA.
  - **Mes**: ventas USD vs mes anterior, pedidos por estado (pipeline), top 5 vendedores, top 5 productos.
  - **Stock**: SKU con saldo negativo, SKU con menos de N unidades (N configurable en Configuración del sitio), unidades totales por depósito.
  - **Actividad reciente**: últimas actividades y pedidos (componente `actividad-reciente` ya existe).
  - Cada tarjeta linkea a la lista filtrada correspondiente.
- Un endpoint artesanal `GET /api/artesanal/centro-control` devuelve todo en una llamada.
- El **modo pantalla** (`/pantalla`, para la TV) muestra la misma sala en fullscreen.

### 4.7 Datos de demo
- Extender `Seed_Dominio_Motos.sql`: ~~3 vendedores (uno vinculado a `pablo`), clientes con
  tipo/ciudad/vendedor, 25 actividades con próximas acciones~~ (hecho en Etapa A), ~~12 pedidos en
  distintos estados enlazados a los envíos ya sembrados~~ (hecho en Etapa B), ~~metas del mes~~ y
  ~~coordenadas de los 8 clientes~~ (Etapa A). **Queda para C/E**: fotos de muestra para 4 productos,
  2 destacados y 2 novedades, y 2 cambios de precio históricos.

### 4.8 Extras que suman a la demo (aprobados 2026-09-07)
- **Ruta del día con mapa** (artesanal, dentro de `/agenda`): las visitas planificadas de hoy ordenadas por ciudad y ubicadas en el componente `mini-mapa` existente. Requiere `Latitud`/`Longitud` en Cliente (se cargan a mano o desde el celular con "usar mi ubicación" al registrar la visita). Muestra al vendedor a dónde ir y a la gerencia dónde está cada uno.
- **Metas mensuales por vendedor**: tabla `PC_METAS` (vendedor, año-mes, objetivo USD). Barra de avance y semáforo en la ficha del vendedor y en el centro de control (`vendido / objetivo`). Deja preparado el terreno para comisiones escalonadas sin construirlas ahora.
- **Pedido compartible por WhatsApp**: botón en la ficha del pedido que arma el texto (número, líneas con talla/color, total USD, link a la ficha comercial de cada producto) y abre `https://wa.me/<telefono del cliente>?text=…`. Sin integración ni API: solo un enlace. Es el flujo real del rubro.
- **Alertas de stock bajo y de clientes sin visitar**: dos consultas artesanales read-only que alimentan (a) tarjetas rojas en el centro de control y (b) reportes programados del motor existente. Reglas: SKU con existencias por debajo del umbral configurado (Configuración del sitio, default 3) y clientes activos con más de N días sin actividad de su vendedor (default 30). Convierte el tablero en algo que avisa, no solo que muestra.
- **Historial de precios y margen por SKU**: tabla `PC_PRECIO_HISTORIAL` que el Hook de Variante llena en cada cambio de `PrecioLista` o `CostoEstandar` (valor anterior, nuevo, fecha, usuario). La ficha comercial muestra el margen sobre costo en USD y en %, y la ficha de Variante el historial como timeline. Prepara listas de precios por canal sin construirlas.

---

## 5. Roadmap (orden de ejecución)

### Etapa 0 — Limpieza de lo existente (media jornada, antes de A)
Revisión de lo ya desarrollado con el horizonte de demo (2026-09-07). Nada toca el modelo de datos.
- [x] **Herramientas del generador ocultas** también en `ng serve`: `herramientasZas` ahora exige opt-in `localStorage.setItem('zas.herramientas','1')` (`site-layout.component.ts`). Cubre lens, mutación, Evolution Hub y modo pantalla.
- [x] **Asistente de voz oculto** (comentado en `site-layout.component.html`) hasta que se configure `Claude.ApiKey`. Reactivar = descomentar una línea.
- [x] **Código muerto borrado**: `web/src/app/home/`, `global-search.component.ts`, CSS del footer de usuario y `.nav-kbd`.
- [x] **Menú en 4 grupos**: Catálogo · Inventario · **Comercial** (Cliente, Vendedor, Actividad, Meta y Agenda tras la Etapa A) · **Operaciones** (Envío, Agencia, Observación, Parámetro SLA). `modulo` del home alineado.
- [x] **`facturado` se lee "Confirmado"** y `facturacion` "Confirmación", sin tocar valores ni ciclo: `CampoDescriptor.etiquetas` (nuevo, opcional) + `entity-table` lo usa para enums; labels de kanban, ficha, acciones y textos alineados. **Bonus**: la tabla genérica formatea `moneda` en **USD** (antes UYU), coherente con "solo dólares".
- [x] **Ajuste de stock con signo**: `MovimientoStock.Validar` exige cantidad ≠ 0 y solo permite negativo en `ajuste`; form sin `min(0.01)` con hint; seed carga el faltante como `ajuste -1`; `doc/modelo-stock.md` actualizado. Verificado por API: `ajuste -1` → 200, `salida -1` → 400, `0` → 400.
- [x] **Campos de casco condicionales**: `producto-form` muestra tipo/homologación/vencimiento/vigencia solo si la categoría es Casco o hija (`esCasco()`).

Lo que se revisó y **queda como está**: `CostoEstandar` en Variante y `CostoUnitario` en Kardex (los usan margen e historial de precios); Observaciones del envío junto a Actividades del cliente (bitácora del paquete vs. contacto comercial, no se pisan); el resto de la plataforma (seguridad, auditoría, reportes, documentos, configuración).

### Etapa A — Vendedores, clientes y contacto
- [x] **Vendedor**: 6 capas (`PC_VENDEDORES`, `VendedorHooks`, `api/Vendedor` con `mio` que compara
  el claim `Name` del JWT) + alta en los 3 registries del front + seed de 3 vendedores.
- [x] **Cliente**: 8 columnas nuevas nullable (tipo, ciudad, contacto, email, vendedorId, notas,
  latitud, longitud), descriptor y form al día, `GET /api/Cliente/by-vendedor/{id}` y pestaña
  **Actividades** en la ficha. La pestaña *Pedidos* queda para la Etapa B.
- [x] **Actividad**: 6 capas con vistas table/cards/master-detail/timeline/calendario y pills de
  `tipo` y `resultado` + seed de 25 actividades. El shell generado venía con el ciclo de Envío
  (kanban, `pasarA*`, `codigoRastreo`): se sacó — la actividad no tiene ciclo, su "estado" es el
  resultado. Filtros por `?clienteId=` y `?vendedorId=`.
- [x] **Agenda del vendedor** (`/agenda`, artesanal) con paradas por día y ciudad, y **filtro
  "mis clientes"** en la lista de clientes: el botón y el link de la agenda comparten el mismo
  estado en la URL (`/cliente?vendedorId=N`), que resuelve `by-vendedor` en el servidor.
- [x] **Ruta del día con mapa**: `Latitud`/`Longitud` en Cliente, `mini-mapa` en la agenda y
  "usar mi ubicación" al registrar la visita. Los 8 clientes del seed tienen coordenadas reales.
- [x] **Metas mensuales**: `PC_METAS` (6 capas, única por vendedor y período) + barra de avance
  con el endpoint artesanal `GET /api/artesanal/vendedor/{id}/avance` (objetivo, actividades,
  visitas, tasa de cierre, clientes sin visitar). `VendidoUsd` queda en 0 hasta que haya pedidos.

Verificado el 2026-09-08 contra la base local: 3 vendedores (el de "Montevideo y Este" enlazado al
login `pablo`), 25 actividades (9 con próxima acción en los próximos 7 días), 3 metas del mes,
8 clientes con vendedor y coordenadas; `Vendedor/mio` devuelve el vendedor de `pablo`, la agenda
10 paradas y la alerta de clientes sin visitar los 2 esperados. `dotnet build` y
`ng build --configuration development` en verde.

### Etapa B — Pedidos y comisiones
- [x] **Pedido + Línea**: 6 capas (`PC_PEDIDOS`, `PC_PEDIDO_LINEAS`, `api/Pedido`, `api/PedidoLinea`).
  El ciclo es dato (`ciclos-vida.json`) y los efectos viven en `PedidoHooks`: número `PED-000n`
  al crear, **salida de Kardex** por línea y **Envío** al despachar, **comisión** al entregar y
  **entrada de reversa** al anular lo ya despachado. Los tres efectos son idempotentes (se miran
  contra el Kardex). `PedidoLineaHooks` recalcula el total, copia el precio de lista cuando no
  viene y **congela las líneas** fuera de borrador/confirmado.
- [x] **`PC_ENVIOS.PedidoId`** + `EnvioHooks.DespuesDeAccion`: entregar el envío **entrega el
  pedido**. Sincronía en una sola dirección, como decidía el plan.
- [x] **Armado de pedido** (`/pedidos/nuevo`, artesanal): buscador de SKU que muestra existencias
  del depósito elegido y precio USD, carrito con total en vivo y aviso cuando se pide más de lo
  que hay. Crea la cabecera y las líneas, y deja el pedido en borrador.
- [x] **Kanban de pedidos** con la matriz del ciclo (arrastrar = ejecutar la transición) y las
  cinco acciones en la tabla y en la ficha, con el proceso guiado paso a paso.
- [x] **Liquidación de comisiones** (`/comisiones`, artesanal read-only) por vendedor y mes con
  export CSV, y **KPIs en la ficha del vendedor**: barra de meta, vendido, comisión, visitas y
  tasa de cierre (`GET /api/artesanal/comisiones` y `.../vendedor/{id}/avance`).
- [x] **Compartir por WhatsApp** desde la ficha del pedido: arma el texto (número, líneas con
  talle/color, total USD) y abre `wa.me` con el teléfono del cliente. Sin API, es un enlace.
- [x] **Adelantado de la Etapa E**: el seed siembra 12 pedidos por todo el ciclo con sus líneas,
  el stock que salió, el enlace con los envíos ya sembrados y las visitas que terminaron en
  pedido — sin datos, ni el kanban ni las comisiones muestran nada.

Se activó además la regla que esperaba a esta etapa: `Resultado = 'pedido'` en una Actividad
**exige** el `PedidoId` (agregado) y que ese pedido exista (`ActividadHooks`), con su FK.

Verificado el 2026-09-08 contra la base local, ciclo completo por API: alta → dos líneas a
precio de lista (total 707) → confirmar (rechaza sin líneas) → preparar → despachar (2 salidas
de Kardex + envío `PED-0001`) → agregar línea rechazada por despachado → entregar el envío, que
entrega el pedido y sella la comisión (707 × 3,5% = 24,75); y un segundo pedido anulado tras
despachar que devuelve el stock (saldo 3 → 6). Con el seed: 12 pedidos, comisiones del mes
US$ 70,21 y US$ 57,16 para dos vendedores, 6 envíos enlazados. `dotnet build` y
`ng build --configuration development` en verde.

**Nota del seed**: las fechas de pedidos y actividades recientes se comprimen dentro del **mes en
curso** (conservando el orden del relato). Comisión y meta se liquidan por mes: sembrar un día 8
con offsets de 24 días dejaba los entregados en el mes anterior y la pantalla de comisiones vacía.

### Etapa C — Catálogo premium
- [ ] Producto: `Destacado`, `Novedad`, `FichaTecnica`, `ImagenPrincipalId` + subida de fotos por Documentos.
- [ ] Ficha comercial de producto con matriz talla × color, existencias y precio USD.
- [ ] Catálogo navegable con cards y "Agregar al pedido".
- [ ] Impresión de ficha con membrete.
- [ ] Historial de precios: `PC_PRECIO_HISTORIAL` llenada por `VarianteHooks` + timeline en ficha de Variante + margen USD y % en la ficha comercial.

### Etapa D — Centro de control
- [ ] Endpoint artesanal `centro-control`.
- [ ] Sala de control como home + modo pantalla.
- [ ] Umbral de stock bajo y días sin visita en Configuración del sitio.
- [ ] Alertas de stock bajo y clientes sin visitar: consultas artesanales + tarjetas rojas en la sala + avance de metas por vendedor.

### Etapa E — Pulido de demo
- [ ] Seed extendido (vendedores, actividades, pedidos, fotos, metas, coordenadas, historial de precios).
- [ ] Reportes programados "Stock bajo" y "Clientes sin visitar" sobre las mismas consultas de las alertas.
- [ ] Recorrido de demo documentado en `doc/demo.md` (guion de 10 minutos siguiendo la historia de la sección 2).
- [ ] Reportes programados de ejemplo: "Ventas del mes por vendedor" y "Stock bajo".

Estimación relativa: A y C son chicas (maestras + pantallas); B es la mediana (Hooks con reglas); D es artesanal pura. Se puede demostrar algo útil al cerrar A + B.

---

## 6. Fuera de alcance por ahora (estacionado, no descartado)

Todo esto estaba en el plan logístico anterior y vuelve cuando el producto tenga rumbo:
multi-moneda y tipo de cambio · proveedores, órdenes de compra y recepción · importación,
embarques y landed cost · listas de precios por canal y descuentos por volumen · facturación
electrónica y cuenta corriente · devoluciones, cambios de talla y RMA · garantías por serie ·
ubicaciones dentro del depósito · planeamiento de reposición · integraciones (e-commerce,
couriers, fiscal). El modelo de arriba no los bloquea: Pedido es el punto natural para colgar
después facturación y precios; Kardex ya tiene `CostoUnitario` para landed cost.

---

## 7. Decisiones tomadas
- **Solo USD.** Sin Moneda ni tipo de cambio en ninguna tabla.
- **El Envío se conserva** tal cual; el Pedido lo genera y lo referencia. Nada de lo existente cambia de comportamiento.
- **Comisión sobre pedidos entregados**, porcentaje fijo por vendedor. Sin escalas ni metas por ahora.
- **Backorder simple**: se puede confirmar sin stock, con aviso. Sin reserva contable de stock (no hay "comprometido" en existencias).
- **Fotos por Documentos**, no una tabla de imágenes nueva.
- **Home artesanal** reemplaza al dashboard genérico; el genérico queda accesible en `/inicio-generico` por si hace falta.
