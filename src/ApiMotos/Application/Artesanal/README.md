# Queries artesanales (contrato 2026-08-29)

Queries **READ-ONLY** creadas por **forja-vistas** cuando una vista artesanal
necesita datos que ningún endpoint generado devuelve. Gemelo backend de
`modules/artesanal/` del sitio y hermano de `Agregates/{Plural}/{Entidad}Hooks.cs`:
zona manual que la regeneración NUNCA pisa.

## Reglas

1. **La regen de Forja no toca esta carpeta.**
2. **Mismo patrón CQRS** que las queries generadas: `Query` + `Handler` (MediatR) + `Dto`,
   namespace `ApiMotos.Application.Artesanal.{Entidad}.{NombreQuery}`.
3. **PROHIBIDO** comandos o escrituras acá. Las escrituras van SIEMPRE por los
   commands generados + Hooks.
4. **Trazabilidad**: cada query queda declarada en el story-file de su vista.

Se exponen únicamente vía GET en `Controllers/Artesanal/` (`/api/artesanal/...`).
