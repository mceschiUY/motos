using ApiMotos.Domain.Agregates.Actividades;
using ApiMotos.Domain.Common;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.Actividades;

namespace ApiMotos.Application.Agregates.Actividades.Commands.Modificar
{
    public class ModificarActividadHandler : GenericModificarHandler<Actividad, ModificarActividadCommand>
    {
        public ModificarActividadHandler(IActividadRepositorio repositorio, ActividadHooks hooks, IEventPublisher eventos)
            : base(repositorio, hooks, eventos, comando => comando.Id,
                (actual, comando) => actual.Modificar(
                    comando.VendedorId, comando.ClienteId, comando.Tipo, comando.Fecha, comando.Resultado,
                    comando.Notas, comando.ProximaAccion, comando.PedidoId))
        {
        }
    }
}
