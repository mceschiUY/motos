using ApiMotos.Domain.Agregates.PedidoLineas;
using ApiMotos.Domain.Common;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.PedidoLineas;

namespace ApiMotos.Application.Agregates.PedidoLineas.Commands.Crear
{
    public class CrearPedidoLineaHandler : GenericCrearHandler<PedidoLinea, CrearPedidoLineaCommand>
    {
        public CrearPedidoLineaHandler(IPedidoLineaRepositorio repositorio, PedidoLineaHooks hooks, IEventPublisher eventos)
            // Precio en 0 = "usá el de lista": PedidoLineaHooks lo completa desde la variante
            // después de guardar (el armado de pedido siempre manda el precio que mostró).
            : base(repositorio, hooks, eventos, comando => PedidoLinea.Crear(
                comando.PedidoId, comando.VarianteId, comando.Cantidad, comando.PrecioUnitarioUsd ?? 0m))
        {
        }
    }
}
