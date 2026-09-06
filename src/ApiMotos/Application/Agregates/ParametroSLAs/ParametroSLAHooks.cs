using FluentResults;
using NSpecifications;
using ApiMotos.Domain.Agregates.ParametroSLAs;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.ParametroSLAs.Commands.Crear;
using ApiMotos.Application.Agregates.ParametroSLAs.Commands.Modificar;

namespace ApiMotos.Application.Agregates.ParametroSLAs
{
    /// <summary>
    /// Reglas de negocio de handler de ParametroSLA: override de AntesDeCrear / DespuesDeCrear /
    /// AntesDeModificar / DespuesDeModificar / AntesDeEliminar / AntesDeAccion / DespuesDeAccion.
    /// Acá escribe forja-reglas las R-XXX no plantillables (unicidad compuesta, efectos
    /// cross-entity). Devolver Result.Fail("mensaje") corta el flujo → 400.
    /// NO tocar los *Handler.cs (shells regenerables) ni Application/Common/Generated.
    /// </summary>
    public class ParametroSLAHooks : CrudHooks<ParametroSLA, CrearParametroSLACommand, ModificarParametroSLACommand>
    {
        private readonly IParametroSLARepositorio _parametroSLARepositorio;

        // El generador emite el ParametroSLA con esta etapa fija (ver CrearParametroSLAHandler):
        // ni CrearParametroSLACommand ni ModificarParametroSLACommand llevan el campo Etapa.
        private const string EtapaPorDefecto = "facturacion";

        public ParametroSLAHooks(IParametroSLARepositorio pParametroSLARepositorio, IReglasNegocioEjecutor motor)
            : base(motor, "ParametroSLA")
        {
            _parametroSLARepositorio = pParametroSLARepositorio;
        }

        public override async Task<Result> AntesDeCrear(CrearParametroSLACommand comando, CancellationToken ct)
        {
            // R-014: unicidad por Etapa — no pueden existir dos parametros de SLA para la misma etapa.
            // El comando no carga Etapa; el alta siempre usa la etapa fija del generador.
            var repetidosEtapa = await _parametroSLARepositorio.GetParametroSLAsAsync(
                new Spec<ParametroSLA>(x => x.Etapa == EtapaPorDefecto));
            if (repetidosEtapa.Count > 0)
                return Result.Fail("Ya existe un registro con el mismo valor de etapa: '" + EtapaPorDefecto + "'");

            return Result.Ok();
        }

        public override async Task<Result> AntesDeModificar(ModificarParametroSLACommand comando, ParametroSLA actual, CancellationToken ct)
        {
            // R-014: unicidad por Etapa al modificar, excluyendo el propio Id.
            // La modificacion no cambia la Etapa (se conserva actual.Etapa), pero se guarda
            // igual la invariante por si existieran duplicados previos.
            var repetidosEtapa = await _parametroSLARepositorio.GetParametroSLAsAsync(
                new Spec<ParametroSLA>(x => x.Etapa == actual.Etapa && x.Id != comando.Id));
            if (repetidosEtapa.Count > 0)
                return Result.Fail("Ya existe un registro con el mismo valor de etapa: '" + actual.Etapa + "'");

            return Result.Ok();
        }
    }
}
