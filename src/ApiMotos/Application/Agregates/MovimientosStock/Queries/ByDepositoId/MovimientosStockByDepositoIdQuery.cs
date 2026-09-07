using MediatR;
using ApiMotos.Application.Agregates.MovimientosStock.Queries.MovimientosStock;

namespace ApiMotos.Application.Agregates.MovimientosStock.Queries.ByDepositoId
{
    public class MovimientosStockByDepositoIdQuery : IRequest<List<MovimientosStockDto>>
    {
        public int DepositoId { get; set; }
    }
}
