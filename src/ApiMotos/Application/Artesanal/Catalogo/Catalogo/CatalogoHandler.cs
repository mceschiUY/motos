using MediatR;
using ApiMotos.Application.Common.Abstractions;

namespace ApiMotos.Application.Artesanal.Catalogo.Catalogo
{
    /// <summary>
    /// READ-ONLY (zona artesanal, plan §4.5). Grilla del catálogo: un producto por card, con el
    /// precio "desde" (MIN de las variantes activas) y las unidades en stock de todos los depósitos.
    ///
    /// El filtro por categoría incluye a las hijas: elegir "Casco" trae Integral, Modular, Jet y
    /// Cross, que es lo que espera quien navega el catálogo.
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
)
SELECT p.Id, p.Codigo, p.Nombre, p.MarcaId, m.Nombre AS MarcaDisplay,
       p.CategoriaId, c.Nombre AS CategoriaDisplay, p.Genero,
       CAST(ISNULL(p.Destacado, 0) AS BIT) AS Destacado,
       CAST(ISNULL(p.Novedad, 0) AS BIT) AS Novedad,
       p.ImagenPrincipalId,
       ISNULL(pr.PrecioDesdeUsd, 0) AS PrecioDesdeUsd,
       ISNULL(pr.Skus, 0) AS Skus,
       ISNULL(st.Unidades, 0) AS Unidades
FROM PC_PRODUCTOS p
LEFT JOIN PC_MARCAS m ON m.Id = p.MarcaId
LEFT JOIN PC_CATEGORIAS c ON c.Id = p.CategoriaId
LEFT JOIN (
    SELECT ProductoId, MIN(PrecioLista) AS PrecioDesdeUsd, COUNT(*) AS Skus
    FROM PC_VARIANTES WHERE Activo = 1 GROUP BY ProductoId
) pr ON pr.ProductoId = p.Id
LEFT JOIN (
    SELECT v.ProductoId, SUM(mov.Delta) AS Unidades
    FROM mov INNER JOIN PC_VARIANTES v ON v.Id = mov.VarianteId
    GROUP BY v.ProductoId
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

        public Task<List<CatalogoItemDto>> Handle(CatalogoQuery query, CancellationToken cancellationToken)
        {
            var q = string.IsNullOrWhiteSpace(query.Q) ? null : query.Q.Trim();
            return _consultas.ConsultarAsync<CatalogoItemDto>(Sql, new
            {
                MarcaId = query.MarcaId,
                CategoriaId = query.CategoriaId,
                Q = q,
            });
        }
    }
}
