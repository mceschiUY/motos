using FluentResults;
using NSpecifications;
using ApiMotos.Domain.Agregates.Tallas;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.Tallas.Commands.Crear;
using ApiMotos.Application.Agregates.Tallas.Commands.Modificar;

namespace ApiMotos.Application.Agregates.Tallas
{
    public class TallaHooks : CrudHooks<Talla, CrearTallaCommand, ModificarTallaCommand>
    {
        private readonly ITallaRepositorio _tallaRepositorio;

        public TallaHooks(ITallaRepositorio pTallaRepositorio, IReglasNegocioEjecutor motor)
            : base(motor, "Talla")
        {
            _tallaRepositorio = pTallaRepositorio;
        }

        public override async Task<Result> AntesDeCrear(CrearTallaCommand comando, CancellationToken ct)
        {
            var repetidos = await _tallaRepositorio.GetTallasAsync(
                new Spec<Talla>(x => x.Nombre == comando.Nombre));
            if (repetidos.Count > 0)
                return Result.Fail("Ya existe un registro con el mismo valor de Nombre");
            return Result.Ok();
        }

        public override async Task<Result> AntesDeModificar(ModificarTallaCommand comando, Talla actual, CancellationToken ct)
        {
            var repetidos = await _tallaRepositorio.GetTallasAsync(
                new Spec<Talla>(x => x.Nombre == comando.Nombre && x.Id != comando.Id));
            if (repetidos.Count > 0)
                return Result.Fail("Ya existe un registro con el mismo valor de Nombre");
            return Result.Ok();
        }
    }
}
