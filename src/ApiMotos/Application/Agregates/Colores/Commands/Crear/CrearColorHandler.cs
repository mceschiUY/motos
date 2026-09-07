using ApiMotos.Domain.Agregates.Colores;
using ApiMotos.Domain.Common;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.Colores;

namespace ApiMotos.Application.Agregates.Colores.Commands.Crear
{
    public class CrearColorHandler : GenericCrearHandler<Color, CrearColorCommand>
    {
        public CrearColorHandler(IColorRepositorio repositorio, ColorHooks hooks, IEventPublisher eventos)
            : base(repositorio, hooks, eventos, comando => Color.Crear(comando.Nombre, comando.CodigoHex, comando.Activo))
        {
        }
    }
}
