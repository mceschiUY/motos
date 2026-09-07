using ApiMotos.Application.Common.Abstractions;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.MovimientosStock.Queries.MovimientosStock;

namespace ApiMotos.Application.Agregates.MovimientosStock.Queries.ByDepositoId
{
    public class MovimientosStockByDepositoIdHandler : GenericPorFkHandler<MovimientosStockByDepositoIdQuery, MovimientosStockDto>
    {
        private const string Sql = @"
SELECT e.*
    , variante.Sku AS VarianteDisplay
    , dep.Nombre AS DepositoDisplay
    , depDest.Nombre AS DepositoDestinoDisplay
FROM PC_MOVIMIENTOS_STOCK e
LEFT JOIN PC_VARIANTES variante ON e.VarianteId = variante.Id
LEFT JOIN PC_DEPOSITOS dep ON e.DepositoId = dep.Id
LEFT JOIN PC_DEPOSITOS depDest ON e.DepositoDestinoId = depDest.Id
WHERE e.DepositoId = @DepositoId OR e.DepositoDestinoId = @DepositoId
ORDER BY e.Fecha DESC, e.Id DESC";

        public MovimientosStockByDepositoIdHandler(IQueryService consultas)
            : base(consultas, Sql, q => new { DepositoId = q.DepositoId })
        {
        }
    }
}
