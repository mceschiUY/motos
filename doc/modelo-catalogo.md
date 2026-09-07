# Modelo de datos — Catálogo (Fase 1)

> Piedra angular del sistema. Define **qué se vende** con las particularidades del rubro
> (cascos e indumentaria de moto). Respeta las convenciones del código existente:
> tablas `PC_<ENTIDAD>`, PK `Id INT IDENTITY`, agregados de dominio con factory `Crear/Modificar`,
> FKs vía `FK_PC_*.sql`, enums renderizados como *pills* de color en la tabla genérica.

---

## 1. Entidades y relaciones

```
                 ┌─────────────┐
                 │   Marca     │
                 └──────┬──────┘
                        │ 1
                        │ N
┌────────────┐   ┌──────┴──────┐   N     1 ┌────────────┐
│ Categoría  │1─N│  Producto   │───────────│  (self)    │  Categoría.PadreId
│ (jerárq.)  │   │  (modelo)   │           └────────────┘
└────────────┘   └──────┬──────┘
                        │ 1
                        │ N
                 ┌──────┴───────┐
                 │  Variante    │  = Producto × Talla × Color  (el SKU real, unidad de stock)
                 │  (SKU)       │
                 └──┬────────┬──┘
                 N  │        │  N
                 1  │        │  1
             ┌──────┴──┐  ┌──┴───────┐
             │  Talla  │  │  Color   │
             └─────────┘  └──────────┘
```

**Regla clave del rubro:** el **SKU real es la Variante** (Producto + Talla + Color). El stock,
el precio, el costo y el código de barras viven en la Variante, no en el Producto. Un mismo
modelo de casco/campera son decenas de SKU.

---

## 2. Tablas

### 2.1 `PC_MARCAS` — Marca
| Campo | Tipo | Null | Nota |
|---|---|---|---|
| Id | INT IDENTITY PK | | |
| Nombre | NVARCHAR(120) | NO | Bell, Shoei, Alpinestars, LS2… |
| Pais | NVARCHAR(80) | SÍ | Origen de la marca |
| Activo | BIT | NO | default 1 |

### 2.2 `PC_CATEGORIAS` — Categoría (jerárquica)
| Campo | Tipo | Null | Nota |
|---|---|---|---|
| Id | INT IDENTITY PK | | |
| Nombre | NVARCHAR(120) | NO | Casco, Indumentaria, Accesorios… / subrubros |
| CategoriaPadreId | INT FK→PC_CATEGORIAS.Id | SÍ | Jerarquía (Casco → Integral) |
| Activo | BIT | NO | default 1 |

### 2.3 `PC_TALLAS` — Talla
| Campo | Tipo | Null | Nota |
|---|---|---|---|
| Id | INT IDENTITY PK | | |
| Nombre | NVARCHAR(30) | NO | XS,S,M,L,XL,XXL / 55-56 / 42… |
| Tipo | NVARCHAR(20) | NO | enum: `alfabetica` \| `numerica` |
| Orden | INT | NO | Para ordenar la curva (XS<S<M<…) |
| Activo | BIT | NO | default 1 |

