using MediatR;
using ApiMotos.Application.Artesanal.Comun;
using ApiMotos.Application.Common.Abstractions;

namespace ApiMotos.Application.Artesanal.Catalogo.Catalogo
{
    /// <summary>
    /// READ-ONLY (zona artesanal, plan §4.5). Una fila por producto activo con su precio "desde",
    /// cantidad de SKU y stock. El saldo usa la MISMA regla del Kardex que ExistenciasQuery
    /// (doc/modelo-stock.md §5: entrada +, salida −, ajuste con signo, transferencia − origen
    /// + destino). Revisión de escenas 2026-09-12: además cuántos SKU tienen stock y cuántos
    /// están en alerta (saldo bajo el umbral `stock.umbral_bajo`), para que la card tenga
    /// semáforo y no solo un número de unidades.
    /// </summary>
    public class CatalogoHandler : IRequestHandler<CatalogoQuery, List<CatalogoItemDto>>
    {
        private const string Sql = @"
WITH mov AS (
    SELECT VarianteId,
        CASE Tipo WHEN 'entrada' THEN Cantidad
                  WHEN 'salida' THEN -Cantidad
                  WHEN 'ajuste' THEN Cantidad
                  WHEN 'transferencia' THEN -Cantidad END AS Delta
    FROM PC_MOVIMIENTOS_STOCK
    UNION ALL
    SELECT VarianteId, Cantidad AS Delta
    FROM PC_MOVIMIENTOS_STOCK
    WHERE Tipo = 'transferencia' AND DepositoDestinoId IS NOT NULL
),
sal AS (
    SELECT v.Id AS VarianteId, v.ProductoId, ISNULL(SUM(mov.Delta), 0) AS Saldo
    FROM PC_VARIANTES v
    LEFT JOIN mov ON mov.VarianteId = v.Id
    WHERE v.Activo = 1
    GROUP BY v.Id, v.ProductoId
)
SELECT p.Id, p.Codigo, p.Nombre, p.MarcaId, m.Nombre AS MarcaDisplay,
       p.CategoriaId, c.Nombre AS CategoriaDisplay, p.Genero,
       CAST(ISNULL(p.Destacado, 0) AS BIT) AS Destacado,
       CAST(ISNULL(p.Novedad, 0) AS BIT) AS Novedad,
       p.ImagenPrincipalId,
       ISNULL(pr.PrecioDesdeUsd, 0) AS PrecioDesdeUsd,
       ISNULL(pr.Skus, 0) AS Skus,
       ISNULL(st.Unidades, 0) AS Unidades,
       ISNULL(st.SkusConStock, 0) AS SkusConStock,
       ISNULL(st.SkusEnAlerta, 0) AS SkusEnAlerta
FROM PC_PRODUCTOS p
LEFT JOIN PC_MARCAS m ON m.Id = p.MarcaId
LEFT JOIN PC_CATEGORIAS c ON c.Id = p.CategoriaId
LEFT JOIN (
    SELECT ProductoId, MIN(PrecioLista) AS PrecioDesdeUsd, COUNT(*) AS Skus
    FROM PC_VARIANTES WHERE Activo = 1 GROUP BY ProductoId
) pr ON pr.ProductoId = p.Id
LEFT JOIN (
    SELECT ProductoId,
           SUM(Saldo) AS Unidades,
           SUM(CASE WHEN Saldo > 0 THEN 1 ELSE 0 END) AS SkusConStock,
           SUM(CASE WHEN Saldo < @Umbral THEN 1 ELSE 0 END) AS SkusEnAlerta
    FROM sal
    GROUP BY ProductoId
) st ON st.ProductoId = p.Id
WHERE p.Activo = 1
  AND (@MarcaId IS NULL OR p.MarcaId = @MarcaId)
  AND (@CategoriaId IS NULL OR p.CategoriaId = @CategoriaId OR c.CategoriaPadreId = @CategoriaId)
  AND (@Q IS NULL OR p.Nombre LIKE '%' + @Q + '%' OR p.Codigo LIKE '%' + @Q + '%' OR m.Nombre LIKE '%' + @Q + '%')
ORDER BY CAST(ISNULL(p.Destacado, 0) AS INT) DESC, p.Nombre";

        private readonly IQueryService _consultas;

        public CatalogoHandler(IQueryService consultas)
        {
            _consultas = consultas;
        }

        public async Task<List<CatalogoItemDto>> Handle(CatalogoQuery query, CancellationToken cancellationToken)
        {
            var q = string.IsNullOrWhiteSpace(query.Q) ? null : query.Q.Trim();
            var umbral = await ParametrosAlertas.UmbralStockBajoAsync(_consultas);
            var items = await _consultas.ConsultarAsync<CatalogoItemDto>(Sql, new
            {
                MarcaId = query.MarcaId,
                CategoriaId = query.CategoriaId,
                Q = q,
                Umbral = umbral,
            });
            foreach (var i in items)
            {
                i.UmbralStockBajo = umbral;
                i.Semaforo = i.Skus > 0 && i.SkusConStock == 0 ? "sin_stock"
                           : i.SkusEnAlerta > 0 ? "bajo"
                           : "ok";
            }
            return items;
        }
    }
}
