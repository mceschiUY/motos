using FluentResults;
using ApiMotos.Domain.Agregates.PedidoLineas;
using ApiMotos.Application.Common;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.PedidoLineas;

namespace ApiMotos.Application.Agregates.PedidoLineas.Commands.Eliminar
{
    /// <summary>
    /// Baja de línea. Envuelve el libreto genérico para recalcular el total del pedido: el
    /// handler genérico tiene AntesDeEliminar pero no un "después", y el total tiene que
    /// quedar bien una vez que la línea REALMENTE se fue.
    /// Re-declara ICommandHandler a propósito: eso REMAPEA la interfaz a este Handle, así
    /// MediatR entra acá y no en el de la base (con `new` a secas entraría en la base).
    /// </summary>
    public class EliminarPedidoLineaHandler : GenericEliminarHandler<PedidoLinea, EliminarPedidoLineaCommand>,
        ICommandHandler<EliminarPedidoLineaCommand, Result<int>>
    {
        private readonly IPedidoLineaRepositorio _lineas;
        private readonly PedidoLineaHooks _hooks;

        public EliminarPedidoLineaHandler(IPedidoLineaRepositorio repositorio, PedidoLineaHooks hooks)
            : base(repositorio, hooks, comando => comando.Id)
        {
            _lineas = repositorio;
            _hooks = hooks;
        }

        public new async Task<Result<int>> Handle(EliminarPedidoLineaCommand comando, CancellationToken cancellationToken)
        {
            var linea = await _lineas.FindAsync(comando.Id);
            var pedidoId = linea?.PedidoId ?? 0;

            var resultado = await base.Handle(comando, cancellationToken);
            if (resultado.IsSuccess && pedidoId > 0)
                await _hooks.RecalcularTrasBaja(pedidoId);

            return resultado;
        }
    }
}
