# Motos — Logística de importación y distribución (sitio generado por CESKIA)

Producto **generado** por el generador CESKIA/Forja (repo `agenteszas`): API .NET 8
(`src/ApiMotos`) + front Angular 18 (`web/`, SiteMotos) + deploy Docker. Este repo es el que
se publica; el generador NO corre acá. Hoy tiene catálogo, stock y tracking de envíos con SLA;
el plan de producto orientado a demo (vendedores, actividad comercial, pedidos, comisiones,
catálogo premium, centro de control; **solo USD**) está en `doc/plan.md`.

> Regla de los .md (heredada de CESKIA): lo escrito tiene que ser REAL y verificable
> contra el código. Ante la duda, el código manda.

## Regla de oro: dónde editar (código generado vs artesanal)

La mayor parte del código salió de plantillas. Si el sitio se regenera y se re-importa, los
SHELLS se pisan. Editar preferentemente en las zonas que la regen jamás toca:

| Zona segura | Qué va ahí |
|---|---|
| `src/ApiMotos/Application/Agregates/{Plural}/{Entidad}Hooks.cs` | Reglas de negocio (ej. `VarianteHooks`: unicidad de SKU; `PedidoHooks`: todo el ciclo del pedido) |
| `src/ApiMotos/Application/Artesanal/` + `Controllers/Artesanal/` | Lógica hecha a mano (queries read-only, procesos) |
| `web/src/app/modules/artesanal/` | Vistas artesanales (ej. Existencias) |
| `web/src/app/shared/vibecoding/` | Componentes visuales genéricos, sin dominio |

Shells regenerables (evitar ediciones a mano): `*Handler.cs` de Commands,
`Application/Common/Generated/*`, `web/src/app/modules/generated/*`.

## Patrón: agregar una entidad (6 capas)

1. **Domain** `Domain/Agregates/<Plural>/<Entidad>.cs` — agregado con factory `Crear`/`Modificar`.
2. **Infra** `Infrastructure/Generated/GeneratedEntityMaps.cs` + `GeneratedContext`.
3. **SQL** `Scripts/PC_<ENTIDAD>.sql` (tabla idempotente + seeds) y `Scripts/FK_PC_<ENTIDAD>.sql`.
4. **Application** CQRS (`Crear/Modificar/Eliminar/Listar/Obtener`) con handlers genéricos.
5. **Controller** `Controllers/<Entidad>Controller.cs` (`api/<Entidad>`, `by-<padre>/{id}`).
6. **Front** `modules/generated/components/<entidad>/{list,form,ficha}` + `models/<entidad>.{model,descriptor}.ts`
   + `services/<entidad>.{api.service,service}.ts` + alta en los 3 registries:
   `generated-components.registry.ts` (rutas), `generated-menu.registry.ts` (sidebar),
   `generated-search.registry.ts` (Ctrl+K y asistente de voz; solo entidades CRUD).

Convenciones: tablas `PC_<ENTIDAD>`, PK `Id INT IDENTITY`, enums en minúsculas renderizados
como `enum-pill`. Estilos solo con tokens `var(--ceskia-*)`; los SCSS de pantallas generadas
son una línea `@use 'generated/entity-list|form|ficha' as *;`.
Modelos de datos: `doc/modelo-catalogo.md`, `doc/modelo-stock.md`.
Las entidades con **ciclo de vida** (Envío, Pedido) no estampan un método por transición: la
matriz vive en `src/ApiMotos/ciclos-vida.json` y los efectos en su `*Hooks.DespuesDeAccion`.

## Correr

**Dev sin Docker:**
```bash
dotnet run --project src/ApiMotos     # https://localhost:7100 (http :5100), swagger en /swagger
cd web && npm start                   # http://localhost:4210, apunta a https://localhost:7100/api
```
Login de desarrollo: `pablo` / `pablo` (bypass solo en Development, `LoginHandler.cs`).
`DbBootstrap` (solo Development) crea la BD con EF y aplica en orden `FK_*.sql`, `Cfg_*.sql` y
`Seed_*.sql` (drift = ALTER ADD, nunca DROP). En Docker lo hace `Scripts/init-db.sh` con los
mismos pases más `Core_Schema.sql` y `PC_*.sql`.

**Datos de demo:** `Scripts/Seed_Dominio_Motos.sql` (mismo contrato que trenes): maestras por fila
con IF NOT EXISTS, catálogo/Kardex y clientes/envíos solo si la tabla cabecera está vacía, anclado
a HOY, nunca borra. Para vaciar el dominio y re-sembrar en el próximo arranque:
```bash
sqlcmd -S "localhost\SQLEXPRESS" -E -d Motos -C -i src/ApiMotos/Scripts/Limpiar_Datos_Dominio.sql
```
(conserva seguridad, configuración y auditoría). Los seeds NO viven en los `PC_*.sql`.

**Local con Docker (todo junto):**
```bash
cp .env.example .env    # editar DB_PASSWORD
./iniciar-motos.sh      # Front: http://localhost:8092 (API :5309)
./detener-motos.sh
```
`docker-compose.yml` es PRODUCCIÓN (imágenes GHCR, lo consume `.github/workflows/deploy.yml`);
local usa `docker-compose.local.yml`.

**Build de verificación:** `dotnet build src/ApiMotos` y `cd web && npx ng build --configuration development`.

## Mapa

| Carpeta | Qué es |
|---|---|
| `src/ApiMotos/` | API .NET (Domain / Application / Infrastructure / Controllers / Scripts) |
| `web/` | Front Angular (SiteMotos) |
| `doc/` | `plan.md` (roadmap con checkboxes `[ ]/[~]/[x]`, marcar al ejecutar), modelos de datos |
| `.github/workflows/deploy.yml` | CI: build de imágenes → GHCR → deploy en runner self-hosted |

## Dominio en 4 líneas

Importadora y distribuidora mayorista de cascos e indumentaria de moto. El SKU real es la
**Variante** (Producto × Talla × Color); el stock es un Kardex de **MovimientoStock** por
**Depósito** y las existencias se calculan sumándolo. Un **Vendedor** visita **Clientes**
(**Actividad**) y arma **Pedidos**: al despachar sale del Kardex y nace el **Envío**, que por
**Agencias** lleva estados con alertas **SLA** por etapa; al entregarse se sella la comisión.
Sitio hermano de referencia visual: `trenes`.
