using FluentResults;
using ApiMotos.Domain.Common;

namespace ApiMotos.Application.Common.Generated
{
    public interface ICrearHooks<TEntity, TCommand>
    {
        Task<Result> AntesDeCrear(TCommand comando, CancellationToken ct);
        Task<Result> DespuesDeCrear(TCommand comando, TEntity creada, CancellationToken ct);
    }

    public interface IModificarHooks<TEntity, TCommand>
    {
        Task<Result> AntesDeModificar(TCommand comando, TEntity actual, CancellationToken ct);
        Task<Result> DespuesDeModificar(TCommand comando, TEntity entidad, CancellationToken ct);
    }

    public interface IEliminarHooks<TEntity>
    {
        Task<Result> AntesDeEliminar(TEntity entidad, CancellationToken ct);
    }

    public interface IAccionHooks<TEntity>
    {
        Task<Result> AntesDeAccion(string accion, TEntity entidad, CancellationToken ct);
        Task<Result> DespuesDeAccion(string accion, TEntity entidad, CancellationToken ct);
    }

    /// <summary>
    /// HOME de las reglas de negocio de handler (unicidad, integridad cross-entity, efectos
    /// post-guardado). El generador emite una subclase {Entidad}Hooks por entidad con las
    /// unicidades plantillables; forja-reglas agrega ahí las R-XXX no plantillables.
    /// Un hook que devuelve Result.Fail corta el flujo → 400 con el mensaje.
    ///
    /// El MOTOR de reglas-dato (reglas-negocio.json) corre en los wrappers EXPLÍCITOS de
    /// interfaz de los Antes*: los genéricos entran por la interfaz, así que el motor corre
    /// SIEMPRE, aunque un override (generado o de Opus) no llame a base.
    ///
    /// OJO: DespuesDe* corre DESPUÉS de persistir (sin transacción) — mismo contrato que
    /// tenían las reglas manuales post-Update.
    /// </summary>
    public abstract class CrudHooks<TEntity, TCrearCommand, TModificarCommand>
        : ICrearHooks<TEntity, TCrearCommand>,
          IModificarHooks<TEntity, TModificarCommand>,
          IEliminarHooks<TEntity>,
          IAccionHooks<TEntity>
        where TEntity : BaseEntity<int>
    {
        private readonly IReglasNegocioEjecutor _motor;
        private readonly string _entidad;

        protected CrudHooks(IReglasNegocioEjecutor motor, string entidad)
        {
            _motor = motor;
            _entidad = entidad;
        }

        // ─── Wrappers explícitos: motor primero, después la virtual (overrideable) ───

        async Task<Result> ICrearHooks<TEntity, TCrearCommand>.AntesDeCrear(TCrearCommand comando, CancellationToken ct)
        {
            var r = await _motor.Ejecutar(_entidad, "crear", comando!, idExcluir: null, ct);
            return r.IsFailed ? r : await AntesDeCrear(comando, ct);
        }

        async Task<Result> IModificarHooks<TEntity, TModificarCommand>.AntesDeModificar(TModificarCommand comando, TEntity actual, CancellationToken ct)
        {
            var r = await _motor.Ejecutar(_entidad, "modificar", comando!, idExcluir: actual.Id, ct);
            return r.IsFailed ? r : await AntesDeModificar(comando, actual, ct);
        }

        async Task<Result> IEliminarHooks<TEntity>.AntesDeEliminar(TEntity entidad, CancellationToken ct)
        {
            var r = await _motor.Ejecutar(_entidad, "eliminar", entidad, idExcluir: entidad.Id, ct);
            return r.IsFailed ? r : await AntesDeEliminar(entidad, ct);
        }

        async Task<Result> IAccionHooks<TEntity>.AntesDeAccion(string accion, TEntity entidad, CancellationToken ct)
        {
            var r = await _motor.Ejecutar(_entidad, "accion", entidad, idExcluir: entidad.Id, ct, accion);
            return r.IsFailed ? r : await AntesDeAccion(accion, entidad, ct);
        }

        // ─── Virtuales públicas: acá overridean el generador y forja-reglas ───

        public virtual Task<Result> AntesDeCrear(TCrearCommand comando, CancellationToken ct)
            => Task.FromResult(Result.Ok());

        public virtual Task<Result> DespuesDeCrear(TCrearCommand comando, TEntity creada, CancellationToken ct)
            => Task.FromResult(Result.Ok());

        public virtual Task<Result> AntesDeModificar(TModificarCommand comando, TEntity actual, CancellationToken ct)
            => Task.FromResult(Result.Ok());

        public virtual Task<Result> DespuesDeModificar(TModificarCommand comando, TEntity entidad, CancellationToken ct)
            => Task.FromResult(Result.Ok());

        public virtual Task<Result> AntesDeEliminar(TEntity entidad, CancellationToken ct)
            => Task.FromResult(Result.Ok());

        public virtual Task<Result> AntesDeAccion(string accion, TEntity entidad, CancellationToken ct)
            => Task.FromResult(Result.Ok());

        public virtual Task<Result> DespuesDeAccion(string accion, TEntity entidad, CancellationToken ct)
            => Task.FromResult(Result.Ok());
    }
}
