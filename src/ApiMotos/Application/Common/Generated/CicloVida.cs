using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;
using FluentResults;
using ApiMotos.Domain.Common;

namespace ApiMotos.Application.Common.Generated
{
    // ─── Ciclos de vida como DATOS ───────────────────────────────────────────
    // El generador emite ciclos-vida.json (por entidad: campo de estado + matriz de
    // transiciones con nombres YA RESUELTOS) y este motor lo interpreta en runtime.
    // Antes la matriz se estampaba como código: un método pasarAXxx en la entidad +
    // un Command + un Handler POR TRANSICIÓN. Ahora hay UN handler por entidad
    // (GenericTransicionHandler) y la matriz vive acá — cambiar el ciclo es tocar
    // la spec y regenerar el JSON, sin tocar código.
    // La autorización por transición NO vive acá: la resuelve el motor de reglas
    // (autorizacion_rol v2 con perilla `accion`) vía IAccionHooks.AntesDeAccion.

    public class CiclosVidaArchivo
    {
        public int Version { get; set; } = 1;
        public string? Universo { get; set; }
        public List<CicloEntidadData> Entidades { get; set; } = new();
    }

    public class CicloEntidadData
    {
        public string Entidad { get; set; } = "";
        /// <summary>Propiedad C# del estado (Pascal, ej. "Estado").</summary>
        public string Campo { get; set; } = "";
        public List<CicloTransicionData> Transiciones { get; set; } = new();
    }

    public class CicloTransicionData
    {
        /// <summary>Nombre Pascal de la acción (ej. "PasarAAprobada") — es el que matchean
        /// las reglas autorizacion_rol v2 (perilla accion) y los overrides de hooks.</summary>
        public string Accion { get; set; } = "";
        /// <summary>Segmento kebab de la ruta HTTP (ej. "pasar-a-aprobada") — el contrato
        /// con el frontend y los tests: POST /api/Entidad/{id}/{ruta}.</summary>
        public string Ruta { get; set; } = "";
        /// <summary>Propiedad C# del estado (Pascal). El banco la completa desde el nivel
        /// entidad al cargar si el generador no la emitió por transición.</summary>
        public string Campo { get; set; } = "";
        /// <summary>Estado destino, normalizado (minúsculas, guiones bajos).</summary>
        public string Hacia { get; set; } = "";
        /// <summary>Estados origen permitidos, normalizados. Vacío = sin guarda de origen.</summary>
        public List<string> Desde { get; set; } = new();
    }

    public interface ICicloBanco
    {
        /// <summary>Busca la transición por nombre de acción O por ruta kebab.</summary>
        CicloTransicionData? Buscar(string entidad, string rutaOAccion);
    }

    public class CicloBanco : ICicloBanco
    {
        private readonly Dictionary<string, CicloEntidadData> _porEntidad = new(StringComparer.OrdinalIgnoreCase);

        public CicloBanco(Microsoft.Extensions.Hosting.IHostEnvironment env,
            Microsoft.Extensions.Logging.ILogger<CicloBanco> logger)
        {
            var path = Path.Combine(env.ContentRootPath, "ciclos-vida.json");
            if (!File.Exists(path))
            {
                Microsoft.Extensions.Logging.LoggerExtensions.LogInformation(
                    logger, "[CicloVida] sin ciclos-vida.json: banco vacío");
                return;
            }

            try
            {
                var archivo = JsonSerializer.Deserialize<CiclosVidaArchivo>(File.ReadAllText(path),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                foreach (var e in archivo?.Entidades ?? new List<CicloEntidadData>())
                {
                    if (string.IsNullOrWhiteSpace(e.Entidad) || string.IsNullOrWhiteSpace(e.Campo))
                        continue;
                    // Denormalizar: cada transición conoce su campo (el handler no vuelve al nivel entidad).
                    foreach (var t in e.Transiciones)
                        if (string.IsNullOrWhiteSpace(t.Campo))
                            t.Campo = e.Campo;
                    _porEntidad[e.Entidad] = e;
                }
            }
            catch (Exception ex)
            {
                Microsoft.Extensions.Logging.LoggerExtensions.LogError(
                    logger, ex, "[CicloVida] ciclos-vida.json ilegible: banco vacío");
            }
        }

        public CicloTransicionData? Buscar(string entidad, string rutaOAccion)
        {
            if (!_porEntidad.TryGetValue(entidad, out var ciclo)) return null;
            return ciclo.Transiciones.FirstOrDefault(x =>
                x.Ruta.Equals(rutaOAccion, StringComparison.OrdinalIgnoreCase) ||
                x.Accion.Equals(rutaOAccion, StringComparison.OrdinalIgnoreCase));
        }
    }

