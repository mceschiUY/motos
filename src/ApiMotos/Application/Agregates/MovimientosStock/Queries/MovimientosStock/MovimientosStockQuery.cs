using ApiMotos.Application.Common;

namespace ApiMotos.Application.Agregates.MovimientosStock.Queries.MovimientosStock
{
    public record MovimientosStockQuery(int? Id = null, int? Skip = null, int? Take = null) : IQuery<List<MovimientosStockDto>>;
}
