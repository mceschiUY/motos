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
  ~~coordenadas de los 8 clientes~~ (Etapa A), ~~fotos de muestra para 4 productos, 2 destacados
  y 2 novedades, y 2 cambios de precio históricos~~ (sección 15, Etapa C: las fotos son SVG
  generados de ~1 KB, ASCII puro, para que la grilla del catálogo no se vea vacía sin engordar
  el repo). **Queda para E**: nada del seed; solo los reportes programados y `doc/demo.md`.

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
- [x] **Producto**: 4 columnas nuevas nullable (`Destacado`, `Novedad`, `FichaTecnica`,
  `ImagenPrincipalId`) en las 6 capas, con `PC_PRODUCTOS.sql` pasado al estilo idempotente de
  `PC_CLIENTES.sql` (ALTER ADD por columna). La subida de fotos ya existía (`app-foto-galeria`
  sobre `PC_DOCUMENTOS`): lo que se agregó es **elegir la portada** con una estrella en la
  miniatura — `foto-galeria` recibe `portadaId` + `permitirPortada` y emite `portadaCambiada`,
  sin conocer el dominio; la ficha de Producto es la que guarda.
- [x] **Ficha comercial** (`/catalogo/:id`, artesanal): foto grande, ficha técnica, matriz
  talla × color con existencias por depósito, precio USD y margen (USD y %) por SKU, y
  "Agregar al pedido" que abre `/pedidos/nuevo?varianteId=N` con el SKU ya en el carrito.
  La matriz arranca de `PC_VARIANTES` con LEFT JOIN al Kardex: el `HAVING <> 0` de
  `ExistenciasQuery` se habría comido justo las celdas agotadas.
- [x] **Catálogo navegable** (`/catalogo`, artesanal): cards con foto, precio "desde",
  stock y SKU, destacados primero, etiqueta "Nuevo", buscador y filtros por marca y categoría
  (el filtro por categoría incluye las hijas). Las fotos viajan en base64 por
  `/api/Documentos/{id}`: `download/{id}` está bajo `[Authorize]` y un `<img src>` no lleva
  el JWT del interceptor.
- [x] **Impresión** de la ficha comercial con el membrete existente y `@media print` (oculta
  galería, filtro de depósito y botones de la matriz).
- [x] **Historial de precios**: `PC_PRECIO_HISTORIAL` (tabla + FK, sin CRUD) que llena
  `VarianteHooks` — valores viejos capturados en `AntesDeModificar`, filas escritas en
  `DespuesDeModificar`, una por campo que cambió. Se lee por
  `GET /api/artesanal/variante/{id}/precios` y se ve como timeline en la ficha de Variante;
  el margen USD y % está en la ficha comercial.

**Nota del hook**: el usuario del historial sale del claim `Name` del JWT (como
`GET /api/Vendedor/mio`) y no de `ICurrentUserService`: en Development ese servicio es el mock
y devuelve `dev_user`, y el timeline tiene que mostrar quién tocó el precio.

**Arreglo al pasar**: `Limpiar_Datos_Dominio.sql` no borraba `PC_PEDIDOS` ni
`PC_PEDIDO_LINEAS` (quedó pendiente de la Etapa B) — con `PC_PEDIDOS.EnvioId` apuntando a
`PC_ENVIOS`, el `DELETE FROM PC_ENVIOS` fallaba por FK. Se agregaron en el orden correcto
junto con `PC_PRECIO_HISTORIAL`.

