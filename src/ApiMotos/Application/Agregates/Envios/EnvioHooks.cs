using FluentResults;
using MediatR;
using NSpecifications;
using ApiMotos.Domain.Agregates.Envios;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.Envios.Commands.Crear;
using ApiMotos.Application.Agregates.Envios.Commands.Modificar;

namespace ApiMotos.Application.Agregates.Envios
{
    /// <summary>
    /// Reglas de negocio de handler de Envio: override de AntesDeCrear / DespuesDeCrear /
    /// AntesDeModificar / DespuesDeModificar / AntesDeEliminar / AntesDeAccion / DespuesDeAccion.
    /// Acá escribe forja-reglas las R-XXX no plantillables (unicidad compuesta, efectos
    /// cross-entity). Devolver Result.Fail("mensaje") corta el flujo → 400.
    /// NO tocar los *Handler.cs (shells regenerables) ni Application/Common/Generated.
    /// </summary>
    public class EnvioHooks : CrudHooks<Envio, CrearEnvioCommand, ModificarEnvioCommand>
    {
        private readonly IEnvioRepositorio _envioRepositorio;
        private readonly IMediator _mediator;

        public EnvioHooks(IEnvioRepositorio pEnvioRepositorio, IMediator mediator, IReglasNegocioEjecutor motor)
            : base(motor, "Envio")
        {
            _envioRepositorio = pEnvioRepositorio;
            _mediator = mediator;
        }

        public override async Task<Result> AntesDeCrear(CrearEnvioCommand comando, CancellationToken ct)
        {
            // R-003: unicidad de CodigoRastreo por Agencia — el sistema IMPIDE el duplicado
            var repetidos = await _envioRepositorio.GetEnviosAsync(
                new Spec<Envio>(x => x.CodigoRastreo == comando.CodigoRastreo && x.AgenciaId == comando.AgenciaId));
            if (repetidos.Count > 0)
                return Result.Fail("Ya existe un envío con el mismo código de rastreo en esta agencia");

            return Result.Ok();
        }

        public override async Task<Result> AntesDeModificar(ModificarEnvioCommand comando, Envio actual, CancellationToken ct)
        {
            // R-003: unicidad de CodigoRastreo por Agencia — el sistema IMPIDE el duplicado
            var repetidos = await _envioRepositorio.GetEnviosAsync(
                new Spec<Envio>(x => x.CodigoRastreo == comando.CodigoRastreo && x.AgenciaId == comando.AgenciaId && x.Id != comando.Id));
            if (repetidos.Count > 0)
                return Result.Fail("Ya existe un envío con el mismo código de rastreo en esta agencia");

            return Result.Ok();
        }

        // R-Etapa B (plan §3): la entrega del envío ARRASTRA al pedido. Sincronía en una
        // sola dirección — el pedido entregado no toca el envío, así no hay ida y vuelta.
        public override async Task<Result> DespuesDeAccion(string accion, Envio entidad, CancellationToken ct)
        {
            if (accion != "PasarAEntregado" || entidad.PedidoId is null or <= 0) return Result.Ok();

            var resultado = await _mediator.Send(
                new ApiMotos.Application.Agregates.Pedidos.Commands.Transicion.TransicionPedidoCommand(
                    entidad.PedidoId!.Value, "PasarAEntregado"), ct);
            // Si el pedido ya estaba entregado (o anulado) el ciclo lo rechaza: no es un
            // error del envío, que se entregó igual. Se ignora a propósito.
            return Result.Ok();
        }
    }
}
