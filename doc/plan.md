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
  `/inicio-generico`.
- [x] **Modo pantalla** (`/pantalla`, `pantalla.component.ts`) reescrito sobre el mismo
  endpoint `centro-control`: cuatro slides que rotan cada 15 s (KPI del día + qué hacer hoy,
  el equipo contra la meta, pedidos del mes + top productos, recién pasó), reloj y fecha,
  refresco cada 60 s. Se abre desde el botón de TV del home (el del cabezal sigue gateado
  por `herramientasZas`).
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
- [x] **Existencias agrupadas** (`/existencias`, reescrita el 2026-09-12 tras el recorrido de
  Pablo: "no tendríamos que mostrar por categoría"): deja de ser la lista plana de 73 filas
  SKU × depósito y pasa a ser la escena "¿qué se está acabando?". `GET api/artesanal/existencias`
  (`Catalogo/Existencias`, en `SkuArtesanalController`): una fila por SKU activo (aunque no
  tenga movimientos: el cero es el dato) con producto, marca, categoría y **categoría raíz**
  (CTE recursiva sobre `CategoriaPadreId`), saldo por depósito y total, comprometido en
  pedidos abiertos, ventas de 30 días, cobertura en días y semáforo (`negativo` / `sin_stock`
  / `bajo` con `stock.umbral_bajo` / `ok`). El front agrupa **categoría → producto → SKU** con
  subtotales de unidades, cantidad de SKU y alertas por nivel, plegable (estado en
  localStorage), KPI de unidades, comprometidas y "con alerta" (que es un botón que filtra),
  chip "Solo alertas", filtro por depósito, búsqueda por SKU/producto/marca/talle/color,
  desglose por depósito en cada SKU, cobertura, pill de semáforo, "Agregar al pedido" y click
  al SKU → ficha; export CSV con una columna por depósito. Responsive a 860 y 640 px.
  Verificado por API con el seed: 73 SKU en 3 categorías raíz (Casco 44 SKU / 168 u / 11
  alertas, Indumentaria 28 / 130 / 1, Accesorios 1 / 18 / 0), 12 alertas en total, comprometido
  (visor LS2: 6 u en pedidos abiertos), filtro por Showroom deja 4 SKU con saldo. La lectura
  plana `GET api/MovimientoStock/existencias` sigue existiendo (la usan la ficha comercial y
  el armado de pedido).
- [x] **Alertas configurables**: claves `stock.umbral_bajo` (3) y `crm.dias_sin_visita` (30)
  en `Cfg_ConfiguracionSitio` (grupo `alertas`, tipo `number`), sembradas de forma idempotente
  por clave en `Cfg_ConfiguracionSitio.sql` y `Core_Schema.sql` (entran también en bases ya
  creadas). Las lee `Application/Artesanal/Comun/ParametrosAlertas.cs` y las usan el centro
  de control (KPI, feed, modo TV), `alertas/clientes-sin-visitar` (`dias=0` = configurado),
  `vendedor/{id}/avance`, el panel del vendedor y el cliente 360 (semáforo: verde hasta la
  mitad, amarillo hasta el límite, rojo pasado el límite). Los DTOs devuelven el umbral usado
  (`diasSinVisitaUmbral`) y las etiquetas del front lo muestran. Se editan en Configuración →
  pestaña **Alertas** (el grupo se agrega en `configuracion-sitio.component.ts`, no en el core).

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

### Etapa F — Modo celular (PWA) para la escena del vendedor (arrancada 2026-09-12)
La historia dice "el vendedor arma el pedido desde el celular" y hoy cuatro pantallas no tienen
responsive. Objetivo: que el sitio se instale como app en el teléfono, que el público lo abra
desde un QR y que las pantallas del vendedor se usen con un dedo. Etapas cortas, cada una deja
el sitio funcionando; **al cerrar cada una se anota el avance acá** para poder seguir en otra
sesión. Sin tocar `core/` ni el generador.

