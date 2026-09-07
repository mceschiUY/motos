using ApiMotos.Domain.Agregates.Tallas;
using ApiMotos.Domain.Common;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.Tallas;

namespace ApiMotos.Application.Agregates.Tallas.Commands.Modificar
{
    public class ModificarTallaHandler : GenericModificarHandler<Talla, ModificarTallaCommand>
    {
        public ModificarTallaHandler(ITallaRepositorio repositorio, TallaHooks hooks, IEventPublisher eventos)
            : base(repositorio, hooks, eventos, comando => comando.Id,
                (actual, comando) => actual.Modificar(comando.Nombre, comando.Tipo, comando.Orden, comando.Activo))
        {
        }
    }
}
