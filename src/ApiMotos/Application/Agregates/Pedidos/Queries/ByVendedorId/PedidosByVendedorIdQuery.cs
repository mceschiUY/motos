using MediatR;
using ApiMotos.Application.Agregates.Pedidos.Queries.Pedidos;

namespace ApiMotos.Application.Agregates.Pedidos.Queries.ByVendedorId
{
    public class PedidosByVendedorIdQuery : IRequest<List<PedidosDto>>
    {
        public int VendedorId { get; set; }
    }
}
