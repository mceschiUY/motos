using ApiMotos.Domain.Agregates.Categorias;
using ApiMotos.Domain.Common;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.Categorias;

namespace ApiMotos.Application.Agregates.Categorias.Commands.Modificar
{
    public class ModificarCategoriaHandler : GenericModificarHandler<Categoria, ModificarCategoriaCommand>
    {
        public ModificarCategoriaHandler(ICategoriaRepositorio repositorio, CategoriaHooks hooks, IEventPublisher eventos)
            : base(repositorio, hooks, eventos, comando => comando.Id,
                (actual, comando) => actual.Modificar(comando.Nombre, comando.CategoriaPadreId, comando.Activo))
        {
        }
    }
}
