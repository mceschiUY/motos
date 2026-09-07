using ApiMotos.Application.Common.Abstractions;
using ApiMotos.Application.Common.Generated;

namespace ApiMotos.Application.Agregates.MovimientosStock.Queries.MovimientosStock
{
    public class MovimientosStockHandler : GenericListaHandler<MovimientosStockQuery, MovimientosStockDto>
    {
        private const string Sql = @"
SELECT e.*
    , variante.Sku AS VarianteDisplay
    , dep.Nombre AS DepositoDisplay
    , depDest.Nombre AS DepositoDestinoDisplay
FROM PC_MOVIMIENTOS_STOCK e
LEFT JOIN PC_VARIANTES variante ON e.VarianteId = variante.Id
LEFT JOIN PC_DEPOSITOS dep ON e.DepositoId = dep.Id
LEFT JOIN PC_DEPOSITOS depDest ON e.DepositoDestinoId = depDest.Id";

        public MovimientosStockHandler(IQueryService consultas)
            : base(consultas, Sql, q => (q.Id, q.Skip, q.Take))
        {
        }
    }
}
