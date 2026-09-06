# Vistas artesanales (contrato 2026-08-29)

Vistas construidas por **forja-vistas** para las US que el catálogo de vistas no
expresa. Es el gemelo frontend de `Application/Agregates/{Plural}/{Entidad}Hooks.cs`:
el hogar del código manual que la regeneración NUNCA pisa.

## Reglas

1. **La regen de Forja no toca esta carpeta.** Nada de acá es shell regenerable.
2. **Datos**: SOLO por facades existentes o queries artesanales read-only del backend.
3. **Escrituras**: SIEMPRE por los commands generados — acá no se muta nada por afuera.
4. **Estilos**: SOLO tokens `var(--ceskia-*)`. Cero colores hardcodeados.
5. **Cada vista tiene su story-file** que la declara y la traza a su US.

## Estructura

`{entidad}/{key}/` por vista (ej: `edificio/morosidad/`). Cada una se registra en
`artesanal.registry.ts`; si falta la entrada o el componente, la entidad cae sola a tabla.
