using ApiMotos.Domain.Agregates.Productos;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.Productos;

namespace ApiMotos.Application.Agregates.Productos.Commands.Eliminar
{
    public class EliminarProductoHandler : GenericEliminarHandler<Producto, EliminarProductoCommand>
    {
        public EliminarProductoHandler(IProductoRepositorio repositorio, ProductoHooks hooks)
            : base(repositorio, hooks, comando => comando.Id)
        {
        }
    }
}
