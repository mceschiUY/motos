# Controllers artesanales (contrato 2026-08-29)

Endpoints creados por **forja-vistas** para exponer las queries de
`Application/Artesanal/`. Zona manual: la regeneración NUNCA pisa esta carpeta.

## Reglas

1. **Solo GET.** Cada endpoint despacha una query artesanal read-only vía MediatR.
   Nada de POST/PUT/DELETE: las escrituras van por los controllers + commands generados.
2. **Ruta base** `/api/artesanal/...` (ej: `/api/artesanal/edificios/morosidad`).
3. `[Authorize]` como el resto de los controllers del template.
4. Cada endpoint queda declarado en el story-file de su vista.
