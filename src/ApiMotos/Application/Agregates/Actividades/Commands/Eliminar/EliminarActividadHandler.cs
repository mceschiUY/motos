using ApiMotos.Domain.Agregates.Actividades;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.Actividades;

namespace ApiMotos.Application.Agregates.Actividades.Commands.Eliminar
{
    public class EliminarActividadHandler : GenericEliminarHandler<Actividad, EliminarActividadCommand>
    {
        public EliminarActividadHandler(IActividadRepositorio repositorio, ActividadHooks hooks)
            : base(repositorio, hooks, comando => comando.Id)
        {
        }
    }
}
