using FluentResults;
using NSpecifications;
using ApiMotos.Domain.Agregates.Productos;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.Productos.Commands.Crear;
using ApiMotos.Application.Agregates.Productos.Commands.Modificar;

namespace ApiMotos.Application.Agregates.Productos
{
    public class ProductoHooks : CrudHooks<Producto, CrearProductoCommand, ModificarProductoCommand>
    {
        private readonly IProductoRepositorio _productoRepositorio;

        public ProductoHooks(IProductoRepositorio pProductoRepositorio, IReglasNegocioEjecutor motor)
            : base(motor, "Producto")
        {
            _productoRepositorio = pProductoRepositorio;
        }

        public override async Task<Result> AntesDeCrear(CrearProductoCommand comando, CancellationToken ct)
        {
            var repetidos = await _productoRepositorio.GetProductosAsync(
                new Spec<Producto>(x => x.Codigo == comando.Codigo));
            if (repetidos.Count > 0)
                return Result.Fail("Ya existe un registro con el mismo valor de Codigo");
            return Result.Ok();
        }

        public override async Task<Result> AntesDeModificar(ModificarProductoCommand comando, Producto actual, CancellationToken ct)
        {
            var repetidos = await _productoRepositorio.GetProductosAsync(
                new Spec<Producto>(x => x.Codigo == comando.Codigo && x.Id != comando.Id));
            if (repetidos.Count > 0)
                return Result.Fail("Ya existe un registro con el mismo valor de Codigo");
            return Result.Ok();
        }
    }
}
