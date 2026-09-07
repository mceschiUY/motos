# Modelo de datos — Stock / Inventario (Fase 1, bloque 2)

> Segundo eslabón del MVP logístico. Depende del **Catálogo** (la unidad de stock es la
> **Variante/SKU**). Respeta las convenciones del repo: tablas `PC_<ENTIDAD>`, PK `Id INT IDENTITY`,
> agregados con factory `Crear/Modificar`, FKs vía `FK_PC_*.sql`, CQRS con handlers genéricos.

---

## 1. Alcance del bloque

- **Depósito** (maestra): dónde se guarda la mercadería (multi-depósito).
- **Movimiento de stock (Kardex)**: el libro mayor de entradas/salidas/ajustes/transferencias.
  Cada movimiento es un hecho inmutable; el stock NO se edita directamente, se mueve.
- **Existencias** (read model): saldo disponible por SKU/depósito, **calculado** sumando el
  Kardex (no es una tabla que se mantiene a mano). Se expone como query/endpoint de lectura.

**Fuera de alcance en este bloque** (vienen con ventas/compras, bloques 3-4):
`comprometido` (reservado por pedidos) y `en tránsito` (PO en viaje). Hoy valen 0; se agregan
cuando existan Pedido de venta y Orden de compra.

---

## 2. Entidades y relaciones

```
┌────────────┐         ┌──────────────────────┐        ┌─────────────┐
│  Depósito  │1──────N │  MovimientoStock     │N──────1│  Variante   │ (SKU, del Catálogo)
│ PC_DEPOS.  │ origen  │  (Kardex)            │        │ PC_VARIANTES│
└─────┬──────┘         └──────────────────────┘        └─────────────┘
      │1  N (DepositoDestinoId, solo transferencia)
      └────────────────────┘

Existencias (VISTA calculada)  =  Σ movimientos firmados  agrupados por (VarianteId, DepositoId)
```

---

## 3. Tablas

### 3.1 `PC_DEPOSITOS` — Depósito
| Campo | Tipo | Null | Nota |
|---|---|---|---|
| Id | INT IDENTITY PK | | |
| Codigo | NVARCHAR(30) | NO | Único (ej. "DEP-CENTRAL") |
| Nombre | NVARCHAR(120) | NO | |
| Direccion | NVARCHAR(250) | SÍ | |
| Activo | BIT | NO | default 1 |

### 3.2 `PC_MOVIMIENTOS_STOCK` — Kardex
| Campo | Tipo | Null | Nota |
|---|---|---|---|
| Id | INT IDENTITY PK | | |
| VarianteId | INT FK→PC_VARIANTES | NO | El SKU que se mueve |
| DepositoId | INT FK→PC_DEPOSITOS | NO | Depósito afectado (origen en transferencia) |
| DepositoDestinoId | INT FK→PC_DEPOSITOS | SÍ | Solo `transferencia`: destino |
| Tipo | NVARCHAR(20) | NO | enum: `entrada` \| `salida` \| `ajuste` \| `transferencia` |
| Cantidad | DECIMAL(18,2) | NO | Siempre > 0 (el signo lo da el Tipo) |
| CostoUnitario | DECIMAL(18,4) | SÍ | Para valorización (se completa con landed cost en Fase 2) |
| Motivo | NVARCHAR(250) | SÍ | Texto libre |
| DocumentoOrigen | NVARCHAR(80) | SÍ | Ref. al documento (ej. "Recepción PO-123") |
| Fecha | DATETIME2 | NO | default ahora |
| Usuario | NVARCHAR(120) | SÍ | Quién lo registró |

**Índices:** `IX (VarianteId)`, `IX (DepositoId)` para el cálculo de existencias.

---

## 4. Reglas de dominio (en `MovimientoStock.Crear/Modificar`)

- `Cantidad > 0` (el Tipo define el signo; nunca cantidades negativas).
- `Tipo ∈ {entrada, salida, ajuste, transferencia}`.
- `transferencia` ⇒ `DepositoDestinoId` requerido y **distinto** de `DepositoId`.
- `entrada|salida|ajuste` ⇒ `DepositoDestinoId` debe ser null.
- (Futuro, cuando exista saldo consolidado) bloquear `salida` que deje stock negativo — se deja
  anotado; hoy el saldo se calcula al vuelo, no se valida contra un balance persistido.

---

## 5. Existencias (read model — SUM firmado del Kardex)

Signo por tipo para el depósito de la fila:
`entrada → +Cantidad`, `salida → −Cantidad`, `ajuste → +Cantidad` (cargar negativo ajustando
con un `salida`), `transferencia → −Cantidad` (sale del origen). La transferencia además **suma**
`+Cantidad` en `DepositoDestinoId` (segunda mitad del UNION).

```sql
WITH mov AS (
    SELECT VarianteId, DepositoId,
        CASE Tipo WHEN 'entrada' THEN Cantidad
                  WHEN 'salida' THEN -Cantidad
                  WHEN 'ajuste' THEN Cantidad
                  WHEN 'transferencia' THEN -Cantidad END AS Delta
    FROM PC_MOVIMIENTOS_STOCK
    UNION ALL
    SELECT VarianteId, DepositoDestinoId, Cantidad
    FROM PC_MOVIMIENTOS_STOCK
    WHERE Tipo = 'transferencia' AND DepositoDestinoId IS NOT NULL
)
SELECT VarianteId, DepositoId, SUM(Delta) AS Disponible
FROM mov
GROUP BY VarianteId, DepositoId
HAVING SUM(Delta) <> 0;
```

Endpoints de lectura en `MovimientoStockController`:
- `GET /api/MovimientoStock/existencias` — saldo por SKU/depósito (con displays de SKU/producto/depósito).
- `GET /api/MovimientoStock/by-variante/{id}` — Kardex de un SKU.
- `GET /api/MovimientoStock/by-deposito/{id}` — Kardex de un depósito.

---

## 6. Implementación por entidad (mismas 6 capas que el Catálogo)

1. **Deposito** — maestra CRUD (idéntico a Marca/Color).
2. **MovimientoStock** — CRUD del Kardex + query `Existencias` + filtros por FK.
3. Front: pantallas generadas de Deposito y MovimientoStock (list/form/ficha) + registries.
4. **Existencias en el front**: pendiente como vista de solo-lectura (siguiente incremento);
   el endpoint ya queda listo para consumir.

## 7. Orden de ejecución

- [x] 1. **Deposito** (maestra) — backend (Domain + CQRS + Controller + EF + DI + `PC_DEPOSITOS.sql` con seed `DEP-CENTRAL`) + front (list/form/ficha, registries). Compila API + build Angular OK.
- [x] 2. **MovimientoStock** (Kardex) — backend con reglas de dominio (`Cantidad>0`, `Tipo` válido, consistencia de transferencia) + Controller con `by-variante`/`by-deposito` + EF Map con 3 FKs + `PC_MOVIMIENTOS_STOCK.sql` + `FK_PC_MOVIMIENTOS_STOCK.sql` + front (form con selects de Variante/Depósito y destino condicional a transferencia).
- [x] 3. **Existencias** — `ExistenciasQuery/Handler` (SUM firmado del Kardex, transferencia origen−/destino+) + endpoint `GET /api/MovimientoStock/existencias?varianteId&depositoId`.
- [ ] 4. Vista de existencias en el front (incremento siguiente): pantalla de solo-lectura que consuma `/existencias`. El endpoint ya está listo.
