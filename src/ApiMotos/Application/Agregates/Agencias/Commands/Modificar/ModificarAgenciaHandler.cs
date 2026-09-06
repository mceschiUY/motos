using ApiMotos.Domain.Agregates.Agencias;
using ApiMotos.Domain.Common;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.Agencias;

namespace ApiMotos.Application.Agregates.Agencias.Commands.Modificar
{
    public class ModificarAgenciaHandler : GenericModificarHandler<Agencia, ModificarAgenciaCommand>
    {
        public ModificarAgenciaHandler(IAgenciaRepositorio repositorio, AgenciaHooks hooks, IEventPublisher eventos)
            : base(repositorio, hooks, eventos, comando => comando.Id,
                (actual, comando) => actual.Modificar(comando.Nombre))
        {
        }
    }
}
