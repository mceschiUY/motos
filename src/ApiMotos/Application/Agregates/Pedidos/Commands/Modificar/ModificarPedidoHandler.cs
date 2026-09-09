using ApiMotos.Domain.Agregates.Pedidos;
using ApiMotos.Domain.Common;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.Pedidos;

namespace ApiMotos.Application.Agregates.Pedidos.Commands.Modificar
{
    public class ModificarPedidoHandler : GenericModificarHandler<Pedido, ModificarPedidoCommand>
    {
        public ModificarPedidoHandler(IPedidoRepositorio repositorio, PedidoHooks hooks, IEventPublisher eventos)
            : base(repositorio, hooks, eventos, comando => comando.Id,
                (actual, comando) => actual.Modificar(
                    comando.ClienteId, comando.VendedorId, comando.DepositoId, comando.AgenciaId,
                    comando.Fecha ?? actual.Fecha, comando.Observaciones))
        {
        }
    }
}