- [ ] **F1 — Instalable (PWA)**: `@angular/service-worker` + `ngsw-config.json` + `manifest.webmanifest`
  + íconos en `web/public/` + `theme-color` y metas de iOS en `index.html`. El service worker
  solo se registra en build de producción (en `ng serve` no existe: así se evita el caché viejo
  en desarrollo). Verificación: `ng build` (producción) genera `ngsw.json` y el manifest en
  `dist/`, y Chrome ofrece "Instalar app".
  Avance (2026-09-12): **hecha**. `@angular/service-worker@18` en `package.json`,
  `provideServiceWorker` en `app.config.ts` con `enabled: !isDevMode()`, `web/ngsw-config.json`
  (app prefetch, assets lazy, `/api/**` sin caché: estrategia freshness con `maxAge 0`),
  `angular.json` → `configurations.production.serviceWorker` y `allowedCommonJsDependencies`
  (`qrcode`), `public/manifest.webmanifest` ("Motos · Distribuidora", standalone, portrait,
  `#111827`) e íconos generados por script sin dependencias (`public/icons/`: 192, 512,
  maskable 512 y apple-touch 180; una "M" en trazo claro sobre fondo oscuro), `index.html` con
  `lang="es"`, `manifest`, `theme-color` y metas de iOS. Verificado: `ng build` de producción
  emite `ngsw.json` (160 archivos en `app`, 7 en `assets`, grupo `api`), `ngsw-worker.js`, el
  manifest y los íconos; el build de desarrollo solo copia el manifest (sin service worker).
  Queda el warning preexistente de presupuesto del bundle inicial (725 kB vs 512 kB).
  **No probado todavía en un teléfono**: eso es la F2/F5 (hace falta abrirlo por IP).
- [ ] **F2 — Abrir en el celular**: la API se resuelve por el host con el que se abrió el front
  (por IP → `http://<ip>:5100/api`; por localhost sigue igual), CORS de desarrollo acepta la
  red local, `npm run start:lan` sirve el front en `0.0.0.0`, y un botón en el cabezal muestra
  un **QR** con la URL del sitio en la IP de la máquina (la IP la devuelve un endpoint
  artesanal `GET api/artesanal/lan`). Verificación: abrir el sitio desde el teléfono en la
  misma WiFi, login `pablo/pablo`, rol Vendedor.
  Avance (2026-09-12): **hecha en código y verificada por red desde la misma máquina; falta el
  teléfono real (F5)**. `environment.ts` deriva `apiUrl` del host (`localhost` → como siempre;
  IP → `http://<ip>:5100/api`). `Program.cs`: el CORS acepta la lista de appsettings y, solo en
  Development, cualquier origen de red local (`RedLocal.EsOrigenDeRedLocal`: localhost o IP
  privada 10/172.16-31/192.168); verificado que un origen externo no recibe
  `Access-Control-Allow-Origin`. Perfil `lan` en `launchSettings.json` (`http://0.0.0.0:5100`,
  `dotnet run --project src/ApiMotos --launch-profile lan`) y `npm run start:lan`
  (`--host 0.0.0.0`). `GET api/artesanal/lan` (`LanArtesanalController`, solo Development)
  devuelve las IPv4 privadas de la máquina, el puerto de la API y el nombre del host.
  Botón **QR** en el cabezal (`abrirEnCelular`, oculto en producción) → diálogo
  `modules/artesanal/celular/abrir-en-celular.component.ts` con el QR de
  `http://<ip>:<puerto del front><ruta actual>`, selector si hay varias IPs, copiar link y
  avisos (sin IP, abierto por HTTPS, API sin perfil lan). Verificado con la API en
  `0.0.0.0:5101` y el front en `0.0.0.0:4211`: `lan` devolvió `192.168.68.59`, la API y el
  front respondieron por esa IP, el manifest y los íconos también.
  **Pendiente de F5**: probar desde el teléfono; si no llega, es el firewall de Windows
  (permitir entrada a `dotnet.exe` y `node.exe` en redes privadas, o los puertos 5100 y 4210).
