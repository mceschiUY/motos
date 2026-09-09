using ApiMotos.Domain.Agregates.Pedidos;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.Pedidos;

namespace ApiMotos.Application.Agregates.Pedidos.Commands.Eliminar
{
    public class EliminarPedidoHandler : GenericEliminarHandler<Pedido, EliminarPedidoCommand>
    {
        public EliminarPedidoHandler(IPedidoRepositorio repositorio, PedidoHooks hooks)
            : base(repositorio, hooks, comando => comando.Id)
        {
        }
    }
}
