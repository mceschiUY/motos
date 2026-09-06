using ApiMotos.Domain.Agregates.Observaciones;
using ApiMotos.Domain.Common;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.Observaciones;

namespace ApiMotos.Application.Agregates.Observaciones.Commands.Crear
{
    public class CrearObservacionHandler : GenericCrearHandler<Observacion, CrearObservacionCommand>
    {
        public CrearObservacionHandler(IObservacionRepositorio repositorio, ObservacionHooks hooks, IEventPublisher eventos)
            : base(repositorio, hooks, eventos, comando => Observacion.Crear(comando.Texto, comando.FechaHora, comando.Usuario, comando.EnvioId))
        {
        }
    }
}
