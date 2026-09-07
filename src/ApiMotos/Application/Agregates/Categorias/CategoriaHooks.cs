using FluentResults;
using NSpecifications;
using ApiMotos.Domain.Agregates.Categorias;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.Categorias.Commands.Crear;
using ApiMotos.Application.Agregates.Categorias.Commands.Modificar;

namespace ApiMotos.Application.Agregates.Categorias
{
    public class CategoriaHooks : CrudHooks<Categoria, CrearCategoriaCommand, ModificarCategoriaCommand>
    {
        private readonly ICategoriaRepositorio _categoriaRepositorio;

        public CategoriaHooks(ICategoriaRepositorio pCategoriaRepositorio, IReglasNegocioEjecutor motor)
            : base(motor, "Categoria")
        {
            _categoriaRepositorio = pCategoriaRepositorio;
        }

        public override async Task<Result> AntesDeCrear(CrearCategoriaCommand comando, CancellationToken ct)
        {
            var repetidos = await _categoriaRepositorio.GetCategoriasAsync(
                new Spec<Categoria>(x => x.Nombre == comando.Nombre));
            if (repetidos.Count > 0)
                return Result.Fail("Ya existe un registro con el mismo valor de Nombre");
            return Result.Ok();
        }

        public override async Task<Result> AntesDeModificar(ModificarCategoriaCommand comando, Categoria actual, CancellationToken ct)
        {
            var repetidos = await _categoriaRepositorio.GetCategoriasAsync(
                new Spec<Categoria>(x => x.Nombre == comando.Nombre && x.Id != comando.Id));
            if (repetidos.Count > 0)
                return Result.Fail("Ya existe un registro con el mismo valor de Nombre");
            return Result.Ok();
        }
    }
}
