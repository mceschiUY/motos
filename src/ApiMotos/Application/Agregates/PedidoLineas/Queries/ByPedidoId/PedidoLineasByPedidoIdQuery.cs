using MediatR;
using ApiMotos.Application.Agregates.PedidoLineas.Queries.PedidoLineas;

namespace ApiMotos.Application.Agregates.PedidoLineas.Queries.ByPedidoId
{
    public class PedidoLineasByPedidoIdQuery : IRequest<List<PedidoLineasDto>>
    {
        public int PedidoId { get; set; }
    }
}
