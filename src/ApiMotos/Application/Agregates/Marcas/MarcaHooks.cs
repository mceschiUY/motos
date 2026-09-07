using FluentResults;
using NSpecifications;
using ApiMotos.Domain.Agregates.Marcas;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.Marcas.Commands.Crear;
using ApiMotos.Application.Agregates.Marcas.Commands.Modificar;

namespace ApiMotos.Application.Agregates.Marcas
{
    public class MarcaHooks : CrudHooks<Marca, CrearMarcaCommand, ModificarMarcaCommand>
    {
        private readonly IMarcaRepositorio _marcaRepositorio;

        public MarcaHooks(IMarcaRepositorio pMarcaRepositorio, IReglasNegocioEjecutor motor)
            : base(motor, "Marca")
        {
            _marcaRepositorio = pMarcaRepositorio;
        }

        public override async Task<Result> AntesDeCrear(CrearMarcaCommand comando, CancellationToken ct)
        {
            var repetidos = await _marcaRepositorio.GetMarcasAsync(
                new Spec<Marca>(x => x.Nombre == comando.Nombre));
            if (repetidos.Count > 0)
                return Result.Fail("Ya existe un registro con el mismo valor de Nombre");
            return Result.Ok();
        }

        public override async Task<Result> AntesDeModificar(ModificarMarcaCommand comando, Marca actual, CancellationToken ct)
        {
            var repetidos = await _marcaRepositorio.GetMarcasAsync(
                new Spec<Marca>(x => x.Nombre == comando.Nombre && x.Id != comando.Id));
            if (repetidos.Count > 0)
                return Result.Fail("Ya existe un registro con el mismo valor de Nombre");
            return Result.Ok();
        }
    }
}
