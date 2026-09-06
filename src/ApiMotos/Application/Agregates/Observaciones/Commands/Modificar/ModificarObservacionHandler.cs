using ApiMotos.Domain.Agregates.Observaciones;
using ApiMotos.Domain.Common;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.Observaciones;

namespace ApiMotos.Application.Agregates.Observaciones.Commands.Modificar
{
    public class ModificarObservacionHandler : GenericModificarHandler<Observacion, ModificarObservacionCommand>
    {
        public ModificarObservacionHandler(IObservacionRepositorio repositorio, ObservacionHooks hooks, IEventPublisher eventos)
            : base(repositorio, hooks, eventos, comando => comando.Id,
                (actual, comando) => actual.Modificar(comando.Texto, comando.FechaHora, comando.Usuario, comando.EnvioId))
        {
        }
    }
}
