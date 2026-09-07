using ApiMotos.Domain.Agregates.Colores;
using ApiMotos.Domain.Common;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.Colores;

namespace ApiMotos.Application.Agregates.Colores.Commands.Modificar
{
    public class ModificarColorHandler : GenericModificarHandler<Color, ModificarColorCommand>
    {
        public ModificarColorHandler(IColorRepositorio repositorio, ColorHooks hooks, IEventPublisher eventos)
            : base(repositorio, hooks, eventos, comando => comando.Id,
                (actual, comando) => actual.Modificar(comando.Nombre, comando.CodigoHex, comando.Activo))
        {
        }
    }
}
