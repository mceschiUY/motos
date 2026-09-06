using FluentResults;
using ApiMotos.Domain.Agregates.Observaciones;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.Observaciones.Commands.Crear;
using ApiMotos.Application.Agregates.Observaciones.Commands.Modificar;

namespace ApiMotos.Application.Agregates.Observaciones
{
    /// <summary>
    /// Reglas de negocio de handler de Observacion: override de AntesDeCrear / DespuesDeCrear /
    /// AntesDeModificar / DespuesDeModificar / AntesDeEliminar / AntesDeAccion / DespuesDeAccion.
    /// Acá escribe forja-reglas las R-XXX no plantillables (unicidad compuesta, efectos
    /// cross-entity). Devolver Result.Fail("mensaje") corta el flujo → 400.
    /// NO tocar los *Handler.cs (shells regenerables) ni Application/Common/Generated.
    /// </summary>
    public class ObservacionHooks : CrudHooks<Observacion, CrearObservacionCommand, ModificarObservacionCommand>
    {
        private readonly IObservacionRepositorio _observacionRepositorio;

        public ObservacionHooks(IObservacionRepositorio pObservacionRepositorio, IReglasNegocioEjecutor motor)
            : base(motor, "Observacion")
        {
            _observacionRepositorio = pObservacionRepositorio;
        }
    }
}
