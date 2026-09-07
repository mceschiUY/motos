using FluentResults;
using NSpecifications;
using ApiMotos.Domain.Agregates.Depositos;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.Depositos.Commands.Crear;
using ApiMotos.Application.Agregates.Depositos.Commands.Modificar;

namespace ApiMotos.Application.Agregates.Depositos
{
    public class DepositoHooks : CrudHooks<Deposito, CrearDepositoCommand, ModificarDepositoCommand>
    {
        private readonly IDepositoRepositorio _depositoRepositorio;

        public DepositoHooks(IDepositoRepositorio pDepositoRepositorio, IReglasNegocioEjecutor motor)
            : base(motor, "Deposito")
        {
            _depositoRepositorio = pDepositoRepositorio;
        }

        public override async Task<Result> AntesDeCrear(CrearDepositoCommand comando, CancellationToken ct)
        {
            var repetidos = await _depositoRepositorio.GetDepositosAsync(
                new Spec<Deposito>(x => x.Codigo == comando.Codigo));
            if (repetidos.Count > 0)
                return Result.Fail("Ya existe un depósito con el mismo Codigo");
            return Result.Ok();
        }

        public override async Task<Result> AntesDeModificar(ModificarDepositoCommand comando, Deposito actual, CancellationToken ct)
        {
            var repetidos = await _depositoRepositorio.GetDepositosAsync(
                new Spec<Deposito>(x => x.Codigo == comando.Codigo && x.Id != comando.Id));
            if (repetidos.Count > 0)
                return Result.Fail("Ya existe un depósito con el mismo Codigo");
            return Result.Ok();
        }
    }
}
