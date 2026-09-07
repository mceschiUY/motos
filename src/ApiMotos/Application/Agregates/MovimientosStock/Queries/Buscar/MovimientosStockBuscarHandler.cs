using ApiMotos.Application.Common.Abstractions;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.MovimientosStock.Queries.MovimientosStock;

namespace ApiMotos.Application.Agregates.MovimientosStock.Queries.Buscar
{
    public class MovimientosStockBuscarHandler : GenericBuscarHandler<MovimientosStockBuscarQuery, MovimientosStockDto>
    {
        private const string Sql = @"SELECT TOP 10 * FROM PC_MOVIMIENTOS_STOCK
                WHERE DocumentoOrigen LIKE @q OR Motivo LIKE @q
                ORDER BY Id DESC";

        public MovimientosStockBuscarHandler(IQueryService consultas)
            : base(consultas, Sql, q => q.Texto)
        {
        }
    }
}
