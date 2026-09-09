using ApiMotos.Domain.Agregates.PedidoLineas;
using ApiMotos.Domain.Common;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.PedidoLineas;

namespace ApiMotos.Application.Agregates.PedidoLineas.Commands.Modificar
{
    public class ModificarPedidoLineaHandler : GenericModificarHandler<PedidoLinea, ModificarPedidoLineaCommand>
    {
        public ModificarPedidoLineaHandler(IPedidoLineaRepositorio repositorio, PedidoLineaHooks hooks, IEventPublisher eventos)
            : base(repositorio, hooks, eventos, comando => comando.Id,
                (actual, comando) => actual.Modificar(
                    comando.PedidoId, comando.VarianteId, comando.Cantidad,
                    comando.PrecioUnitarioUsd ?? actual.PrecioUnitarioUsd))
        {
        }
    }
}
