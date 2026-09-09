using ApiMotos.Domain.Agregates.Actividades;
using ApiMotos.Domain.Common;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.Actividades;

namespace ApiMotos.Application.Agregates.Actividades.Commands.Crear
{
    public class CrearActividadHandler : GenericCrearHandler<Actividad, CrearActividadCommand>
    {
        public CrearActividadHandler(IActividadRepositorio repositorio, ActividadHooks hooks, IEventPublisher eventos)
            : base(repositorio, hooks, eventos, comando => Actividad.Crear(
                comando.VendedorId, comando.ClienteId, comando.Tipo, comando.Fecha, comando.Resultado,
                comando.Notas, comando.ProximaAccion, comando.PedidoId))
        {
        }
    }
}
