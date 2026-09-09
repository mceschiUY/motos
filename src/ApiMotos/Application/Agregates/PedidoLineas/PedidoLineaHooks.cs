using FluentResults;
using NSpecifications;
using ApiMotos.Domain.Agregates.PedidoLineas;
using ApiMotos.Domain.Agregates.Pedidos;
using ApiMotos.Domain.Agregates.Variantes;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.PedidoLineas.Commands.Crear;
using ApiMotos.Application.Agregates.PedidoLineas.Commands.Modificar;

namespace ApiMotos.Application.Agregates.PedidoLineas
{
    /// <summary>
    /// Reglas de la línea de pedido (plan §3.5, Etapa B):
    ///   · el pedido tiene que existir y estar EDITABLE (borrador o confirmado) — despachado
    ///     ya movió stock, cambiarle las líneas dejaría el Kardex mintiendo;
    ///   · precio en 0 = "usá el de lista": se copia de la variante al guardar;
    ///   · después de cada alta, cambio o baja se RECALCULA el total del pedido.
    /// </summary>
    public class PedidoLineaHooks : CrudHooks<PedidoLinea, CrearPedidoLineaCommand, ModificarPedidoLineaCommand>
    {
        private readonly IPedidoLineaRepositorio _lineas;
        private readonly IPedidoRepositorio _pedidos;
        private readonly IVarianteRepositorio _variantes;

        public PedidoLineaHooks(IPedidoLineaRepositorio lineas, IPedidoRepositorio pedidos,
            IVarianteRepositorio variantes, IReglasNegocioEjecutor motor)
            : base(motor, "PedidoLinea")
        {
            _lineas = lineas;
            _pedidos = pedidos;
            _variantes = variantes;
        }

        public override Task<Result> AntesDeCrear(CrearPedidoLineaCommand comando, CancellationToken ct)
            => ValidarPedidoEditable(comando.PedidoId);

        public override Task<Result> AntesDeModificar(ModificarPedidoLineaCommand comando, PedidoLinea actual, CancellationToken ct)
            => ValidarPedidoEditable(comando.PedidoId);

        public override Task<Result> AntesDeEliminar(PedidoLinea entidad, CancellationToken ct)
            => ValidarPedidoEditable(entidad.PedidoId);

        public override async Task<Result> DespuesDeCrear(CrearPedidoLineaCommand comando, PedidoLinea creada, CancellationToken ct)
        {
            await CompletarPrecioDeLista(creada);
            return await RecalcularTotal(creada.PedidoId);
        }

        public override async Task<Result> DespuesDeModificar(ModificarPedidoLineaCommand comando, PedidoLinea entidad, CancellationToken ct)
        {
            await CompletarPrecioDeLista(entidad);
            return await RecalcularTotal(entidad.PedidoId);
        }

        /// <summary>El borrado ya pasó por AntesDeEliminar; el total se recalcula acá porque
        /// el handler genérico no tiene un DespuesDeEliminar.</summary>
        public async Task<Result> RecalcularTrasBaja(int pedidoId) => await RecalcularTotal(pedidoId);

        private async Task<Result> ValidarPedidoEditable(int pedidoId)
        {
            var pedido = await _pedidos.FindAsync(pedidoId);
            if (pedido == null) return Result.Fail($"El pedido {pedidoId} no existe");
            if (!pedido.EsEditable)
                return Result.Fail($"El pedido {pedido.Numero} está {pedido.Estado}: ya no se le pueden tocar las líneas");
            return Result.Ok();
        }

        /// <summary>Precio 0 ⇒ se copia el precio de lista de la variante (plan §3.5: el pedido
        /// congela el precio del momento; de ahí en más no lo vuelve a mirar).</summary>
        private async Task CompletarPrecioDeLista(PedidoLinea linea)
        {
            if (linea.PrecioUnitarioUsd > 0) return;
            var variante = await _variantes.FindAsync(linea.VarianteId);
            var precio = variante?.PrecioLista ?? 0m;
            if (precio <= 0) return;
            linea.Modificar(linea.PedidoId, linea.VarianteId, linea.Cantidad, precio);
            _lineas.Update(linea);
        }

        private async Task<Result> RecalcularTotal(int pedidoId)
        {
            var pedido = await _pedidos.FindAsync(pedidoId);
            if (pedido == null) return Result.Ok();
            var lineas = await _lineas.GetPedidoLineasAsync(new Spec<PedidoLinea>(l => l.PedidoId == pedidoId));
            pedido.RecalcularTotal(lineas.Sum(l => l.SubtotalUsd));
            _pedidos.Update(pedido);
            return Result.Ok();
        }
    }
}
