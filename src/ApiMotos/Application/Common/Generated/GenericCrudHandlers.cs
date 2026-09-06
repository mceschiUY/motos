using FluentResults;
using ApiMotos.Domain.Common;

namespace ApiMotos.Application.Common.Generated
{
    /// <summary>
    /// Libreto CRUD genérico (una sola vez): factory de dominio → hooks → guardar → FK-catch.
    /// Los shells generados por entidad aportan solo los delegates (llamada a la factory con
    /// los campos del comando) y la clase de hooks. Los mensajes de error son CONTRATO
    /// (el front y los tests los comparan): no cambiarlos.
    /// </summary>
    public abstract class GenericCrearHandler<TEntity, TCommand> : ICommandHandler<TCommand, Result<int>>
        where TEntity : BaseEntity<int>
        where TCommand : ICommand<Result<int>>
    {
        private readonly IRepository<TEntity, int> _repositorio;
        private readonly ICrearHooks<TEntity, TCommand> _hooks;
        private readonly Func<TCommand, Result<TEntity>> _crear;
        private readonly IEventPublisher _eventos;

        protected GenericCrearHandler(IRepository<TEntity, int> repositorio,
            ICrearHooks<TEntity, TCommand> hooks, IEventPublisher eventos,
            Func<TCommand, Result<TEntity>> crear)
        {
            _repositorio = repositorio;
            _hooks = hooks;
            _eventos = eventos;
            _crear = crear;
        }

        public async Task<Result<int>> Handle(TCommand comando, CancellationToken cancellationToken)
        {
            Result<TEntity> entidad = _crear(comando);

            if (entidad.IsFailed)
                return Result.Fail<int>(entidad.Errors);

            var antes = await _hooks.AntesDeCrear(comando, cancellationToken);
            if (antes.IsFailed)
                return Result.Fail<int>(antes.Errors);

            TEntity creada;
            try
            {
                creada = await _repositorio.AddAsync(entidad.Value);
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException)
            {
                // FK constraint: alguna referencia apunta a un registro inexistente — 400, no 500.
                return Result.Fail<int>("No se pudo guardar: alguna referencia no existe (verificá los datos relacionados)");
            }

            var despues = await _hooks.DespuesDeCrear(comando, creada, cancellationToken);
            if (despues.IsFailed)
                return Result.Fail<int>(despues.Errors);

            // Eventos de dominio (Ola 3): lo que el agregado emitió durante Crear se publica
            // DESPUÉS de persistir y de los hooks — reacciones de cascada in-process.
            await _eventos.PublicarAsync(creada.TomarEventos(), cancellationToken);

            return Result.Ok(creada.Id);
        }
    }

    public abstract class GenericModificarHandler<TEntity, TCommand> : ICommandHandler<TCommand, Result<int>>
        where TEntity : BaseEntity<int>
        where TCommand : ICommand<Result<int>>
    {
        private readonly IRepository<TEntity, int> _repositorio;
        private readonly IModificarHooks<TEntity, TCommand> _hooks;
        private readonly Func<TCommand, int> _id;
        private readonly Func<TEntity, TCommand, Result<TEntity>> _modificar;
        private readonly IEventPublisher _eventos;

        protected GenericModificarHandler(IRepository<TEntity, int> repositorio,
            IModificarHooks<TEntity, TCommand> hooks, IEventPublisher eventos,
            Func<TCommand, int> id, Func<TEntity, TCommand, Result<TEntity>> modificar)
        {
            _repositorio = repositorio;
            _hooks = hooks;
            _eventos = eventos;
            _id = id;
            _modificar = modificar;
        }

        public async Task<Result<int>> Handle(TCommand comando, CancellationToken cancellationToken)
        {
            TEntity? actual = await _repositorio.FindAsync(_id(comando));

            if (actual == null)
                return Result.Fail<int>("No se encontró el registro");

            var antes = await _hooks.AntesDeModificar(comando, actual, cancellationToken);
            if (antes.IsFailed)
                return Result.Fail<int>(antes.Errors);

            // Las guardas del dominio (requerido/rango) también aplican al modificar
            Result<TEntity> modificado = _modificar(actual, comando);
            if (modificado.IsFailed)
                return Result.Fail<int>(modificado.Errors);

            try
            {
                _repositorio.Update(actual);
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException)
            {
                // FK constraint: alguna referencia apunta a un registro inexistente — 400, no 500.
                return Result.Fail<int>("No se pudo guardar: alguna referencia no existe (verificá los datos relacionados)");
            }

            var despues = await _hooks.DespuesDeModificar(comando, actual, cancellationToken);
            if (despues.IsFailed)
                return Result.Fail<int>(despues.Errors);

            // Eventos de dominio (Ola 3): publicados después de persistir y de los hooks.
            await _eventos.PublicarAsync(actual.TomarEventos(), cancellationToken);

            return Result.Ok(actual.Id);
        }
    }

