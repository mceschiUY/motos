using ApiMotos.Domain.Agregates.Colores;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.Colores;

namespace ApiMotos.Application.Agregates.Colores.Commands.Eliminar
{
    public class EliminarColorHandler : GenericEliminarHandler<Color, EliminarColorCommand>
    {
        public EliminarColorHandler(IColorRepositorio repositorio, ColorHooks hooks)
            : base(repositorio, hooks, comando => comando.Id)
        {
        }
    }
}
