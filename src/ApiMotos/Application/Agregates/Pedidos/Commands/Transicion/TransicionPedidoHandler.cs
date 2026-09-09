using ApiMotos.Domain.Agregates.Pedidos;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.Pedidos;

namespace ApiMotos.Application.Agregates.Pedidos.Commands.Transicion
{
    public class TransicionPedidoHandler : GenericTransicionHandler<Pedido, TransicionPedidoCommand>
    {
        public TransicionPedidoHandler(IPedidoRepositorio repositorio, PedidoHooks hooks, ICicloBanco banco,
            ApiMotos.Domain.Common.IEventPublisher eventos)
            : base(repositorio, hooks, banco, eventos, "Pedido", comando => comando.Id, comando => comando.Accion)
        {
        }
    }
}