### 2.4 `PC_COLORES` — Color
| Campo | Tipo | Null | Nota |
|---|---|---|---|
| Id | INT IDENTITY PK | | |
| Nombre | NVARCHAR(60) | NO | Negro mate, Rojo, Blanco… |
| CodigoHex | NVARCHAR(7) | SÍ | Para muestra visual (#RRGGBB) |
| Activo | BIT | NO | default 1 |

### 2.5 `PC_PRODUCTOS` — Producto (modelo/estilo)
| Campo | Tipo | Null | Nota |
|---|---|---|---|
| Id | INT IDENTITY PK | | |
| Codigo | NVARCHAR(60) | NO | Código de estilo (SKU padre) — único |
| Nombre | NVARCHAR(200) | NO | Ej. "Casco Integral LS2 FF800 Storm II" |
| MarcaId | INT FK→PC_MARCAS.Id | NO | |
| CategoriaId | INT FK→PC_CATEGORIAS.Id | NO | |
| Descripcion | NVARCHAR(1000) | SÍ | Ficha comercial |
| Genero | NVARCHAR(20) | NO | enum: `unisex`\|`hombre`\|`mujer`\|`nino` |
| Temporada | NVARCHAR(40) | SÍ | Indumentaria estacional (ej. "Verano 2026") |
| Material | NVARCHAR(120) | SÍ | Policarbonato, cordura, cuero… |
| PesoGramos | INT | SÍ | Para flete/landed cost |
| **TipoCasco** | NVARCHAR(20) | SÍ | enum: `integral`\|`modular`\|`jet`\|`cross`\|`na` |
| **Homologacion** | NVARCHAR(20) | SÍ | enum: `ece2206`\|`dot`\|`snell`\|`na` |
| **HomologacionVigente** | BIT | SÍ | Regla: no vender casco con homolog. vencida |
| **FechaVencHomologacion** | DATE | SÍ | |
| Activo | BIT | NO | default 1 |

> Los campos de casco son *nullable* y aplican solo a la categoría Casco (`na` / null para
> indumentaria y accesorios). Se mantienen en Producto para el MVP; si crecen, se extraen a
> una tabla de atributos.

### 2.6 `PC_VARIANTES` — Variante / SKU (unidad de stock)
| Campo | Tipo | Null | Nota |
|---|---|---|---|
| Id | INT IDENTITY PK | | |
| ProductoId | INT FK→PC_PRODUCTOS.Id | NO | |
| TallaId | INT FK→PC_TALLAS.Id | SÍ | Null = talla única |
| ColorId | INT FK→PC_COLORES.Id | SÍ | Null = color único |
| Sku | NVARCHAR(60) | NO | Código único de la variante |
| CodigoBarras | NVARCHAR(40) | SÍ | EAN/UPC |
| CostoEstandar | DECIMAL(18,4) | NO | default 0 — se actualiza con landed cost (Fase 2) |
| PrecioLista | DECIMAL(18,2) | NO | default 0 — lista base (listas por canal en Fase 2) |
| Activo | BIT | NO | default 1 |

**Índice único:** `(ProductoId, TallaId, ColorId)` — evita variantes duplicadas.
**Único:** `Sku`, `CodigoBarras`.

---

## 3. Enums (se renderizan como *pills* de color)

| Enum | Valores | Sugerencia de color (pill) |
|---|---|---|
| Genero | unisex, hombre, mujer, nino | neutro |
| TipoCasco | integral, modular, jet, cross, na | azul (primary) |
| Homologacion | ece2206, dot, snell, na | verde si vigente / rojo si vencida |
| Talla.Tipo | alfabetica, numerica | neutro |

Mapeo de color en `styles.scss` (sección enum-pills), igual que en trenes.

---

## 4. DDL propuesto (patrón `PC_*` idempotente)

> Un archivo por tabla en `src/ApiMotos/Scripts/PC_<ENTIDAD>.sql` (crea tabla + seeds base) y
> las FKs en `FK_PC_<ENTIDAD>.sql` (las aplica DbBootstrap tras crear todas las tablas).
> Orden de creación: Marcas, Categorías, Tallas, Colores → Productos → Variantes.

Ejemplo (Variante):

```sql
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PC_VARIANTES')
BEGIN
    CREATE TABLE [PC_VARIANTES] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [ProductoId] INT NOT NULL,
        [TallaId] INT NULL,
        [ColorId] INT NULL,
        [Sku] NVARCHAR(60) NOT NULL,
        [CodigoBarras] NVARCHAR(40) NULL,
        [CostoEstandar] DECIMAL(18,4) NOT NULL DEFAULT 0,
        [PrecioLista] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [Activo] BIT NOT NULL DEFAULT 1
    );
    CREATE UNIQUE INDEX [UX_PC_VARIANTES_Combo] ON [PC_VARIANTES]([ProductoId],[TallaId],[ColorId]);
    CREATE UNIQUE INDEX [UX_PC_VARIANTES_Sku]   ON [PC_VARIANTES]([Sku]);
END
```

FKs (`FK_PC_VARIANTES.sql`): ProductoId→PC_PRODUCTOS, TallaId→PC_TALLAS, ColorId→PC_COLORES,
cada una con su índice, siguiendo el patrón de `FK_PC_ENVIOS.sql`.

---

## 5. Seeds mínimos (para operar de entrada)

- **Tallas:** XS, S, M, L, XL, XXL (alfabéticas) + numéricas de casco (53-54 … 63-64).
- **Colores:** Negro, Negro mate, Blanco, Rojo, Azul, Gris.
- **Categorías:** Casco (Integral, Modular, Jet, Cross), Indumentaria (Campera, Guantes, Botas, Pantalón, Protección), Accesorios.
- **Marcas:** las que maneje la empresa (dato de negocio).

---

## 6. Alcance de implementación por entidad (todas las capas)

Cada entidad nueva toca, siguiendo el patrón del repo:

1. **Domain** — `Domain/Agregates/<Plural>/<Entidad>.cs` (agregado con `Crear`/`Modificar`).
2. **Infra** — registro en `Infrastructure/Generated/GeneratedEntityMaps.cs` + `GeneratedContext`.
3. **SQL** — `Scripts/PC_<ENTIDAD>.sql` (+ seeds) y `Scripts/FK_PC_<ENTIDAD>.sql`.
4. **Application** — comandos/queries CQRS (Crear/Modificar/Eliminar/Listar/Obtener).
5. **Controllers** — `Controllers/<Entidad>Controller.cs`.
6. **Front** — componentes generados (`modules/generated/components/<entidad>/…`) + alta en `generated-menu.registry.ts`.

> ⚠️ **Decisión de build (importante):** este código es **generado** (Kosmos/Forja). La forma
> "oficial" de agregar entidades es **definirlas en el generador y re-importar**; hacerlo a mano
> acá funciona, pero una regeneración futura podría pisar los shells. Ver §7.

---

## 7. Cómo construirlo — opciones

- **A) Vía generador (Kosmos/Forja, repo `agenteszas`):** se define el universo (producto,
  variante, etc.), se genera y se re-importa. Es el camino "de fábrica" y produce las 6 capas
  consistentes. Requiere trabajar en el repo del generador.
