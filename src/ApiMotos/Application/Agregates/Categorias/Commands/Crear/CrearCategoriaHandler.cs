using ApiMotos.Domain.Agregates.Categorias;
using ApiMotos.Domain.Common;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.Categorias;

namespace ApiMotos.Application.Agregates.Categorias.Commands.Crear
{
    public class CrearCategoriaHandler : GenericCrearHandler<Categoria, CrearCategoriaCommand>
    {
        public CrearCategoriaHandler(ICategoriaRepositorio repositorio, CategoriaHooks hooks, IEventPublisher eventos)
            : base(repositorio, hooks, eventos, comando => Categoria.Crear(comando.Nombre, comando.CategoriaPadreId, comando.Activo))
        {
        }
    }
}
