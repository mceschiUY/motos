using MediatR;
using ApiMotos.Application.Agregates.MovimientosStock.Queries.MovimientosStock;

namespace ApiMotos.Application.Agregates.MovimientosStock.Queries.ByVarianteId
{
    public class MovimientosStockByVarianteIdQuery : IRequest<List<MovimientosStockDto>>
    {
        public int VarianteId { get; set; }
    }
}