- **B) A mano en este repo:** replicar el patrón entidad por entidad. Más control inmediato,
  pero mayor esfuerzo y riesgo de divergencia si algún día se regenera.

---

## 8. Orden sugerido de ejecución (dentro de Fase 1)

- [x] 1. Tablas maestro: **Marca, Categoría, Talla, Color** — backend completo (Domain + CQRS + Controllers + EF + DI + SQL) y verificado por API.
- [x] 2. **Producto** (FK a Marca y Categoría) + atributos de casco — backend completo (Domain + CQRS + Controller con `by-marca`/`by-categoria` + EF Map con FKs + DI + `PC_PRODUCTOS.sql` + `FK_PC_PRODUCTOS.sql`) y front (modelo, descriptor, servicios, list/form/ficha, registries).
- [x] 3. **Variante/SKU** (FK a Producto, Talla, Color) + índices únicos — backend completo (Domain + CQRS + Controller con `by-producto` + EF Map con FKs e índice único `(ProductoId,TallaId,ColorId)` + `UX Sku` + DI + `PC_VARIANTES.sql` + `FK_PC_VARIANTES.sql`) y front. Regla de unicidad de SKU y de combinación en `VarianteHooks`.
- [x] 4. Seeds base (tallas, colores, categorías) — cargados por `db-init`.
- [~] 5. Enum-pills: los enums (Genero/TipoCasco/Homologacion) quedan declarados en el descriptor con `tipo:'enum'` + `valores`. La tabla genérica (`entity-table`) de ESTE repo renderiza enum como texto (no hay capa de pills consumida); no se agregó CSS muerto.
- [~] 6. Verificar alta/edición desde el **front**: pantallas Producto y Variante generadas y registradas (menú Catálogo, búsqueda global, rutas). Falta verificación en runtime (no hay entorno de build/levantado en esta sesión).
