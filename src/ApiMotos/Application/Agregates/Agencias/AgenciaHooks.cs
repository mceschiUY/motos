using FluentResults;
using NSpecifications;
using ApiMotos.Domain.Agregates.Agencias;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.Agencias.Commands.Crear;
using ApiMotos.Application.Agregates.Agencias.Commands.Modificar;

namespace ApiMotos.Application.Agregates.Agencias
{
    /// <summary>
    /// Reglas de negocio de handler de Agencia: override de AntesDeCrear / DespuesDeCrear /
    /// AntesDeModificar / DespuesDeModificar / AntesDeEliminar / AntesDeAccion / DespuesDeAccion.
    /// Acá escribe forja-reglas las R-XXX no plantillables (unicidad compuesta, efectos
    /// cross-entity). Devolver Result.Fail("mensaje") corta el flujo → 400.
    /// NO tocar los *Handler.cs (shells regenerables) ni Application/Common/Generated.
    /// </summary>
    public class AgenciaHooks : CrudHooks<Agencia, CrearAgenciaCommand, ModificarAgenciaCommand>
    {
        private readonly IAgenciaRepositorio _agenciaRepositorio;

        public AgenciaHooks(IAgenciaRepositorio pAgenciaRepositorio, IReglasNegocioEjecutor motor)
            : base(motor, "Agencia")
        {
            _agenciaRepositorio = pAgenciaRepositorio;
        }

        public override async Task<Result> AntesDeCrear(CrearAgenciaCommand comando, CancellationToken ct)
        {
            // R: unicidad de Nombre — el sistema IMPIDE el duplicado
            var repetidosNombre = await _agenciaRepositorio.GetAgenciasAsync(
                new Spec<Agencia>(x => x.Nombre == comando.Nombre));
            if (repetidosNombre.Count > 0)
                return Result.Fail("Ya existe un registro con el mismo valor de Nombre");

            return Result.Ok();
        }

        public override async Task<Result> AntesDeModificar(ModificarAgenciaCommand comando, Agencia actual, CancellationToken ct)
        {
            // R: unicidad de Nombre — el sistema IMPIDE el duplicado
            var repetidosNombre = await _agenciaRepositorio.GetAgenciasAsync(
                new Spec<Agencia>(x => x.Nombre == comando.Nombre && x.Id != comando.Id));
            if (repetidosNombre.Count > 0)
                return Result.Fail("Ya existe un registro con el mismo valor de Nombre");

            return Result.Ok();
        }
    }
}
