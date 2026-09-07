using ApiMotos.Domain.Agregates.Categorias;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.Categorias;

namespace ApiMotos.Application.Agregates.Categorias.Commands.Eliminar
{
    public class EliminarCategoriaHandler : GenericEliminarHandler<Categoria, EliminarCategoriaCommand>
    {
        public EliminarCategoriaHandler(ICategoriaRepositorio repositorio, CategoriaHooks hooks)
            : base(repositorio, hooks, comando => comando.Id)
        {
        }
    }
}
