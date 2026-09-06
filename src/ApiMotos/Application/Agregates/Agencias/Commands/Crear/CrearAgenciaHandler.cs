using ApiMotos.Domain.Agregates.Agencias;
using ApiMotos.Domain.Common;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.Agencias;

namespace ApiMotos.Application.Agregates.Agencias.Commands.Crear
{
    public class CrearAgenciaHandler : GenericCrearHandler<Agencia, CrearAgenciaCommand>
    {
        public CrearAgenciaHandler(IAgenciaRepositorio repositorio, AgenciaHooks hooks, IEventPublisher eventos)
            : base(repositorio, hooks, eventos, comando => Agencia.Crear(comando.Nombre))
        {
        }
    }
}