**Replanteo (2026-09-12, después de F2)**: la demo va por **Teams** (pantalla compartida) y en
producción el sitio se publica como `trenes` (nginx + API por HTTP en un puerto, sin HTTPS).
Sin HTTPS no hay PWA instalable, así que F1 queda como preparación (solo se activa si algún
día hay HTTPS) y F2 como herramienta de desarrollo para un teléfono físico; ninguna de las dos
abre puertos en producción. Se eligió la **opción B**: una **app del vendedor dentro del mismo
sitio**, en `/m`, con shell propio (barra inferior, sin sidebar ni Administración), reutilizando
las escenas artesanales; el "celular" se muestra por Teams con un marco de teléfono en el
navegador. Seguridad (perfil Vendedor, `[ZasAuthorize]`, filtro "lo mío") queda **aparte** como
Etapa G, todavía sin fecha: hoy ninguna query del dominio exige capability y el front no
filtra por permisos; el selector de rol es solo presentación.

- [ ] **F3 — Responsive de las pantallas que venden**: catálogo (grilla a una columna, filtros
  plegados) y armado de pedido (carrito como tarjetas, barra de total pegada abajo con "Crear
  pedido"). Verificación con el modo dispositivo de Chrome a 400 px sin scroll horizontal.
  Avance (2026-09-12): **hecha en código, compila; falta mirarla en el modo dispositivo de
  Chrome** (sin herramienta de navegador en la sesión). Catálogo (`catalogo.component.scss`,
  `@media ≤640px`): cabecera apilada, tres KPI compactos en fila, buscador a lo ancho y
  filtros a mitad y mitad, grilla de dos cards por fila (una sola ≤360 px), precio y stock
  apilados en la card. Armado de pedido: el carrito dejó de ser `<table>` y es una grilla de
  seis columnas en escritorio que en el celular se vuelve tarjeta por línea (SKU y quitar
  arriba, producto, cantidad y precio en dos campos grandes con `inputmode`, subtotal abajo);
  cabecera del pedido en una columna; KPI a dos por fila; y la barra de acciones queda **fija
  abajo** con "Crear pedido · total" (`position: fixed`, `safe-area-inset-bottom`, padding en
  la página para no tapar el final). Sin cambios en el `.ts`.
- [ ] **F4 — App del vendedor `/m`**: layout móvil propio en `modules/artesanal/movil/` (cabezal
  chico con el nombre del vendedor, barra inferior: Mi día · Clientes · Catálogo · Nuevo pedido
  · Pedidos), rutas hijas que reutilizan agenda, cliente 360, catálogo, ficha comercial,
  armado de pedido, ficha del pedido y seguimiento. Sin sidebar, sin Administración ni Sistema.
  El manifest apunta `start_url` a `/m`. "Ver como vendedor" del selector de rol abre `/m`.
  Avance (2026-09-12): **hecha en código, compila; falta recorrerla en el modo dispositivo**.
  `modules/artesanal/movil/`: `movil-layout.component.ts` (cabezal con nombre del sitio y del
  vendedor de `GET api/Vendedor/mio`, o el usuario si no es vendedor; menú con tema, "Mi panel",
  "Sitio completo" y salir; barra inferior fija: Mi día · Clientes · Catálogo · Vender ·
  Pedidos; en escritorio se centra a 480 px para ensayar), `movil.routes.ts` (hijas que
  reutilizan agenda, cliente 360, catálogo, ficha comercial, armado de pedido, ficha generada
  del pedido, seguimiento, SKU, panel del vendedor y ficha de actividad; `**` → agenda),
  `movil-clientes.component.ts` (tarjetas con semáforo del panel del vendedor, o la lista
  completa si el usuario no es vendedor; búsqueda por nombre o ciudad) y
  `movil-pedidos.component.ts` (tarjetas por estado, chips de filtro, "Abiertos" por defecto).
  **Truco para no tocar las escenas**: `ModoMovilService` + `modoMovilGuard` en la ruta del
  layout de escritorio: al entrar a `/m` se activa el modo (localStorage `motos.modo-movil`) y
  cualquier navegación absoluta de las escenas (`/cliente/2`, `/pedidos/nuevo?clienteId=2`)
  se redirige a la misma URL bajo `/m`; "Sitio completo" lo apaga. `app.routes.ts`: ruta
  top-level `m` (authGuard + `loadComponent` + `loadChildren`) antes del layout de escritorio.
  `cambiarRol('vendedor')` navega a `/m`. `manifest.webmanifest` → `start_url: /m`.
- [ ] **F5 — Responsive del resto del recorrido móvil**: agenda (mapa plegable), cliente 360,
  panel del vendedor, seguimiento y ficha de SKU con KPI a dos por fila; existencias y
  comisiones con scroll propio en la tabla.
  Avance (2026-09-12): **hecha en código, compila; falta recorrerla en el modo dispositivo**.
  Agenda: botón "Mapa" (solo ≤640 px) que pliega/despliega el lateral con el mapa, la ruta
  del día y las alertas (`mostrarLateral`), paradas a lo ancho. Panel del vendedor: KPI a dos
  por fila, gráfico más bajo, acciones que envuelven. Cliente 360: acciones y KPI a dos por
  fila. Existencias y comisiones: filtros a lo ancho y KPI en tres columnas (la tabla ya tenía
  `.table-wrapper` con scroll propio). Seguimiento y SKU ya tenían sus reglas.
  **Paracaídas global** al final de `styles.scss` (`@media ≤640px`, solo clases compartidas de
  los shells, sin tocar core ni partials): `.kosmos-page` con menos padding, cabecera apilada
  y KPI a dos columnas, toolbar que envuelve con buscador a lo ancho, `content-card` sin
  desborde, `master-detail`/`with-relations` a una columna, diálogos de los forms generados a
  lo ancho del teléfono con contenido de 70 dvh, fichas con acciones que envuelven, grilla de
  datos a una columna y menos padding. En `/m`, los FAB de las escenas (`.floating-actions`)
  suben por encima de la barra inferior.
- [ ] **F6 — Marco de celular para Teams y guion**: ruta `/celular` que muestra `/m` en un
  iframe con bisel de teléfono, y una **vista doble** (teléfono del vendedor a la izquierda,
  "Hoy" de la gerencia a la derecha) para mostrar cómo el pedido creado en el celular aparece
  en el tablero. Actualizar `doc/demo.md` con la escena 3 y 4 contadas así.
  Avance (2026-09-12): **hecha en código, compila; falta recorrerla en el navegador**.
  `modules/artesanal/celular/marco-celular.component.ts` en la ruta top-level `/celular`
  (authGuard, sin layout): teléfono dibujado en CSS (390 × 844, muesca y barra de inicio,
  escalado a la ventana) con un iframe de `/m?modo=marco`; **vista doble** (por defecto) con un
  segundo iframe de `/?modo=escritorio` y el rótulo "Gerencia · Hoy"; atajos a Mi día,
  Clientes, Catálogo, Vender y Pedidos; recargar; Esc para salir. Botón de celular en el
  cabezal (solo fuera de producción). `ModoMovilService` lee `?modo=marco|escritorio` al
  arrancar y en ese caso **no persiste** el modo en localStorage: los dos iframes comparten el
  storage y sin esto el tablero se iría a `/m`. `doc/demo.md` cuenta las escenas 3 y 4 desde
  el marco. Pendiente de mirar: que el tablero refleje el pedido creado (se refresca cada
  minuto) y el escalado en la resolución de la notebook que se comparte por Teams.

### Etapa H — Revisión de contenido de las escenas (arrancada 2026-09-12)
Tras el recorrido de Pablo y una revisión de producto escena por escena (qué se ve, no cómo se
ve), se ordenaron 12 ítems: transversales (A etiquetas y moneda, B links y un bug), y por
escena (1 Pedido como escena propia, 2 Armado de pedido con contexto del cliente, 3 Catálogo
con semáforo de stock, 4 Agenda con contexto por parada, 5 Seguimiento con fecha estimada y
fotos, 6 Comisiones con meta y mes anterior, 7 Hoy y TV, 8 Ficha de SKU con hermanos, 9 Panel
del vendedor, 10 Móvil con acciones de un tap). Se anota el avance por ítem.
- [x] **A — Etiquetas y moneda unificadas**: `modules/artesanal/comun/etiquetas.ts`
  (diccionario único de estados de pedido y envío, etapas SLA, tipos y resultados de
  actividad, tipos de movimiento, semáforos, tipos de cliente y producto, tipos de acción del
  feed; función `etiqueta()` y pipe `| etiqueta`) y `comun/usd.pipe.ts` (`usd()` y pipe
  `| usd` / `| usd:0`, "US$ 1.234,50" en es-UY). Reemplazados los 33 `US$ {{ … | number }}`
  y los enums crudos en Hoy, TV, Catálogo, Ficha comercial, Cliente 360, Panel, Seguimiento,
  SKU, Agenda y móvil; los helpers locales (`etiquetaResultado`, `etiquetaEstado`, `usd()`,
  `formatoUsd()`) delegan en los compartidos. "Pedido" → "Con pedido" en todas; la parada
  planificada de la agenda dice "Planificada" en vez de "Volver". La ficha generada del pedido
  queda para el ítem 1.
- [x] **B — Links y bug**: la agenda ahora lee `?vendedorId=N` (abre ESA agenda; antes lo
  ignoraba y "Ver agenda" desde el panel de otro vendedor abría la propia); "Mis clientes" de
  la agenda va al panel del vendedor (`/vendedor/:id`) en vez del CRUD; la agencia del
  seguimiento dejó de linkear a su ficha CRUD. El link a `/pedidolinea/:id` de la ficha
  generada del pedido se resuelve con el ítem 1.
- [x] **1 — Pedido como escena** (`/pedido/:id`, `modules/artesanal/pedido/`,
  `GET api/artesanal/pedido/{id}/ficha` en `PedidoArtesanalController` + `Comercial/FichaPedido`):
  cabecera con número, estado, unidades y líneas; tarjetas de cliente (→ cliente 360, con
  tipo, ciudad y dirección), vendedor (→ panel, con % y comisión sellada o estimada), envío
  (→ seguimiento, con código y estado; o "nace al despachar") y total (con cuántas líneas van
  bajo lista); recorrido del ciclo con el **próximo paso guiado** (actor y condición, misma
  matriz que `ciclos-vida.json`), aviso de faltante antes de confirmar, elección de agencia
  inline al despachar si falta, anulación con motivo; líneas con foto, producto → ficha de
  SKU, marca y SKU, talle y color con muestra, cantidad, precio con "−N %" si está bajo lista,
  subtotal, stock en el depósito (solo mientras el pedido no salió) y pie de totales;
  "Agregar línea" (form generado en diálogo) y quitar, solo en borrador/confirmado; WhatsApp
  con detalle, total y link público del envío; imprimir con membrete; "Nuevo pedido para
  este cliente". La ruta móvil `/m/pedido/:id` también la usa. **Kanban generado** (shell
  tocado, `pedido-list.component.{html,ts}`): la tarjeta muestra cliente, total y días en el
  estado y abre la escena (antes el form); el cabezal de cada columna suma el total US$.
  Verificado por API con el seed: PED-0007 preparado con 2 líneas, 2 faltantes en Showroom,
  comisión 3,5 %; PED-0005 despachado con envío MT-2026-0101 y 2 líneas con foto; 404 en
  inexistente. Front compilado.
