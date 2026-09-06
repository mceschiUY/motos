using ApiMotos.Domain.Agregates.Observaciones;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.Observaciones;

namespace ApiMotos.Application.Agregates.Observaciones.Commands.Eliminar
{
    public class EliminarObservacionHandler : GenericEliminarHandler<Observacion, EliminarObservacionCommand>
    {
        public EliminarObservacionHandler(IObservacionRepositorio repositorio, ObservacionHooks hooks)
            : base(repositorio, hooks, comando => comando.Id)
        {
        }
    }
}
