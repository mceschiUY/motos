using MediatR;
using ApiMotos.Application.Agregates.Pedidos.Queries.Pedidos;

namespace ApiMotos.Application.Agregates.Pedidos.Queries.ByClienteId
{
    public class PedidosByClienteIdQuery : IRequest<List<PedidosDto>>
    {
        public int ClienteId { get; set; }
    }
}