Verificado el 2026-09-09 contra la base local (LocalDB) por API: catálogo con 10 productos,
2 destacados primero, 2 novedades y 4 con portada; el buscador `?q=shoei` devuelve el SKU
esperado. Ficha comercial del Shoei: 8 SKU en la matriz, margen 690 − 380 = US$ 310 (81,58 %);
**filtrada por el depósito Showroom Centro deja 6 celdas en cero visibles**, que es justo lo que
el `HAVING <> 0` de `ExistenciasQuery` habría ocultado. El hook de precios escribe una fila al
cambiar el precio (690 → 725, +5,07 %, usuario `pablo` del JWT), **no** escribe nada si el PUT
manda el mismo valor, y escribe dos filas si cambian precio y costo a la vez. El PUT de Producto
con `imagenPrincipalId` (lo que hace la estrella de la galería) cambia la portada sin pisar el
resto. Segundo arranque: el seed rellenó la ficha técnica que se había borrado a mano y no
duplicó ni documentos ni historial — la sección 15 es idempotente. Las 4 portadas SVG pesan
3 KB en total. `dotnet build` y `ng build --configuration development` en verde.

**Falta**: el recorrido por pantalla (`/catalogo`, `/catalogo/:id`, impresión y estrella de
portada) no se probó en el navegador; el front está verificado solo por compilación.

### Etapa D — Escenas: el sitio se navega por relato, no por tablas (2026-09-12)
Replanteo aprobado por Pablo: el problema no era cómo se veían los ABM sino **qué** se veía
(fichas como `/movimientostock/1` que no dicen nada, 25 entradas de menú). Sin tocar `core/`
ni el generador. Dos capas: arriba las **escenas** artesanales que cuentan un día en la
distribuidora y se enlazan entre sí; abajo los CRUD generados, plegados en el grupo
**Administración** del menú. Truco: las rutas artesanales van antes de `GENERATED_ROUTES`
en `app.routes.ts`, así `cliente/:id`, `vendedor/:id`, `variante/:id`, `movimientostock/:id`
y `envio/:id` caen en la escena sin tocar el registry. Guion en `doc/demo.md`.
- [x] **Menú de relato** (`generated-menu.registry.ts`): Hoy · Catálogo (Catálogo, Existencias)
  · Vender (Agenda, Nuevo pedido, Pedidos, Clientes) · Despachar (Envíos) · Equipo (Vendedores,
  Actividades, Comisiones) · Administración (plegado). Ctrl+K: Producto abre `/catalogo/:id`;
  Línea de pedido, Observación, Meta, Movimiento y Parámetro SLA quedan `oculta`
  (`generated-search.registry.ts` + filtro en `header-search`). El layout navega paths con
  barra (`pedidos/nuevo`) y titula las escenas sin menú.
- [x] **Rol de vista** (`layout/rol-vista.service.ts`): Gerencia / Vendedor / Depósito en el
  cabezal. Filtra grupos del menú, oculta "Sistema" y cambia el inicio (Hoy / Agenda / Kanban
  de pedidos). Presentación, no seguridad; vive en `localStorage`.
- [x] **Hoy** (`/`, `modules/artesanal/hoy`, `GET api/artesanal/centro-control`): feed de
  acciones por urgencia (SLA vencido/advertencia, pedidos a despachar, stock bajo/negativo,
  metas rezagadas, clientes sin visitar), 5 KPI con acento, pipeline del mes, equipo con
  metas, top productos, stock por depósito y actividad reciente. Umbral de stock bajo:
  `stock.umbral_bajo` de Configuración si existe, si no 3. El home genérico quedó en
  `/inicio-generico`. Modo TV **no** se tocó (sigue con los slides genéricos).
- [x] **Ficha de SKU** (`/variante/:id`, `modules/artesanal/sku`, `GET api/artesanal/sku/{id}`):
  stock por depósito, comprometido, cobertura en días, margen, Kardex con saldo corrido y
  `PED-000n` clickeable, pedidos abiertos, historial de precios; "Registrar movimiento" y
  "Editar datos" abren los forms generados como diálogo. `/movimientostock/:id` redirige a
  la ficha con el movimiento resaltado.
- [x] **Cliente 360** (`/cliente/:id`, `modules/artesanal/cliente360`,
  `GET api/artesanal/cliente/{id}/360`): identidad, semáforo con motivo, KPI, línea de tiempo
  única (actividades + pedidos + envíos), top productos, mapa, "Registrar visita", "Nuevo
  pedido" (`/pedidos/nuevo?clienteId=N`), "Editar datos", WhatsApp.
