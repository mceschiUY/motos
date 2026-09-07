using ApiMotos.Domain.Agregates.Productos;
using ApiMotos.Domain.Common;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.Productos;

namespace ApiMotos.Application.Agregates.Productos.Commands.Crear
{
    public class CrearProductoHandler : GenericCrearHandler<Producto, CrearProductoCommand>
    {
        public CrearProductoHandler(IProductoRepositorio repositorio, ProductoHooks hooks, IEventPublisher eventos)
            : base(repositorio, hooks, eventos, comando => Producto.Crear(
                comando.Codigo, comando.Nombre, comando.MarcaId, comando.CategoriaId, comando.Descripcion,
                comando.Genero, comando.Temporada, comando.Material, comando.PesoGramos, comando.TipoCasco,
                comando.Homologacion, comando.HomologacionVigente, comando.FechaVencHomologacion, comando.Activo))
        {
        }
    }
}
