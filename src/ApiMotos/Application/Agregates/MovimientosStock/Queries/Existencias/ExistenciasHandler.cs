using MediatR;
using ApiMotos.Application.Common.Abstractions;

namespace ApiMotos.Application.Agregates.MovimientosStock.Queries.Existencias
{
    /// <summary>
    /// Existencias = SUM firmado del Kardex por (VarianteId, DepositoId). La transferencia resta
    /// en el origen y suma en el destino (segunda mitad del UNION ALL). Ver doc/modelo-stock.md §5.
    /// </summary>
    public class ExistenciasHandler : IRequestHandler<ExistenciasQuery, List<ExistenciasDto>>
    {
        private readonly IQueryService _consultas;

        public ExistenciasHandler(IQueryService consultas)
        {
            _consultas = consultas;
        }

        public Task<List<ExistenciasDto>> Handle(ExistenciasQuery query, CancellationToken cancellationToken)
        {
            var sql = @"
WITH mov AS (
    SELECT VarianteId, DepositoId,
        CASE Tipo WHEN 'entrada' THEN Cantidad
                  WHEN 'salida' THEN -Cantidad
                  WHEN 'ajuste' THEN Cantidad
                  WHEN 'transferencia' THEN -Cantidad END AS Delta
    FROM PC_MOVIMIENTOS_STOCK
    UNION ALL
    SELECT VarianteId, DepositoDestinoId AS DepositoId, Cantidad AS Delta
    FROM PC_MOVIMIENTOS_STOCK
    WHERE Tipo = 'transferencia' AND DepositoDestinoId IS NOT NULL
)
SELECT mov.VarianteId
     , v.Sku AS Sku
     , p.Nombre AS ProductoDisplay
     , mov.DepositoId
     , d.Nombre AS DepositoDisplay
     , SUM(mov.Delta) AS Disponible
FROM mov
LEFT JOIN PC_VARIANTES v ON mov.VarianteId = v.Id
LEFT JOIN PC_PRODUCTOS p ON v.ProductoId = p.Id
LEFT JOIN PC_DEPOSITOS d ON mov.DepositoId = d.Id
WHERE (@VarianteId IS NULL OR mov.VarianteId = @VarianteId)
  AND (@DepositoId IS NULL OR mov.DepositoId = @DepositoId)
GROUP BY mov.VarianteId, v.Sku, p.Nombre, mov.DepositoId, d.Nombre
HAVING SUM(mov.Delta) <> 0
ORDER BY p.Nombre, v.Sku, d.Nombre";

            return _consultas.ConsultarAsync<ExistenciasDto>(sql,
                new { VarianteId = query.VarianteId, DepositoId = query.DepositoId });
        }
    }
}
