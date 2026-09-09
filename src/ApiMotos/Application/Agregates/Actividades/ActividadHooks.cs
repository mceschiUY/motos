using FluentResults;
using ApiMotos.Domain.Agregates.Actividades;
using ApiMotos.Domain.Agregates.Clientes;
using ApiMotos.Domain.Agregates.Vendedores;
using ApiMotos.Domain.Agregates.Pedidos;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.Actividades.Commands.Crear;
using ApiMotos.Application.Agregates.Actividades.Commands.Modificar;

namespace ApiMotos.Application.Agregates.Actividades
{
    /// <summary>
    /// Reglas de negocio de Actividad (Etapa A). Las guardas intrínsecas (enums de tipo y
    /// resultado, Fecha requerida, reprogramar ⇒ ProximaAccion) viven en el agregado
    /// (Actividad.Validar). Acá lo cross-entity: el vendedor y el cliente deben existir.
    /// Etapa B: si el resultado es `pedido`, el PedidoId tiene que existir (el "hay que
    /// indicarlo" ya lo exige el agregado).
    /// </summary>
    public class ActividadHooks : CrudHooks<Actividad, CrearActividadCommand, ModificarActividadCommand>
    {
        private readonly IVendedorRepositorio _vendedorRepositorio;
        private readonly IClienteRepositorio _clienteRepositorio;
        private readonly IPedidoRepositorio _pedidoRepositorio;

        public ActividadHooks(IVendedorRepositorio pVendedorRepositorio, IClienteRepositorio pClienteRepositorio,
            IPedidoRepositorio pPedidoRepositorio, IReglasNegocioEjecutor motor)
            : base(motor, "Actividad")
        {
            _vendedorRepositorio = pVendedorRepositorio;
            _clienteRepositorio = pClienteRepositorio;
            _pedidoRepositorio = pPedidoRepositorio;
        }

        public override Task<Result> AntesDeCrear(CrearActividadCommand comando, CancellationToken ct)
            => ValidarReferencias(comando.VendedorId, comando.ClienteId, comando.PedidoId);

        public override Task<Result> AntesDeModificar(ModificarActividadCommand comando, Actividad actual, CancellationToken ct)
            => ValidarReferencias(comando.VendedorId, comando.ClienteId, comando.PedidoId);

        private async Task<Result> ValidarReferencias(int vendedorId, int clienteId, int? pedidoId)
        {
            if (await _vendedorRepositorio.FindAsync(vendedorId) == null)
                return Result.Fail($"El vendedor {vendedorId} no existe");
            if (await _clienteRepositorio.FindAsync(clienteId) == null)
                return Result.Fail($"El cliente {clienteId} no existe");
            if (pedidoId is > 0 && await _pedidoRepositorio.FindAsync(pedidoId.Value) == null)
                return Result.Fail($"El pedido {pedidoId} no existe");
            return Result.Ok();
        }
    }
}
