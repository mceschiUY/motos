using ApiMotos.Application.Common;
using ApiMotos.Application.Agregates.MovimientosStock.Queries.MovimientosStock;

namespace ApiMotos.Application.Agregates.MovimientosStock.Queries.Buscar
{
    public record MovimientosStockBuscarQuery(string Texto) : IQuery<List<MovimientosStockDto>>;
}
