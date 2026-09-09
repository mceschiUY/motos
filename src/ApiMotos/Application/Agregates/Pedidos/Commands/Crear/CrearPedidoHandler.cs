using ApiMotos.Domain.Agregates.Pedidos;
using ApiMotos.Domain.Common;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.Pedidos;

namespace ApiMotos.Application.Agregates.Pedidos.Commands.Crear
{
    public class CrearPedidoHandler : GenericCrearHandler<Pedido, CrearPedidoCommand>
    {
        public CrearPedidoHandler(IPedidoRepositorio repositorio, PedidoHooks hooks, IEventPublisher eventos)
            : base(repositorio, hooks, eventos, comando => Pedido.Crear(
                comando.ClienteId, comando.VendedorId, comando.DepositoId, comando.AgenciaId,
                comando.Fecha ?? Clock.Current.Now, comando.Observaciones))
        {
        }
    }
}