    public abstract class GenericEliminarHandler<TEntity, TCommand> : ICommandHandler<TCommand, Result<int>>
        where TEntity : BaseEntity<int>
        where TCommand : ICommand<Result<int>>
    {
        private readonly IRepository<TEntity, int> _repositorio;
        private readonly IEliminarHooks<TEntity> _hooks;
        private readonly Func<TCommand, int> _id;

        protected GenericEliminarHandler(IRepository<TEntity, int> repositorio,
            IEliminarHooks<TEntity> hooks, Func<TCommand, int> id)
        {
            _repositorio = repositorio;
            _hooks = hooks;
            _id = id;
        }

        public async Task<Result<int>> Handle(TCommand comando, CancellationToken cancellationToken)
        {
            TEntity? aEliminar = await _repositorio.FindAsync(_id(comando));

            if (aEliminar == null)
                return Result.Fail<int>("No se encontró el registro");

            var antes = await _hooks.AntesDeEliminar(aEliminar, cancellationToken);
            if (antes.IsFailed)
                return Result.Fail<int>(antes.Errors);

            try
            {
                _repositorio.Delete(aEliminar);
                return Result.Ok(aEliminar.Id);
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException)
            {
                // FK constraint: hay registros que dependen de este — 400 legible, no 500.
                return Result.Fail<int>("No se puede eliminar: tiene registros asociados");
            }
        }
    }

    public abstract class GenericAccionHandler<TEntity, TCommand> : ICommandHandler<TCommand, Result<bool>>
        where TEntity : BaseEntity<int>
        where TCommand : ICommand<Result<bool>>
    {
        private readonly IRepository<TEntity, int> _repositorio;
        private readonly IAccionHooks<TEntity> _hooks;
        private readonly string _accion;
        private readonly Func<TCommand, int> _id;
        private readonly Func<TEntity, Result<TEntity>> _ejecutar;
        private readonly IEventPublisher _eventos;

        protected GenericAccionHandler(IRepository<TEntity, int> repositorio,
            IAccionHooks<TEntity> hooks, IEventPublisher eventos, string accion,
            Func<TCommand, int> id, Func<TEntity, Result<TEntity>> ejecutar)
        {
            _repositorio = repositorio;
            _hooks = hooks;
            _eventos = eventos;
            _accion = accion;
            _id = id;
            _ejecutar = ejecutar;
        }

        public async Task<Result<bool>> Handle(TCommand comando, CancellationToken cancellationToken)
        {
            TEntity? entidad = await _repositorio.FindAsync(_id(comando));

            if (entidad == null)
                return Result.Fail<bool>("No se encontró el registro");

            var antes = await _hooks.AntesDeAccion(_accion, entidad, cancellationToken);
            if (antes.IsFailed)
                return Result.Fail<bool>(antes.Errors);

            var result = _ejecutar(entidad);
            if (result.IsFailed)
                return Result.Fail<bool>(result.Errors);

            _repositorio.Update(entidad);

            var despues = await _hooks.DespuesDeAccion(_accion, entidad, cancellationToken);
            if (despues.IsFailed)
                return Result.Fail<bool>(despues.Errors);

            // Eventos de dominio (Ola 3): publicados después de persistir y de los hooks.
            await _eventos.PublicarAsync(entidad.TomarEventos(), cancellationToken);

            return Result.Ok(true);
        }
    }
}
