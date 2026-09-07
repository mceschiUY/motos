using FluentResults;
using NSpecifications;
using ApiMotos.Domain.Agregates.Colores;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.Colores.Commands.Crear;
using ApiMotos.Application.Agregates.Colores.Commands.Modificar;

namespace ApiMotos.Application.Agregates.Colores
{
    public class ColorHooks : CrudHooks<Color, CrearColorCommand, ModificarColorCommand>
    {
        private readonly IColorRepositorio _colorRepositorio;

        public ColorHooks(IColorRepositorio pColorRepositorio, IReglasNegocioEjecutor motor)
            : base(motor, "Color")
        {
            _colorRepositorio = pColorRepositorio;
        }

        public override async Task<Result> AntesDeCrear(CrearColorCommand comando, CancellationToken ct)
        {
            var repetidos = await _colorRepositorio.GetColoresAsync(
                new Spec<Color>(x => x.Nombre == comando.Nombre));
            if (repetidos.Count > 0)
                return Result.Fail("Ya existe un registro con el mismo valor de Nombre");
            return Result.Ok();
        }

        public override async Task<Result> AntesDeModificar(ModificarColorCommand comando, Color actual, CancellationToken ct)
        {
            var repetidos = await _colorRepositorio.GetColoresAsync(
                new Spec<Color>(x => x.Nombre == comando.Nombre && x.Id != comando.Id));
            if (repetidos.Count > 0)
                return Result.Fail("Ya existe un registro con el mismo valor de Nombre");
            return Result.Ok();
        }
    }
}