    /// <summary>
    /// UN handler de transiciones POR ENTIDAD (reemplaza al par Command+Handler POR
    /// TRANSICIÓN). Libreto: cargar → hooks AntesDeAccion (motor de reglas incluido:
    /// autorizacion_rol v2 matchea el nombre Pascal) → validar la matriz del banco
    /// (dato) → asignar el estado → guardar → DespuesDeAccion.
    /// Mensajes de rechazo = CONTRATO con tests y front: no cambiarlos.
    /// </summary>
    public abstract class GenericTransicionHandler<TEntity, TCommand> : ICommandHandler<TCommand, Result<bool>>
        where TEntity : BaseEntity<int>
        where TCommand : ICommand<Result<bool>>
    {
        private static readonly ConcurrentDictionary<string, PropertyInfo?> PropCache = new();

        private readonly IRepository<TEntity, int> _repositorio;
        private readonly IAccionHooks<TEntity> _hooks;
        private readonly ICicloBanco _banco;
        private readonly IEventPublisher _eventos;
        private readonly string _entidad;
        private readonly Func<TCommand, int> _id;
        private readonly Func<TCommand, string> _accion;

        protected GenericTransicionHandler(IRepository<TEntity, int> repositorio,
            IAccionHooks<TEntity> hooks, ICicloBanco banco, IEventPublisher eventos,
            string entidad, Func<TCommand, int> id, Func<TCommand, string> accion)
        {
            _repositorio = repositorio;
            _hooks = hooks;
            _banco = banco;
            _eventos = eventos;
            _entidad = entidad;
            _id = id;
            _accion = accion;
        }

        public async Task<Result<bool>> Handle(TCommand comando, CancellationToken cancellationToken)
        {
            var transicion = _banco.Buscar(_entidad, _accion(comando));
            if (transicion is null)
                return Result.Fail<bool>($"Acción desconocida para {_entidad}: {_accion(comando)}");

            TEntity? entidad = await _repositorio.FindAsync(_id(comando));
            if (entidad == null)
                return Result.Fail<bool>("No se encontró el registro");

            // Hooks primero (motor de reglas-dato incluido — autorización por transición).
            var antes = await _hooks.AntesDeAccion(transicion.Accion, entidad, cancellationToken);
            if (antes.IsFailed)
                return Result.Fail<bool>(antes.Errors);

            var campo = PropCache.GetOrAdd($"{typeof(TEntity).FullName}.{transicion.Campo}",
                _ => typeof(TEntity).GetProperty(transicion.Campo,
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase));
            if (campo is null || campo.PropertyType != typeof(string))
                return Result.Fail<bool>($"La entidad {_entidad} no tiene el campo de estado declarado en el ciclo");

            // La matriz del ciclo como DATO: mismo normalizado y mismo mensaje que la
            // guarda que antes se estampaba en el método pasarAXxx de la entidad.
            var estadoActual = (campo.GetValue(entidad) as string ?? string.Empty)
                .Trim().ToLowerInvariant().Replace(" ", "_");
            if (transicion.Desde.Count > 0 && !transicion.Desde.Contains(estadoActual))
                return Result.Fail<bool>(
                    $"No se puede pasar a '{transicion.Hacia}' desde '{campo.GetValue(entidad)}'");

            campo.SetValue(entidad, transicion.Hacia);
            _repositorio.Update(entidad);

            var despues = await _hooks.DespuesDeAccion(transicion.Accion, entidad, cancellationToken);
            if (despues.IsFailed)
                return Result.Fail<bool>(despues.Errors);

            // Eventos de dominio (Ola 3): toda transición exitosa emite TransicionRealizada
            // — el gatillo determinístico de las cascadas ("al devolver, la máquina vuelve a
            // disponible"): la reacción se suscribe por INotificationHandler<DomainEvent-
            // Notification<TransicionRealizada>> y filtra por Entidad + Accion. Además se
            // publican los eventos que el agregado haya emitido él mismo.
            await _eventos.PublicarAsync(new TransicionRealizada(
                _entidad, entidad.Id, transicion.Accion, estadoActual, transicion.Hacia), cancellationToken);
            await _eventos.PublicarAsync(entidad.TomarEventos(), cancellationToken);

            return Result.Ok(true);
        }
    }
}
