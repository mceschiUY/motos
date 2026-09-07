using ApiMotos.Domain.Agregates.Productos;
using ApiMotos.Domain.Common;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.Productos;

namespace ApiMotos.Application.Agregates.Productos.Commands.Modificar
{
    public class ModificarProductoHandler : GenericModificarHandler<Producto, ModificarProductoCommand>
    {
        public ModificarProductoHandler(IProductoRepositorio repositorio, ProductoHooks hooks, IEventPublisher eventos)
            : base(repositorio, hooks, eventos, comando => comando.Id,
                (actual, comando) => actual.Modificar(
                    comando.Codigo, comando.Nombre, comando.MarcaId, comando.CategoriaId, comando.Descripcion,
                    comando.Genero, comando.Temporada, comando.Material, comando.PesoGramos, comando.TipoCasco,
                    comando.Homologacion, comando.HomologacionVigente, comando.FechaVencHomologacion, comando.Activo))
        {
        }
    }
}