- [x] **Panel del vendedor** (`/vendedor/:id`, `modules/artesanal/vendedor-panel`,
  `GET api/artesanal/vendedor/{id}/panel` + los existentes `avance` y `agenda`): ranking,
  meta con "esperado a hoy", comisión sellada y proyectada, slider "¿y si vende más?",
  ventas por semana, esta semana, mis clientes con semáforo, pedidos abiertos.
- [x] **Seguimiento del envío** (`/envio/:id`, `modules/artesanal/seguimiento`,
  `GET api/artesanal/envio/{id}/seguimiento`): línea de estados con SLA por etapa
  (`PC_PARAMETROSLAS`: umbral/límite desde la fecha de la etapa actual), pedido origen,
  bitácora, acciones del ciclo, impresión, link público + **QR** (`qrcode`, nueva dependencia
  en `web/package.json`). **Público** `/seguimiento/:codigo` sin login ni layout sobre
  `GET api/publico/seguimiento/{codigo}` (`[AllowAnonymous]`; sin precios, totales,
  teléfonos ni usuarios).
- [x] **Kanban por defecto en Pedidos** y **form de Cliente completo**: ver "shells tocados".
- [ ] Umbral de stock bajo y días sin visita como claves de Configuración del sitio (hoy el
  home lee `stock.umbral_bajo` si existe; la clave no está en `Cfg_ConfiguracionSitio.sql`).
- [ ] Modo pantalla con los KPI y el feed de Hoy.

**Shells generados tocados** (la regen los pisa; rehacer si se regenera):
`pedido-list.component.ts` (`loadViewMode` cae a `kanban` en vez de `table`);
`cliente-form.component.{ts,html}` (tipo, ciudad, contacto, email, vendedor, notas,
latitud/longitud plegados: el backend ya los aceptaba desde la Etapa A, el form no los exponía);
`generated-menu.registry.ts` y `generated-search.registry.ts` (campo `oculta`);
`header-search.component.ts` (filtra `oculta`). Fuera de shells: `armado-pedido` acepta
`?clienteId=`, `agenda.model.ts` suma `comisionUsd`.

Verificado el 2026-09-12 contra la base local por API con el seed: `centro-control` devuelve
12 acciones (4 envíos con SLA vencido, 1 SKU negativo, 2 pedidos preparados, 3 SKU con 2
unidades, 1 confirmado, 1 cliente sin visita hace 47 días), pipeline de 11 pedidos, 3
vendedores con meta; `cliente/2/360` (Casa Bike Salto) semáforo verde con 8 hitos en la línea
de tiempo; `vendedor/1/panel` ranking 2 de 3 y 8 semanas; `envio/11/seguimiento` SLA
`vencido` (14 días en facturación, límite 4); `publico/seguimiento/MT-2026-0111` responde
200 sin token y sin teléfono. `dotnet build` y `ng build --configuration development` en verde.
**Falta**: el recorrido por pantalla de las escenas no se probó en el navegador (sin
herramienta de browser en la sesión); el front está verificado por compilación y por los
endpoints que consume.

### Etapa E — Pulido de demo
- [x] Seed extendido: hecho en A, B y C; no se agregó volumen en D (los offsets actuales ya
  dan material para cada escena: PED-0007/0008 preparados, MT-2026-0111 fuera de SLA, SKU con
  2 unidades y uno negativo, 1 visita hoy).
- [x] Recorrido de demo documentado en `doc/demo.md` (guion de 10 minutos, 6 escenas).
- [ ] Reportes programados "Stock bajo" y "Clientes sin visitar" sobre las mismas consultas de
  las alertas. **Ojo**: los reportes programados se guardan pero no hay scheduler que los
  ejecute (no existe `IHostedService` ni envío de email en el repo); no mostrarlos en la demo.
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