- [x] **2 — Armado de pedido con contexto** (`armado-pedido.component.*`, sin backend nuevo:
  reusa `cliente/{id}/360` y `pedido/{id}/ficha`): el cliente se elige con **autocompletado**
  (nombre, ciudad o tipo; tilde verde al elegirlo); al elegirlo aparece la tarjeta **"Este
  cliente"** con semáforo y motivo, días sin visita, pedidos y US$ del año, ticket promedio,
  pedidos abiertos, **último pedido con "Repetir pedido"** (mismas líneas y cantidades, a
  precio de lista de hoy, avisa cuántas entraron) y **"Lo que más compra"** como chips que
  cargan el buscador; `?pedidoId=N` precarga las líneas (Cliente 360 tiene el botón "Repetir
  este pedido"); en cada línea, al tocar el precio, se ve el **descuento sobre lista** y el
  **margen sobre costo** (rojo si es menor a 15 %), y el pie muestra el margen total del
  carrito. "Cancelar" vuelve al cliente 360 si hay cliente. Compila; sin probar en pantalla.
- [x] **3 — Catálogo con semáforo de stock**: `CatalogoHandler` calcula por producto
  `SkusConStock`, `SkusEnAlerta` (saldo bajo `stock.umbral_bajo`) y `Semaforo`
  (`sin_stock` / `bajo` / `ok`); la card muestra un punto de color y "N de M SKU · U u", el KPI
  "Con foto" (higiene de datos) pasó a **"En alerta"** y es un botón que filtra, y cada card
  tiene **"Vender"** → `/pedidos/nuevo?buscar=<producto>` (el armado deja el texto en el
  buscador). Ficha comercial: `FichaProductoHandler` devuelve `UmbralStockBajo`, `SkusEnAlerta`
  y por SKU `Comprometido`, `Vendidas30d` y `CoberturaDias`; las celdas se pintan con el umbral
  real (antes fijo en 3), muestran "N comprometidas · N días" y hay chip "N bajo el umbral".
  Sin probar en pantalla; API compilada.
- [x] **4 — Agenda con contexto por parada**: `AgendaDto`/`AgendaHandler` suman por parada
  teléfono, días sin visita (última actividad), último pedido (fecha y US$) y pedidos abiertos
  (tres `OUTER APPLY`), y dentro de cada ciudad ordenan las paradas por días sin visita
  descendente (antes alfabético). Front: debajo de cada parada una línea "hace N d · último
  d/MM · US$ · N abiertos" (rojo y borde izquierdo si supera el umbral), botones **WhatsApp y
  Llamar** cuando hay teléfono, alerta de "sin visitar" ordenada de más a menos días con la
  fecha del último contacto y botón **Agendar** (abre el form de actividad con el cliente
  fijado). Verificado por API: 8 paradas con teléfono, días, último pedido y abiertos.
- [x] **5 — Seguimiento con fecha estimada, fotos y aviso al cliente**: `estimarEntrega()` en
  `seguimiento.model.ts` (desde el último hito cumplido suma los días límite de la etapa en
  curso y de las que faltan; null si el ciclo cerró o falta un límite) se muestra en "Etapa en
  curso" (interna) y bajo el titular (pública, "Llegaría alrededor del jueves 18 de
  septiembre"). Las líneas llevan `imagenPrincipalId` (`SeguimientoLineaDto` + SQL) y se ven
  como miniaturas en las dos páginas; para la pública, sin token, hay
  `GET api/publico/imagen/{id}` (`SeguimientoPublicoController`, anónimo) que **solo sirve
  documentos que sean portada de un producto activo**, cualquier otro id da 404. Interna:
  botón **"Avisar al cliente"** (WhatsApp con estado, agencia, fecha estimada y link público).
  Pública: tarjeta "¿Dudas con tu envío?" con WhatsApp de la empresa (`social.whatsapp` o
  `empresa.telefono` de Configuración; si no hay, no aparece). Verificado: imagen 1 → 200
  image, 999 → 404, seguimiento público con `imagenPrincipalId` por línea y sin precios.
- [x] **6 — Comisiones con meta, proyección y mes anterior** (`/comisiones`; `ComisionesHandler`
  y `ComisionVendedorDto` crecen, el endpoint es el mismo): cada fila trae la **meta** del
  período (`PC_METAS`), lo **pendiente de entregar** (pedidos del mes en estado distinto de
  entregado/anulado, mismo criterio que el panel del vendedor) y el **mes anterior** (entregados,
  vendido, comisión sellada y meta). La proyección se calcula en el handler: sellada + pendiente ×
  % de hoy. Pantalla: cuatro KPI (entregados con el mes anterior, vendido con variación ▲▼ contra
  el mes anterior, **meta del equipo** con barra, tick de "esperado a hoy" y semáforo, a liquidar
  con la **proyectada** "si se entrega todo lo abierto"); en la tabla, columna **Meta** (barra
  mini con semáforo, % y objetivo; el semáforo compara contra lo esperado a la fecha en el mes en
  curso y contra la meta entera en un mes cerrado), columna **Proyectada** (solo en el mes en
  curso, con cuántos pedidos abiertos) y columna del **mes anterior** (comisión de entonces y
  variación; "nuevo" si antes no había nada). La fila lleva al panel del vendedor; el CSV exporta
  todas las columnas nuevas. **Seed**: metas del mes anterior (16.000 / 12.000 / 22.000) y
  sección **14 bis** con 5 pedidos entregados el mes pasado repartidos entre los tres vendedores
  (numerados a continuación del último Id, sin envío), guardada por "no hay entregados anteriores
  al mes en curso" así entra también en una base ya sembrada; cada salida tiene su entrada previa
  por la misma cantidad (contenedor MRKU-3318) para que las existencias de hoy no cambien.
  **Ojo** al escribir seeds: `DbBootstrap` ejecuta con `ExecuteSqlRaw`, que interpreta `{ }`
  como formato; un comentario con llaves tira `FormatException` y corta el resto del archivo.
  Verificado por API: mes actual con 3 filas (Andrés 2 entregados, 3 abiertos, proyectada
  US$ 174,12; Lucía sin entregas pero 1 abierto y US$ 61,20 el mes pasado); mes anterior con los
  5 entregados y proyectada = sellada; período inválido → 400; existencias con el mismo único
  negativo; ficha de PED-0013 (entregado, sin envío) responde. Front compilado. Las metas del
  seed (US$ 18.000 / 12.000 / 25.000) siguen muy por encima de lo vendido, así que el semáforo
  arranca en rojo para todos: si para la demo se quiere ver verde, bajar las metas en el seed.
- [x] **7 — Hoy y TV** (revisión con el payload real de `centro-control` sobre el seed):
  (a) el feed decía "AGV Casco Integral AGV K1 S…" y "Alpinestars Botas Alpinestars…": el
  nombre del producto ya trae la marca, `NombreSku` no la antepone si está contenida;
  (b) la **visita del día** no entraba en el feed (prioridad 7, detrás del stock bajo, y el
  corte en 12 la dejaba afuera): ahora va con prioridad 3, a la par de "listo para despachar",
  y el cupo sube a 14 (el TV sigue mostrando 8), así entran también los clientes sin visitar;
  (c) el KPI "Ventas del mes" (colocado, US$ 7.743) no cerraba con el "vendió" del equipo
  (entregado, US$ 3.193): el DTO trae `VentasEntregadasMesUsd`, el KPI lo aclara en tooltip
  junto con el mes anterior, y la tarjeta Equipo abre con "Entregado en el mes US$ X ·
  comisiones US$ Y" (el `comisionesMesUsd` que venía sin usar); (d) barras de meta del equipo
  con el **tick de "esperado a hoy"** (día/días del mes) y leyenda, en Hoy y en la TV, mismo
  criterio que el panel y comisiones; (e) TV: el KPI de ventas tomaba siempre acento verde y
  no decía la variación; ahora lleva el acento por variación y la línea "+42 % vs mes anterior",
  y la slide del equipo suma entregado, comisiones y la leyenda del tick. Verificado por API:
  14 acciones (4 SLA, negativo, 2 preparados, la visita de hoy, 3 stock bajo, confirmado, 2 sin
  visitar), entregado 3.193 / comisiones 127,37 / variación +42,1 % (gracias al 14 bis del ítem
  6 el mes anterior ya no es cero). Front compilado. Sin cambios de seed.
- [x] **8 — Ficha de SKU con hermanos** (`/variante/:id`; `FichaSkuDto.Hermanos` +
  `SqlHermanos` en `FichaSkuHandler`, mismo endpoint): la ficha trae TODAS las variantes del
  producto (la actual marcada) con talla, orden de talla, color y hex, precio, saldo total del
  Kardex (misma regla: transferencia con destino suma 0), comprometido en pedidos abiertos y
  semáforo (la regla `bajo = entre 0 y 3` quedó en un helper compartido con el SKU). Pantalla:
  sección **"Otras tallas y colores"** debajo de los KPI, como **matriz color × talla** (filas
  colores con muestra, columnas tallas en orden; cada celda es el saldo con borde semáforo, el
  comprometido en chico y "este" en la actual; celda punteada si ese SKU no existe; clic o
  Enter abre la ficha del hermano, la ruta ya recarga por `paramMap`). Si el SKU actual está
  bajo, sin stock o negativo, una línea de **alternativas** ofrece hasta 4 hermanos con stock
  neto, primero los de la misma talla y luego los del mismo color; si ninguno tiene, lo dice.
  Producto con un solo SKU: mensaje y nada más. Verificado por API: botas SMX-6 42 (2 u, bajo)
  → hermanos 41/43/44 con 3 u; AGV K1 S 57-58 (−1) → los otros 3 talles en 0, sin alternativa;
  Bell MX-9 55-56 amarillo → matriz 4 tallas × 3 colores, con comprometidos en 57-58 azul (2)
  y 59-60 azul (1). Front compilado. Sin cambios de seed.
- [x] **9 — Panel del vendedor** (`/vendedor/:id`, solo front; revisado con el payload real de
  `panel`, `avance` y `agenda` de Andrés): la escena ya contaba ranking, meta con "esperado a
  hoy", comisión sellada/proyectada/"podría cobrar" (coincide con `/comisiones`: 174,12),
  simulador, ventas por semana (ahora con las semanas de agosto pobladas por el 14 bis),
  esta semana, cartera con salud y pedidos abiertos. Faltaba el **mes anterior**: la tarjeta de
  meta cierra con una línea "Agosto 2026: vendió US$ 1.340 de US$ 16.000 (8 %) · comisión
  US$ 60,31" y la variación de lo vendido contra ese mes ("+16 % este mes"; "primer mes con
  entregas" si antes no hubo). Sale de una segunda llamada a `GET vendedor/{id}/avance?periodo=`
  en el mismo `forkJoin` (el endpoint ya aceptaba período; nada nuevo en la API). Y la parada
  planificada de "Esta semana" decía "Volver": ahora "Planificada", como la agenda (ítem A).
  Front compilado. Sin cambios de seed.
- [x] **10 — Móvil con acciones de un tap** (solo front): helper `modules/artesanal/comun/contacto.ts`
  (`telUrl`, `whatsappUrl`, `mapaUrl` por coordenadas o dirección+ciudad, `abrirContacto`), para
  que agenda, Mis clientes y Mis pedidos armen los mismos links. **Agenda / Mi día**: los
  botones WhatsApp y Llamar estaban **duplicados** en la parada (dos `@if (p.telefono)`
  iguales) y las acciones tenían `opacity: 0` hasta el hover, o sea **invisibles en el
  celular**: ahora se ven siempre sin hover o ≤640 px, sin duplicados, y se suman "Cómo llegar"
  (si la parada tiene coordenadas) y "Nuevo pedido para este cliente". **Mis clientes** (`/m/clientes`):
  cruza el panel del vendedor (semáforo) con la lista de clientes (teléfono, dirección,
  coordenadas) y cada tarjeta lleva una fila de acciones altas para el dedo: Llamar, WhatsApp,
  Llegar y Vender (la tarjeta pasó de `<button>` a `div role=button`, porque un botón no puede
  contener botones). **Mis pedidos** (`/m/pedidos`): por tarjeta, WhatsApp al cliente con el
  número y el estado del pedido (teléfono de la lista de clientes, el pedido generado no lo
  trae), Envío (seguimiento, si ya salió) y Repetir (`/m/pedidos/nuevo?clienteId&pedidoId`, que
  el armado ya entendía). Cliente 360 y la ficha del pedido ya tenían sus acciones. Front
  compilado; los links de un tap se verificaron por lectura (sin navegador ni teléfono).

**Etapa H cerrada (2026-09-12)**: los 12 ítems hechos. Pendiente transversal que quedó anotado en
el ítem 6: las metas del seed (US$ 18.000 / 12.000 / 25.000) siguen muy por encima de lo vendido
y el semáforo arranca en rojo para todos; si para la demo se quiere ver verde, bajar las metas.

### Etapa G — Permisos del negocio (pendiente, sin fecha)
La plataforma de seguridad existe (usuarios, perfiles, roles, capabilities, `[ZasAuthorize]`,
`capabilityGuard`) pero no está aplicada al dominio. Cuando se decida: definir ~10 capabilities
del negocio, 3 perfiles (Gerencia, Vendedor, Depósito), `[ZasAuthorize]` en queries y commands,
filtro "lo mío" en las artesanales del vendedor (`GET api/Vendedor/mio` ya enlaza login y
vendedor), guard y menú por capability en el front, y un usuario real por perfil en el seed
(el bypass `pablo/pablo` tiene la lista de capabilities vacía y en Development el
`MockCurrentUserService` da todas).

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
