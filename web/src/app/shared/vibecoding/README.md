# Vibecoding — biblioteca de piezas genéricas (contrato 2026-08-29)

Piezas nacidas del vibecoding de vistas artesanales que resultaron genéricas.
forja-vistas **primero busca acá** antes de construir algo nuevo, y **deposita acá**
lo genérico que le salga (read-modify-write del barrel `index.ts`).

## Reglas de admisión

1. **Agnóstica al dominio**: cero vocabulario de negocio en código, inputs o textos.
2. **TODO por `@Input`**: cero servicios inyectados, cero llamadas HTTP.
3. **Estilos**: SOLO tokens `var(--ceskia-*)`.
4. **Estado vacío**: toda pieza renderiza algo digno sin datos.

Si una pieza necesita conocer el dominio, no va acá: vive dentro de su vista en
`modules/artesanal/{entidad}/{key}/`.
